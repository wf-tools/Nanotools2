using NanoTools2.Process;
using NanoTools2.Process.Properties;
using NanoTools2.Utils;

namespace NanoTools2.ViewModels
{
    partial class AnalysesViewModel
    {

        // mapping string.
        private const string ngmlr = "ngmlr";
        private const string minimap2 = "minimap2";

        public int minQualityHightValue = 30;
        private string variantMinQualOption = string.Empty;
        public string VariantMinQualOption
        {
            get { return variantMinQualOption; }
            set { SetProperty(ref variantMinQualOption, value); }
        }

        private string sVButtonLabel = ButtonLabelExecute;
        public string SVButtonLabel
        {
            get { return sVButtonLabel; }
            set { if (SetProperty(ref sVButtonLabel, value)) IsParameterSetup(); }
        }

        private string variationToolTip = MessageValues.notYetSetting;
        public string VariationToolTip
        {
            get { return variationToolTip; }
            set { SetProperty(ref variationToolTip, value); }
        }


        private bool isSVEnable = enable;
        public bool IsSVEnable
        {
            get { return isSVEnable; }
            set { SetProperty(ref isSVEnable, value); }
        }

        // Sequencer Select.
        private bool isNgmlr;
        private bool isMinimap2;
        public bool IsNgmlr
        {
            get { return isNgmlr; }
            set
            {
                if (SetProperty(ref isNgmlr, value))
                {
                    if (isNgmlr) CheckMapperSelect(ngmlr);
                }
            }
        }
        public bool IsMinimap2
        {
            get { return isMinimap2; }
            set
            {
                if (SetProperty(ref isMinimap2, value))
                {
                    if (isMinimap2) CheckMapperSelect(minimap2);
                }
            }
        }

        private void CheckMapperSelect(string mapper)
        {
            switch (mapper)
            {
                case ngmlr:
                    IsMinimap2 = false;
                    break;
                case minimap2:
                    IsNgmlr = false;
                    break;
            }
            IsParameterSetup();
        }

        public void CallVariation()
        {

            // 状態チェック
            switch (SVButtonLabel)
            {
                case ButtonLabelExecute:
                    Properties.Settings.Default.SelecedTab = VatiantAnalysis;
                    Properties.Settings.Default.Save();

                    VariationStart();
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

        internal void VariationStart()
        {
            // パラメータの精査
            IsParameterSetup();
            // 必須チェック
            if (variationToolTip != MessageValues.enableSetting) return;
            if (!IsSetSaveDir()) return;  // save dir setting.

            var minqual = minQualityHightValue;  //  パラメータ設定時に精査しているのでエラーは無いはず
            int.TryParse(this.variantMinQualOption, out minqual);

            System.Diagnostics.Debug.WriteLine("## call variant. ##");
            var options = new CallVariantOptions()
            {
                selectedSequences = this.FastqFullPaths,
                reference = this.referenceName,
                isNanopore = this.isNanopore,
                isNgmlr = this.isNgmlr,
                // isLinux = true, // View側で設定しない。
                minQuality = minqual,
                outDir = savedir
            };
            AnalysisExecute(new CallSVIM(options, mainLog));

        }
    }
}
