using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Windows;

namespace Core
{
    class DownloadHosts
    {

        #region Declaration
        //Declaration

        //TODO: Remove Clone Class
        private Core.Language _Language = new Language("de");//memory issue

        //List of Hosts Sources
        List<string> listSources = new List<string>();

        //Local AppPath
        private string sAppPath = String.Empty;

        //Threat count
        private int iCount = 0;

        #endregion

        #region Properties
        //Properties

        //Add New Host Source
        public string AddNewSource
        {
            set
            {
                if (value.Remove(7, value.Length).ToLower().Contains("http://") || value.Remove(8, value.Length).ToLower().Contains("https://"))
                {
                    listSources.Add(value);
                }
            }
        }

        #endregion

        #region Add Events
        //Add Events

        public delegate void EventHandler();

        //Helper
        public event EventHandler ProgressChanged;
        public event EventHandler Completed;

        #endregion

        #region Event Handler
        //Event Handler

        private void SetProgressChanged()
        {
            if (ProgressChanged != null)
            {
                ProgressChanged();
            }
        }

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

        public DownloadHosts(string AppPath)
        {
            this.listSources.Add("http://adaway.org/hosts.txt");
            this.listSources.Add("http://adblock.gjtech.net/?format=hostfile");
            this.listSources.Add("http://hosts-file.net/ad_servers.asp");
            this.listSources.Add("http://pgl.yoyo.org/adservers/serverlist.php?hostformat=hosts&showintro=0&mimetype=plaintext");
            this.listSources.Add("http://winhelp2002.mvps.org/hosts.txt");

            this.sAppPath = AppPath + "\\Hosts\\";

            if (!Directory.Exists(this.sAppPath))
            {
                try
                {
                    Directory.CreateDirectory(this.sAppPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(_Language.Msg_FailCreateDir + "\n" + ex.Message, _Language.Des_FailCreateDir, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            this.iCount = this.listSources.Count;

            //TODO: better limit Threads
            //for each Source in List create an Thread
            for (int i = 0; i < this.listSources.Count; i++)
            {
                AsyncRequest(i);
            }

            this.AwaitCompleted();
        }

        #endregion

        #region Get Host Files
        //Get Host Files

        //Get Host Files Async
        private async void AsyncRequest(int i)
        {
            string sBuffer = String.Empty;

            //Download String Async
            sBuffer = await GetResult(this.listSources[i]);

            //write result in a file
            try
            {
                using (StreamWriter swBuffer = new StreamWriter(String.Format(this.sAppPath + "hosts.{0}", i)))
                {
                    swBuffer.Write(sBuffer);
                    swBuffer.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(_Language.Msg_FailWriteHost + "\n" + ex.Message, _Language.Des_FailWriteHost, MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            this.iCount -= 1;
            this.SetProgressChanged();
        }

        private async Task<string> GetResult(string sHostsUrl)
        {
            string sBuffer = String.Empty;

            //create WebClient and GetResult as String
            using (WebClient wcBuffer = new WebClient())
            {
                sBuffer = await wcBuffer.DownloadStringTaskAsync(sHostsUrl);
            }

            return sBuffer;
        }

        #endregion

        #region Await Completed
        //Await Completed

        private async void AwaitCompleted()
        {
            while (this.iCount > 0)
            {
                await Task.Delay(100);
            }

            this.SetCompleted();
        }

        #endregion

    }
}
