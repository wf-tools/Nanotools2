using NanoTools2.Process;
using NanoTools2.Process.Properties;
using NanoTools2.Utils;
using System;

namespace NanoTools2.ViewModels
{
    partial class AnalysesViewModel
    {
        // sequencer button は、Mapping と共有
        // fastq は Mapping と共有


        private string assembleOptionText = string.Empty;
        public string AssembleOptionText
        {
            get { return assembleOptionText; }
            set { if(SetProperty(ref assembleOptionText, value)) IsParameterSetup(); }
        }

        private string assembleCycleText = Process.CallAssemble.cycleno;
        public string AssembleCycleText
        {
            get { return assembleCycleText; }
            set { if (SetProperty(ref assembleCycleText, value)) IsParameterSetup(); }
        }

        private string assembleButtonLabel = ButtonLabelExecute;
        public string AssembleButtonLabel
        {
            get { return assembleButtonLabel; }
            set { SetProperty(ref assembleButtonLabel, value); }
        }

        private bool isAssembleEnable = enable;
        public bool IsAssembleEnable
        {
            get { return isAssembleEnable; }
            set { SetProperty(ref isAssembleEnable, value); }
        }

        private string assembleToolTip = MessageValues.notYetSetting;
        public string AssembleToolTip
        {
            get { return assembleToolTip; }
            set { SetProperty(ref assembleToolTip, value); }
        }

        // Mapping の オプション表示。 
        public void AssembleCycle()
        {
            System.Diagnostics.Process.Start("https://github.com/lh3/miniasm");
        }


        public void CallAssemble()
        {
            // 状態チェック
            switch (assembleButtonLabel)
            {
                case ButtonLabelExecute:
                    Properties.Settings.Default.SelecedTab = AssembleAnalysis;
                    Properties.Settings.Default.Save();
                    AssembleStart();
                    break;

                case ButtonLabelCancel:
                    ProcessCancel();
                    break;

                case ButtonLabelExit:
                    break;

                case ButtonLabelClear:
                    ParamClear();
                    break;
            }
        }


        internal void AssembleStart()
        {
            // 必須チェック
            IsParameterSetup();
            if (assembleToolTip != MessageValues.enableSetting) return;
            if (!IsSetSaveDir()) return;  // save dir setting.

            var errorMessage = string.Empty;
            mainLog.Report("## call assemble. ##");
            System.Diagnostics.Debug.WriteLine("## call assemble. ##");

            var preset = WfComponent.External.Properties.Minimap2Options.avaont;
            if (isPacbio) preset = WfComponent.External.Properties.Minimap2Options.avapb;

            var options = new CallAssembleOptions()
            {
                preset = preset,
                fastqs = this.FastqFullPaths,
                binaryPath = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\'), // 実行ファイルの binディレクトリの中
                outDir = SaveDir,
                cycleNo = this.assembleCycleText
            };
            AnalysisExecute(new CallAssemble(options, mainLog));
        }
    }
}
