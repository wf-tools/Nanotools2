using NanoTools2.Utils;
using NanoTools2.ViewModels.Base;
using NanoTools2.Views;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Windows;

namespace NanoTools2.ViewModels
{
    class MainWindowViewModel : ViewModelBase
    {

        public DelegateCommand MainWindowLoadedCommand { get; }

        private string _title = "NanoTools2";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private string initLogFileName = "InitStartError.log";

        public MainWindowViewModel(IRegionManager regionManager) : base(regionManager)
        {
            System.Diagnostics.Debug.WriteLine("## main window view init.");

            // View読み込み時コマンド。
            this.MainWindowLoadedCommand = new DelegateCommand(MainWindowInitialize);
        }

        void MainWindowInitialize()
        {
            // license
            if (Utils.EnvInfo.IsLicenceInvalid())
            {
                System.Threading.Thread.Sleep(1000);

                var mes = string.Empty;
                WfComponent.Utils.FileUtils.WriteFileFromString(
                                System.IO.Path.Combine(
                                    System.AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\'), initLogFileName),
                                WfComponent.Utils.FileUtils.UniqueDateString() + " license invalid... ",
                                ref mes);
                Application.Current.Shutdown();
            }

            // WSL
            if (Utils.WslImportCheck.IsNoInitialSettings())
            {
                System.Threading.Thread.Sleep(5000);

                var mes = string.Empty;
                WfComponent.Utils.FileUtils.WriteFileFromString(
                                System.IO.Path.Combine(
                                    System.AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\'), initLogFileName),
                                   WfComponent.Utils.FileUtils.UniqueDateString() + "Not install WSL/Linux tools, Please check,  WfComponentInstaller.... ",
                                ref mes);

                Application.Current.Shutdown();
            }

            // ----- init- check clear ----- //

            this.navigationParameters = new NavigationParameters();
            this.mainLog = new Progress<string>(OnLogAppend);
            navigationParameters.Add(ConstantValues.MainLog, mainLog);
            _regionManager.RequestNavigate(ConstantValues.MainRegion, nameof(AnalysesView), navigationParameters);
        }

        private void OnLogAppend(string log)
        {

            if (string.IsNullOrEmpty(log)) return;
            if (log.Equals(ConstantValues.MainLogClear))
            {
                LogText = string.Empty;
                return;
            }
            log = log.EndsWith(Environment.NewLine) ? 
                    log :
                    log + Environment.NewLine;
            LogText += log;
        }


        protected override void OnloadViewImplement()
        {

            System.Diagnostics.Debug.WriteLine("Main view model init.");
        }

        private string logText;
        public string LogText
        {
            get { return logText; }
            set { SetProperty(ref logText, value); }
        }

        public string AppDate
        {
            get { return Properties.Settings.Default.UseConfig; }
        }
    }
}
