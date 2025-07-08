using NanoTools2.Utils;
using Prism.Regions;
using System;
using System.IO;

namespace NanoTools2.ViewModels.Base
{
    abstract partial class ViewModelBase : ViewModelNotifyDataError, IRegionMemberLifetime, INavigationAware
    {
        // public IRegionManager RegionManager { get; set; }
        public IRegionManager _regionManager;
        public bool KeepAlive { get; set; } = true;

        protected IProgress<string> mainLog;
        protected NavigationContext navigationContext;
        protected NavigationParameters navigationParameters;

        public ViewModelBase(IRegionManager regionManager)
        {
            _regionManager = regionManager;
        }

        // View画面の読み込まれる直前（前画面からの表示処理）
        protected abstract void OnloadViewImplement();
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            // 画面遷移時に遷移先の画面処理 (前画面の設定値を取得)
            this.navigationContext = navigationContext;
            this.mainLog = (IProgress<string>)navigationContext.Parameters[ConstantValues.MainLog];


            OnloadViewImplement();
            // Console.WriteLine("OnNavigatedTo");
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            // 取り敢えず
            this.navigationContext = navigationContext;
            return true;
        }

        // 画面遷移時に元画面から遷移する直前
        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            this.navigationContext = navigationContext;
            // this.paramName = (string)navigationContext.Parameters[ConstantValues.PassParamName];
            // Console.WriteLine("OnNavigatedFrom");
        }

        // 遷移先の決定
        protected void NextView(string nextView)
        {
            this.navigationParameters = navigationParameters ??
                                                     new NavigationParameters();
            navigationParameters.Add(ConstantValues.MainLog, this.mainLog);
            _regionManager.RequestNavigate(ConstantValues.MainRegion, nextView, navigationParameters);
        }

        protected string savedir;
        protected string SaveDir
        {
            get
            {
                // database recent save dir
                savedir = savedir ?? Path.Combine(@"C:\Users", Environment.UserName, "Documents");
                return savedir;
            }
            set { savedir = value; }
        }

        protected string selectdir;
        protected string SelectDir
        {
            get
            {
                // database recent select dir
                selectdir = selectdir ?? Path.Combine(@"C:\Users", Environment.UserName, "Documents");
                return selectdir;
            }
            set { selectdir = value; }
        }

    }
}
