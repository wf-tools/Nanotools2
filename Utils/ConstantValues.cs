namespace NanoTools2.Utils
{
    public static class ConstantValues
    {
        public const string CanceledMessage = "canceled";
        public const string ErrorMessage = "process fail.";
        public const string NormalEndMessage = "normal";

        public static readonly string MainRegion = "MainRegion";
        public static readonly string SideRegion = "SideRegion";
        public static readonly string LogRegion = "LogRegion";
        public static readonly string PassParamName = "ParamName";
        public static readonly string MainLog = "mainlog";
        public static readonly string MainLogClear = "CLEAR";

        // Database と連動しているので注意。
        public static readonly int MiSeqSequencerId = 1;
        public static readonly int MinIONSequencerId = 2;
        public static readonly string DefauleDbParameterName = "default";

        public static readonly string ConvertDir = "fastq";
        public static readonly string MappingDir = "mapping";


        /** Mapping **/
        public static readonly string mappingResultDirName = "aligned";
        public static readonly string fstMappingResultDirName = "1st_aligned";
        public static readonly string secMappingResultDirName = "2nd_aligned";
        public static readonly string secMappingReferenceDirName = "reference";

        // 
        public const string DbDelimiter = "///";
        public const string ViewDelimiter = "-";
        public const string UNDER = "_";

        public const string MIN = "MIN";
        public const string MAX = "MAX";

        public const string DbPropertyPrefixCoverRatio = "COVER_RATIO_";
        public const string DbPropertyPrefixCoverAvarage = "AVE_COVER_";
        public const string DbDefaultFloatValue = "0.000";

        public const int DefaultCutoff = 30;
    }
}
