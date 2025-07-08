using System;
using System.Threading;
using System.Threading.Tasks;
using WfComponent.Base;

namespace NanoTools2.Process
{
    public abstract class CallBase
    {
        protected IProcess process;
        protected string binaryDirectory;
        protected CancellationTokenSource cancellationTokenSource;
        protected CancellationToken token;
        protected IProgress<string> log;
        public CallBase(string binaryDirectory, IProgress<string> progres)
        {
            if (string.IsNullOrEmpty(binaryDirectory))
                this.binaryDirectory = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
            else
                this.binaryDirectory = binaryDirectory;

            var info = new System.IO.DirectoryInfo(this.binaryDirectory);
            if(! info.Exists)
                this.binaryDirectory = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');

            // 共通処理
            cancellationTokenSource = new CancellationTokenSource();
            this.token = cancellationTokenSource.Token;
            this.log = progres;
        }

        /**
         * 
         * 非同期処理TASK 
         * 正常に終了 -> ContinueWith で 作成Token などの後始末
         * 強制Cancel -> 現在のProcess ID が削除されるので、自信を含めた後始末

        public static string processEndMessage = "";
        protected async Task<string> ExternalProcessStart()
        {
            // 設定チェック
            if(!isEnable)
                return "not setting parameters.";

            var returnMessage = processEndMessage;
            CancellationToken token = cancellationTokenSource.Token;
            try
            {
                await Task.Factory.StartNew(() =>
                {
                    process.StartProcess();

                }, token).ContinueWith(t =>
                {
                    cancellationTokenSource.Dispose();
                    cancellationTokenSource = null;
                    if (process != null)
                    {
                        System.Diagnostics.Debug.WriteLine("ContinueWith end.");
                    }
                },
                TaskContinuationOptions.OnlyOnRanToCompletion);
            }
            catch (Exception e)
            {
                returnMessage = "ProcessCall2 Broken.\n" + e.Message;
            }
            return returnMessage;
        }
        */

        public static string processEndMessage = "";
        public static string processCancelMessage = "process is already canceled.";
        internal async Task<string> ExternalProcessStart()
        {
            // 設定チェック
            if (!isEnable) return "not setting parameters.";

            var mesage = processEndMessage;
            try { 
                await Task.Factory.StartNew(() =>
            {
                process.StartProcess();
            }, token);

            if (token.IsCancellationRequested)
                return "process is already canceled."; 
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Process exception. \n" + e.Message);
                mesage = e.Message;
            }
            return mesage;
        }

        /**
         * 基本Processとは別に、エラーの有無関係ないProcessを実行する
         * 結果のサマリー表示やIGV
         * */
        internal async Task<string> OtherProcess(WfComponent.External.BaseProcess process)
        {
            var mesage = processEndMessage;
            try
            {
                await Task.Factory.StartNew(() =>
                {
                    process.StartProcess();
                }, token);

                if (token.IsCancellationRequested)
                    return "process is already canceled.";
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Process exception. \n" + e.Message);
                mesage = e.Message;
            }
            return mesage;

        }

        // force cansel.
        public string CancelProcess()
        {
            System.Diagnostics.Debug.WriteLine("CancelProcess called! (force cancel)");
            log.Report("CancelProcess (force cancel)");

            this.cancellationTokenSource.Cancel();
            var res = string.Empty;
            if (this.process != null)
            {
                res = process.StopProcess(); // process.kill
                process = null;
            }
            return res;
        }

        protected bool isEnable = false;
        public bool IsProcessEnable() => isEnable;

    }
}
