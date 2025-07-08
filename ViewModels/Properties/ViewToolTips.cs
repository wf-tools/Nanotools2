using System;

namespace NanoTools2.ViewModels
{
    partial class AnalysesViewModel
    {

        private static string SelectFileDialogToolTip = "Open file select dialog, ";
        private static string SelectDirDialogToolTip = "Open foldser select dialog, ";

        private string guppyPathToolTip = "set a \"guppy_basecaller.exe\" path.";  // TODO i18n ?
        public string GuppyPathToolTip
                => guppyPathToolTip;

        public string GuppyPathButtonToolTip
                => SelectFileDialogToolTip + "Please select ont-guppy basecaller.";

        private string fast5DirsListToolTip = "Selected Fast5 folder(s)";
        public string Fast5DirsListToolTip
                => fast5DirsListToolTip;

        public string Fast5SelectToolTip
                => SelectDirDialogToolTip + "Please choose folder(s) including Fast5 files.";

        private string guppyOptionsToolTip = "this option is disabled, implement it in next version.";
        public string GuppyOptionsToolTip
                => guppyOptionsToolTip;


        // ----- mapping view -----
        private string referenceNameToolTip = "Please choose a Fasta file!.";
        public string ReferenceNameToolTip
                => referenceNameToolTip;


        private string referenceButtonToolTip = SelectFileDialogToolTip + "choose a Fasta file!.";
        public string ReferenceButtonToolTip
                => referenceButtonToolTip;

        private string fastqNamesToolTip = "Selected Fastq file(s)";
        public string FastqNamesToolTip
                => fastqNamesToolTip;

        private string fastqSelectToolsTip = SelectFileDialogToolTip + "choose Fastq files!.";
        public string FastqSelectToolsTip
                => fastqSelectToolsTip;

        private string selectSequencerToolTip = "select your fasrtq sequencer.";
        public string SelectSequencerToolTip
                => selectSequencerToolTip;

        private string mappingOptionToolTip = "add mapping option string.";
        public string MappingOptionToolTip
                => mappingOptionToolTip;

        private string assembleTargetGenomeSizeButtonToolTip = "number of target genome size. (k/m/g)";
        public string AssembleTargetGenomeSizeButtonToolTip
                => assembleTargetGenomeSizeButtonToolTip;

        private string assembleCycleToolTip = "number of times to perform minimap2 and miniasm repeatedly" + Environment.NewLine +
                                                        "0 is unlimited, until create same fasta.(Use this option with care) ";
        public string AssembleCycleToolTip
                => assembleCycleToolTip;

        private string assembleOptionToolTip = "eg: e.coli) 4.6m ,\neg: mouse) 3.3g ";
        public string AssembleOptionToolTip
        {
            get { return assembleOptionToolTip; }
            set { SetProperty(ref assembleOptionToolTip, value); }
        }

        private string mappingRadioToolTip = "select use mapping tool.";
        public string MappingRadioToolTip
                => mappingRadioToolTip;


        private string variantMinQualOptionToolTip = "Use only high-quality alignments for SV calling, \nif not use blank.";
        public string VariantMinQualOptionToolTip
                => variantMinQualOptionToolTip;

        
    }
}
