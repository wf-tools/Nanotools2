using NanoTools2.Process;
using NanoTools2.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace NanoTools2.ViewModels
{
    partial class AnalysesViewModel
    {

        // Sequencer string.
        private const string illumina = "illumina";
        private const string nanopore = "nanopore";
        private const string pacbio = "pacbio";

        private string referenceName;
        public string ReferenceName
        {
            get { return referenceName; }
            set { if (SetProperty(ref referenceName, value)) IsParameterSetup(); }
        }

        private List<string> fastqNames;
        public List<string> FastqNames
        {
            get { return fastqNames; }
            set { if( SetProperty(ref fastqNames, value)) IsParameterSetup(); }
        }

        private string mappingOptionText;
        public string MappingOptionText
        {
            get { return mappingOptionText; }
            set { SetProperty(ref mappingOptionText, value); }
        }

        private string mappingButtonLabel = ButtonLabelExecute;
        public string MappingButtonLabel
        {
            get { return mappingButtonLabel; }
            set { SetProperty(ref mappingButtonLabel, value); }
        }

        private bool isMappingEnable = enable;
        public bool IsMappingEnable
        {
            get { return isMappingEnable; }
            set { SetProperty(ref isMappingEnable, value); }
        }

        // Sequencer Select.
        private bool isIllumina;
        private bool isNanopore;
        private bool isPacbio;

        public bool IsIllumina
        {
            get { return isIllumina; }
            set
            {
                if (SetProperty(ref isIllumina, value))
                {
                    if (isIllumina) CheckSequencerSelect(illumina);
                }
            }
        }

        public bool IsNanopore
        {
            get { return isNanopore; }
            set
            {
                if (SetProperty(ref isNanopore, value))
                {
                    if (isNanopore) CheckSequencerSelect(nanopore);
                }
            }
        }

        public bool IsPacbio
        {
            get { return isPacbio; }
            set
            {
                if (SetProperty(ref isPacbio, value))
                {
                    if (isPacbio) CheckSequencerSelect(pacbio);
                }
            }
        }

        private void CheckSequencerSelect(string sequencer)
        {
            switch (sequencer)
            {
                case illumina:
                    IsNanopore = false;
                    IsPacbio = false;
                    break;
                case nanopore:
                    IsIllumina = false;
                    IsPacbio = false;
                    break;
                case pacbio:
                    IsIllumina = false;
                    IsNanopore = false;
                    break;
            }
            IsParameterSetup();
        }

        private string mappingToolTip = MessageValues.notYetSetting;
        public string MappingToolTip
        {
            get { return mappingToolTip; }
            set { SetProperty(ref mappingToolTip, value); }
        }

        private IEnumerable<string> FastqFullPaths;  // mapping に渡す方
        private void SelectFastqs()
        {
            var files = SelectFiles(
                                    "Select Fastq Files",
                                    "すべてのファイル|*.*|fastq ファイル|*.fastq|gz ファイル|*.gz",
                                    2,
                                    true);
            if(files != null && files.Any())
            {
                FastqNames = files.Select(s => Path.GetFileName(s)).ToList();
                FastqFullPaths = files;
            }
        }

        // Mapping の オプション表示。 
        public void MappingOption()
        {
            System.Diagnostics.Process.Start("https://lh3.github.io/minimap2/minimap2.html");
        }

        public void CallMapping()
        {
            // 状態チェック
            switch (mappingButtonLabel)
            {
                case ButtonLabelExecute:
                    Properties.Settings.Default.SelecedTab = MappingAnalysis;
                    Properties.Settings.Default.Save();
                    MappingStart();
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

        internal void MappingStart() {
            // パラメータの精査
            IsParameterSetup();

            // 必須チェック
            if (mappingToolTip != MessageValues.enableSetting) return;
            if (!IsSetSaveDir()) return;  // save dir setting.

            var errorMessage = string.Empty;
            System.Diagnostics.Debug.WriteLine("## call mapping. ##");

            var preset = WfComponent.External.Properties.Minimap2Options.Ont;
            if (isIllumina) preset = WfComponent.External.Properties.Minimap2Options.sr;
            if (isPacbio) preset = WfComponent.External.Properties.Minimap2Options.Pb;

            var outSam = Path.Combine(
                                    savedir,
                                    Path.GetFileNameWithoutExtension(fastqNames.First()) + ".sam");

            var options = new WfComponent.External.Properties.Minimap2Options()
            {
                Reference = this.referenceName,
                QueryFastqs  = this.FastqFullPaths,
                binaryPath = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\'), // 実行ファイルの binディレクトリの中
                Preset = preset,
                OutFile = outSam
            };
            AnalysisExecute(new CallMinimap2(options, mainLog));
        }
    }
}
