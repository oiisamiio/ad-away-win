using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace adaway_win
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region Initialization
        //Initialization

        public MainWindow()
        {
            InitializeComponent();

            //AddHandler MainWindow Loaded
            this.Loaded += MainWindow_Loaded;

            //AddHandler Buttons
            this.btnActivated.IsEnabled = false;
            this.btnActivated.Click += btnActivated_Click;
            this.btnRestore.Click += btnRestore_Click;
            this.btnSettings.Click += btnSettings_Click;
            this.btnExit.Click += btnExit_Click;

            this._DownloadHosts.ProgressChanged += DownloadHosts_ProgressChanged;
            this._DownloadHosts.Completed += DownloadHosts_Completed;

            this._ProcessHosts.ProgressChanged += ProcessHosts_ProgressChanged;
            this._ProcessHosts.Completed += ProcessHosts_Completed;
        }

        #endregion

        #region Declaration
        //Declaration

        Core.Language _Language = new Core.Language("eng");

        Core.DownloadHosts _DownloadHosts = new Core.DownloadHosts(AppDomain.CurrentDomain.BaseDirectory);
        Core.ProcessHosts _ProcessHosts = new Core.ProcessHosts(AppDomain.CurrentDomain.BaseDirectory);

        string sAppPath = AppDomain.CurrentDomain.BaseDirectory;

        int iProgressCount = 0;

        #endregion

        #region MainWindow - Loaded
        //MainWindow - Loaded

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //Language Settings Controls
            this.btnActivated.Content = this._Language.btn_activated;
            this.btnRestore.Content = this._Language.btn_restore;
            this.btnSettings.Content = this._Language.btn_settings;
            this.btnExit.Content = this._Language.btn_exit;
        }

        #endregion

        #region Hosts - Intialisation
        //Hosts - Intialisation

        private bool Hosts_Init()
        {
            if (String.IsNullOrEmpty(_ProcessHosts.MessageBuffer) && _ProcessHosts.HostsIsValid())
            {
                //dummy
            }
            else
            {
                MessageBox.Show(_ProcessHosts.MessageBuffer, _Language.Des_HostsError);
            }

            return true;
        }

        #endregion

        #region DownloadHosts - ProgressChanged
        //DownloadHosts - ProgressChanged

        private void DownloadHosts_ProgressChanged()
        {
            this.iProgressCount += 1;
            this.statItemValue.Text = String.Format("{0} {1}", this.iProgressCount, this._Language.stats_value);
        }

        #endregion

        #region DownloadHosts - Completed
        //DownloadHosts - Completed

        private void DownloadHosts_Completed()
        {
            this.btnActivated.IsEnabled = true;
        }

        #endregion

        private void ProcessHosts_ProgressChanged(object sender, int Progress)
        {
            switch (Progress)
            {
                case 1://Process started
                    this.statItemValue.Text = this._Language.stats_value_activ;                    
                    break;
                case 2://Processed Hosts
                    this.statItemValue.Text = String.Format("{0} Hosts added, {1} Duplicate Hosts removed", this._ProcessHosts.HostsCount, this._ProcessHosts.HostsRemoved);
                    this.dgMain.DataContext = this._ProcessHosts.TableHosts.DefaultView;
                    break;
                case 3://Create File
                    this.statItemValue.Text = this._Language.stats_value_finished;
                    break;
                default:
                    break;
            }
        }

        private void ProcessHosts_Completed()
        {
            throw new NotImplementedException();
        }

        #region Controls - Activited
        //Controls - Activited

        private async void btnActivated_Click(object sender, RoutedEventArgs e)
        {
            this.btnActivated.IsEnabled = false;
            await _ProcessHosts.LoadHosts();
        }

        #endregion

        #region Controls - Restore
        //Controls - Restore

        private void btnRestore_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Controls - Settings
        //Controls - Settings

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Controls - Exit
        //Controls - Exit

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion

    }
}
