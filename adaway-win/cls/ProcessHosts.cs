using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Data;

namespace Core
{
    class ProcessHosts : IDisposable
    {

        #region Declaration
        //Declaration

        private bool disposed = false;

        //TODO: Remove Clone Class
        private Core.Language _Language = new Language("eng");//memory issue

        private string sAppPath = String.Empty;

        #endregion

        #region Properties

        //Hosts File Sys Path
        public string HostsFilePath
        { get; set; }

        //Hosts File Absolute Path
        public string HostsFile
        { get; set; }

        private DataTable dtHosts = new DataTable("Hosts");
        /// <summary>
        /// DataTable Hosts
        /// </summary>
        public DataTable TableHosts
        {
            get { return dtHosts; }
            set { dtHosts = value; }
        }

        private string sRouteAdress = "127.0.0.1";
        /// <summary>
        /// Route Adress
        /// </summary>
        public string RouteAdress
        {
            set { this.sRouteAdress = value; }
        }

        private int iHostsCount;
        /// <summary>
        /// Hosts Count
        /// </summary>
        public int HostsCount
        {
            get { return this.iHostsCount; }
        }

        private int iHostsRemoved;
        /// <summary>
        /// Removed Hosts Count
        /// </summary>
        public int HostsRemoved
        {
            get { return this.iHostsRemoved; }
        }

        private string sMessageBuffer;
        /// <summary>
        /// Message Buffer
        /// </summary>
        public string MessageBuffer
        {
            get { return this.sMessageBuffer; }
            set { this.sMessageBuffer = value; }
        }

        #endregion

        #region Event Handler
        //Event Handler

        public delegate void EventHandler();

        public event EventHandler<int> ProgressChanged;
        //Report Progress Changed
        private void SetProgressChanged(int Progress)
        {
            if (ProgressChanged != null)
            {
                ProgressChanged(this, Progress);
            }
        }

        public event EventHandler Completed;
        //Report Complete
        private void SetCompleted()
        {
            if (Completed != null)
            {
                Completed();
            }
        }

        #endregion

        #region Construct
        //Construct

        public ProcessHosts(string AppPath)
        {
            try
            {
                this.sAppPath = AppPath;

                //Get Hosts File Path from Registry
                RegistryKey rkSubKey = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters\\");

                if (rkSubKey != null)
                {
                    HostsFilePath = rkSubKey.GetValue("DataBasePath").ToString();
                }

                if (HostsFilePath == null || String.IsNullOrEmpty(HostsFilePath))
                {
                    this.sMessageBuffer = _Language.Msg_regkey;
                    return;
                }

                HostsFile = HostsFilePath + "\\hosts";

                //Check Hosts File Exists
                if (!File.Exists(HostsFile))
                {
                    this.sMessageBuffer = _Language.Msg_HostsNotFound;
                    return;
                }

                //Check Backup Dir
                if (!Directory.Exists(AppPath + "\\Backup\\"))
                {
                    Directory.CreateDirectory(AppPath + "\\Backup\\");
                }

                //Check Backup
                if (!File.Exists(AppPath + "\\Backup\\hosts"))
                {
                    File.Copy(HostsFile, AppPath + "\\Backup\\hosts");
                }
            }
            //catch File Exceptions
            catch (IOException ex)
            {
                this.sMessageBuffer = ex.Message;
            }
            //catch General Exceptions
            catch (Exception ex)
            {
                this.sMessageBuffer = ex.Message;
            }

            this.InitDataTable(this.dtHosts);
        }

        #endregion

        #region Init DataTable
        //Init DataTable

        private void InitDataTable(DataTable dtTemp)
        {
            //primary key
            dtTemp.Columns.Add("id");
            dtTemp.Columns[0].AutoIncrement = true;
            dtTemp.Columns[0].Unique = true;
            dtTemp.Columns[0].DataType = typeof(Int32);

            //value
            dtTemp.Columns.Add("value");
            dtTemp.Columns[1].DataType = typeof(string);
            //workaround for Rows.Find
            dtTemp.PrimaryKey = new DataColumn[] { dtTemp.Columns[1] };
        }

        #endregion

        #region Load Hosts Files
        //Load Hosts Files

