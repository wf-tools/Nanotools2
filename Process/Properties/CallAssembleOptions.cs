using System.Collections.Generic;

namespace NanoTools2.Process.Properties
{
    public class CallAssembleOptions : WfComponent.External.Properties.RaconOptions
    {

        public string preset;
        public string outDir;
        public string cycleNo;
        public IEnumerable<string> fastqs;

    }
}
