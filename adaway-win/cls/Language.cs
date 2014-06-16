using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    class Language
    {

        #region Declaration
        //Declaration



        #endregion

        #region Properties
        //Properties

        //LanguageCode
        public string LanguageCode
        { get; set; }

        //Application Path
        public string AppPath
        { get; set; }

        //Button $activated
        public string btn_activated
        { get; set; }

        //Button $restore
        public string btn_restore
        { get; set; }

        //Button $settings
        public string btn_settings
        { get; set; }

        //Button $exit
        public string btn_exit
        { get; set; }

        //StatusBar value started
        public string stats_value_start
        { get; set; }

        //StatusBar value activ
        public string stats_value_activ
        { get; set; }

        //StatusBar Value
        public string stats_value
        { get; set; }

        //StatusBar value finished
        public string stats_value_finished
        { get; set; }

        //ExMessage cant open RegKey
        public string Msg_regkey
        { get; set; }

        //Hosts FileNotFound
        public string Msg_HostsNotFound
        { get; set; }

        //Description 
        public string Des_HostsError
        { get; set; }

        //failed to Create Directory AppPath\Hosts\
        public string Msg_FailCreateDir
        { get; set; }
        
        //Description
        public string Des_FailCreateDir
        { get; set; }

        //failed to Create Downloaded Hosts.txt
        public string Msg_FailWriteHost
        { get; set; }

        //Description
        public string Des_FailWriteHost
        { get; set; }

        #endregion

        #region Construct
        //Construct

        public Language(string Language)
        {
            this.LanguageCode = Language;

            if (!String.IsNullOrEmpty(this.LanguageCode) && File.Exists(this.AppPath + "\\lang_" + this.LanguageCode))
            {

            }
            else
            {
                this.LoadStandard();
            }
        }

        #endregion

        #region Load Standard English
        //Load Standard English

        private void LoadStandard()
        {
            //Buttons
            this.btn_activated = "Activate";
            this.btn_restore = "Restore";
            this.btn_settings = "Settings";
            this.btn_exit = "Exit";

            //Messages
            this.Msg_regkey = "Cant get Path from Registry";

            this.Msg_HostsNotFound = "Cant find Hosts File";
            this.Des_HostsError = "Locate Hosts File, an Error occured";

            this.Msg_FailCreateDir = "failed to create Directory AppPath\\Hosts\\";
            this.Des_FailCreateDir = "Cant create Directory!";

            this.Msg_FailWriteHost = "failed to create temp hosts.txt";
            this.Des_FailWriteHost = "Cant write hosts.txt";

            //StatusBar
            this.stats_value = "Hosts Files loaded...";
            this.stats_value_start = "Generate Data...";
            this.stats_value_activ = "Create File Hosts.txt...";
            this.stats_value_finished = "done...";

        }

        #endregion

    }
}