        public async Task LoadHosts()
        {
            this.SetProgressChanged(1);

            this.dtHosts = await Task.Run(() => AsyncAddHosts());

            this.SetProgressChanged(2);

            //Route IP Adress valid
            if (!this.RouteAdressValid())
            {
                this.sRouteAdress = "127.0.0.1";
            }
            this.CreateHostsTxt();

            this.SetProgressChanged(3);
        }

        #endregion

        #region Add Hosts to List
        //Add Hosts to List

        private DataTable AsyncAddHosts()
        {
            //get hosts
            string[] sBuffer = Directory.GetFiles(this.sAppPath + "Hosts\\", "hosts*");
            string sReadBuffer = String.Empty;

            using (DataTable dtBuffer = new DataTable("Buffer"))
            {
                this.InitDataTable(dtBuffer);
                //loop hosts.txt and add Data
                for (int i = 0; i < sBuffer.Length; i++)
                {
                    using (StreamReader srBuffer = new StreamReader(sBuffer[i]))
                    {
                        while (!srBuffer.EndOfStream)
                        {
                            sReadBuffer = srBuffer.ReadLine();
                            //ignore comments
                            if (!String.IsNullOrEmpty(sReadBuffer) && !sReadBuffer.Contains('#'))
                            {
                                if (sReadBuffer.Contains("\t"))
                                {
                                    sReadBuffer = sReadBuffer.Replace("\t", "");
                                }

                                if (sReadBuffer.Contains(" "))
                                {
                                    sReadBuffer = sReadBuffer.Replace(" ", "");
                                }

                                if (sReadBuffer.Contains("0.0.0.0"))
                                {
                                    sReadBuffer = sReadBuffer.Replace("0.0.0.0", "");
                                }

                                if (sReadBuffer.Contains("127.0.0.1"))
                                {
                                    sReadBuffer = sReadBuffer.Replace("127.0.0.1", "");
                                }

                                //check for duplicate entrys
                                DataRow drFind = null;

                                drFind = dtBuffer.Rows.Find(sReadBuffer);

                                if (drFind == null)
                                {
                                    //remove locals
                                    if (!(sReadBuffer.Contains("localhost") || sReadBuffer.Contains("::1") || sReadBuffer.Contains("127.0.0.1") || String.IsNullOrWhiteSpace(sReadBuffer)))
                                    {
                                        dtBuffer.Rows.Add(null, sReadBuffer);
                                        this.iHostsCount += 1;
                                    }
                                }
                                else
                                {
                                    this.iHostsRemoved += 1;
                                }
                            }
                        }
                        srBuffer.Close();
                    }
                }
                return dtBuffer;
            }
        }

        #endregion

        #region Create Hosts File
        //Create Hosts File

        private void CreateHostsTxt()
        {
            using (StreamWriter swOutput = new StreamWriter(this.sAppPath + "hosts.txt"))
            {
                swOutput.WriteLine("127.0.0.1 localhost");
                swOutput.WriteLine("::1 localhost");
                for (int i = 0; i < this.dtHosts.Rows.Count; i++)
                {//write Column 1
                    swOutput.WriteLine(this.sRouteAdress + " " + this.dtHosts.Rows[i][1].ToString());
                }
            }
        }

        #endregion

        #region Host File Validation
        //Hoste File Validation

        public bool HostsIsValid()
        {
            return File.Exists(HostsFile);
        }

        #endregion

        #region RouteAdress Validation
        //RouteAdress Validation

        private bool RouteAdressValid()
        {
            try
            {
                string[] sBuffer = this.sRouteAdress.Split('.');

                if (sBuffer.Length != 4)
                {
                    return false;
                }

                byte iBuffer = 0;

                //try parse ip adress 0-254
                for (int i = 0; i < 4; i++)
                {
                    if (!byte.TryParse(sBuffer[i], out iBuffer))
                    {
                        return false;
                    }
                }

                //ip adress valid
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region Disposable
        //Disposable

        // implement idisposable

        /// <summary>
        /// Release unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    this.dtHosts.Dispose();
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.

                // Note disposing has been done.
                disposed = true;

            }
        }

        #endregion

    }
}
