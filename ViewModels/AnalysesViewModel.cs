using NanoTools2.Process;
using NanoTools2.Utils;
using NanoTools2.ViewModels.Base;
using Prism.Commands;
using Prism.Regions;
using System.Threading.Tasks;
using System.Windows;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NanoTools2.ViewModels
{
    partial class AnalysesViewModel : ViewModelBase
    {
        // constractor....
        public AnalysesViewModel(IRegionManager regionManager) : base(regionManager)
        {

            // Main->Analyses 移動時にチェックへ変更
            if (EnvInfo.IsDupliStart())
                Application.Current.Shutdown();

            // convert.
            GuppySlectCommand = new DelegateCommand(SelectGuppyPath);
            Fast5SelectCommand = new DelegateCommand(SelectFast5Directories);
            ConvertOptionCommand = new DelegateCommand(ConvertOption);
            ConvertCommand = new DelegateCommand(CallConvert);
            DefaultGuppyPath();

            // mapping 
            ReferenceSlectCommand = new DelegateCommand(SelectReference);
            FastqSelectCommand = new DelegateCommand(SelectFastqs);
            MappingExecuteCommand = new DelegateCommand(CallMapping);
            MappingOptionCommand = new DelegateCommand(MappingOption);

            // assemble
            // AssembleOptionCommand = new DelegateCommand(MappingOption);
            AssembleExecuteCommand = new DelegateCommand(CallAssemble);
            AssembleCycleCommand = new DelegateCommand(AssembleCycle);

            // variant
            SVExecuteCommand = new DelegateCommand(CallVariation);
            this.variantMinQualOption = minQualityHightValue.ToString(); // Hight Quarity mapping, default value.

            OpenInfomationCommand = new DelegateCommand(OpenInfoHta);

            // init tab
            InitSelectTab();

            // log clear
            DeleteLogs();
        }

        private void IsParameterSetup()
        {
            if (!string.IsNullOrEmpty(referenceName) &&
                fastqNames != null &&
                fastqNames.Any() &&
                (isIllumina || isNanopore || isPacbio))
                MappingToolTip = MessageValues.enableSetting;
            else
                MappingToolTip = MessageValues.notYetSetting;

            var assembleOptionMatch = Regex.Match(AssembleOptionText, "^[1-9]+[\\.|0-9]*[0-9|k|m|g]$", RegexOptions.IgnoreCase);
            if (fastqNames != null &&
                fastqNames.Any() &&
                assembleOptionMatch.Success &&
                (isNanopore || isPacbio))
                AssembleToolTip = MessageValues.enableSetting;
            else
                AssembleToolTip = MessageValues.notYetSetting;

            var variantOptionMatch = Regex.Match(variantMinQualOption, "^[1-9][0-9]*$", RegexOptions.IgnoreCase);
            if (!string.IsNullOrEmpty(referenceName) &&
                fastqNames != null &&
                fastqNames.Any() &&
                variantOptionMatch.Success &&
                (isNanopore || isPacbio) &&
                (isNgmlr || isMinimap2))
                variationToolTip = MessageValues.enableSetting;
            else
                variationToolTip = MessageValues.notYetSetting;
        }

        private void AnalysisExecute(ICallProcess proc)
        {
            if (! proc.IsProcessEnable() ){
                System.Diagnostics.Debug.WriteLine("no executable process.");
                var result = MessageBox.Show(
                                    "not execute process,\nPlease, chck log.", "process error.");

                mainLog.Report("process error : not execute process,\nPlease, chck log.");
                return;
            }
            if (string.IsNullOrEmpty(savedir)) return;  // 選択キャンセル
                                                        // IsEnableExecute = false;

            // input check は 前段階で済み。   TODO　実行中の POP を出す様に。
            ButtonChange(ButtonLabelCancel);
            var res = ProcessAsync(proc);
        }

        // 各Process の 非同期処理
        private string ProcessResultMessage;
        internal async Task<string> ProcessAsync(ICallProcess proc)
        {
            // Log puts
            mainLog.Report("\nstart analysis. ");
            mainLog.Report(WfComponent.Utils.FileUtils.LogDateString());

            // await
            this.execProc = proc;
            ProcessResultMessage = await proc.CallProcessAsync();

            mainLog.Report(ProcessResultMessage);

            ProcessEnd();
            return ProcessResultMessage; 
        }

        // 通常は此処に戻る。
        internal void ProcessEnd()
        {
            // 作業終了
            mainLog.Report("\nend analysis. ");
            mainLog.Report(WfComponent.Utils.FileUtils.LogDateString());

            this.IsProgress = disnable;

            // 全部のパラメータリセット？
            if (this.execProc != null) execProc = null;  // 必要？
            ButtonChange(ButtonLabelClear);
            CopyLogs();

            // 処理終了時の動作。ポップだす。
            var result = MessageBox.Show("process end." + System.Environment.NewLine +
                                                            this.ProcessResultMessage,
                                                            "process end.");

            System.Diagnostics.Process.Start(savedir);  // 終了時に出力フォルダを表示する
        }


        ICallProcess execProc;
        internal void ProcessCancel()
        {
            if (this.execProc == null)
            {
                System.Diagnostics.Debug.WriteLine("process cancel was requested... already canceled?");
                return;
            }

            // force cancel !!!!
            var res = this.execProc.CancelProcess();
            // button .
            ButtonChange(ButtonLabelClear);

            // empty なら、既に計算は終了済み。そのまま返す。
            if (string.IsNullOrEmpty(res)) return;


            CopyLogs();   // Cancel までのログをコピーする
            var result = MessageBox.Show(
                                    "process canceled.\n" + res,
                                    "warning !");
        }

        internal void ParamClear()
        {
            if (Fast5Dirs != null) Fast5Dirs = null;
            if (FastqNames != null) FastqNames = null;
            if (FastqFullPaths != null) FastqFullPaths = new List<string>();

            // Button change.
            ButtonChange(ButtonLabelExecute);

            // toolstips
            MappingToolTip = MessageValues.notYetSetting;
            ConvertToolTip = MessageValues.notYetSetting;
            VariationToolTip = MessageValues.notYetSetting;

            mainLog.Report(ConstantValues.MainLogClear);
        }

        internal void ButtonChange (string label)
        {
            // 全てのボタンを "label" へ 変更
            ConvertButtonLabel = label;
            MappingButtonLabel = label;
            AssembleButtonLabel = label;
            SVButtonLabel = label;

            // Executeの時はprogressだす　その他はクリア
            if (ButtonLabelCancel.Equals(label))
            {
                System.Diagnostics.Process.Start("taskmgr"); // タスクマネージャ起動
                this.IsProgress = enable;
            }
            else
            {
                this.IsProgress = disnable;
            }
        }


        protected void CopyLogs()
        {
            var logDir = System.IO.Path.Combine(
                                        System.AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\'),
                                        WfComponent.Utils.ConstantValues.CurrentLogDir);
            var logs = System.IO.Directory.GetFiles(logDir, "*.log");
            if (!logs.Any()) return;

            var outLog = System.IO.Path.Combine(
                                        this.savedir,
                                        "execute-" + System.IO.Path.GetFileName( logs.First()));

            var message = string.Empty;
            foreach (var log in logs)
            {
                var logLines = WfComponent.Utils.FileUtils.ReadFile(log, ref message);
                if (!string.IsNullOrEmpty(message))
                {
                    mainLog.Report(message);
                    return;
                }

                WfComponent.Utils.FileUtils.WriteFile(outLog, logLines, ref message, true);
                if (!string.IsNullOrEmpty(message))
                {
                    mainLog.Report(message);
                    return;
                }
                // アプリのログにも表示
                mainLog.Report(string.Join(System.Environment.NewLine, logLines));
            }

        }

        protected void DeleteLogs()
        {
            var logDir = System.IO.Path.Combine(
                            System.AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\'),
                            WfComponent.Utils.ConstantValues.CurrentLogDir);
            var logs = System.IO.Directory.GetFiles(logDir, "*.log");
            if (!logs.Any()) return;

            try {  
                foreach (var log in logs)
                    System.IO.File.Delete(log);
            }
            catch
            {
                mainLog.Report("Error,  log file delete fail." );
            }
            return;
        }

        private void OpenInfoHta()
        {
            // template file
            var binDir = System.AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
            var hta = WfComponent.CommandUtils.FindProgramFile(binDir, "pcinfo.hta", true, false);

            var mes = string.Empty;
            var htaLine = string.Join("\n", WfComponent.Utils.FileUtils.ReadFile(hta, ref mes));
            if (!string.IsNullOrEmpty(mes))
            {
                mainLog.Report(hta + " " + mes);
                return;
            }
            htaLine = htaLine.Replace("%OS%", Utils.EnvInfo.os_product_name + " " + Utils.EnvInfo.os_edition);
            htaLine = htaLine.Replace("%BUILD%", Utils.EnvInfo.os_release);

            htaLine = htaLine.Replace("%MAC%", System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces().First().GetPhysicalAddress().ToString().ToLower());

            var outHta = System.IO.Path.Combine(binDir, "pcinfo.hta");
            WfComponent.Utils.FileUtils.WriteFileFromString(hta, htaLine, ref mes);
            if (!string.IsNullOrEmpty(mes)) return;


            System.Diagnostics.Process.Start(hta); // hta起動

        }

        protected override void OnloadViewImplement()
        {
            // 
            System.Diagnostics.Debug.WriteLine("Analysis view model init.");
        }
    }
}
