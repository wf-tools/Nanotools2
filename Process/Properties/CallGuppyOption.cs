using System.Collections.Generic;

namespace NanoTools2.Process.Properties
{
    public class CallGuppyOption : WfComponent.External.Properties.GuppyOptions
    {

        public IDictionary<string, string> inDir2outDir;
        public string selectedConfig;
        public bool isDeleteTmp = false;  // 全部Fastq 以外のモノを全て削除

    }
}
