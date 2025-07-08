using NanoTools2.Utils;
using System;
using System.IO;
using System.Threading.Tasks;
using WfComponent.External.Properties;

namespace NanoTools2.Process
{
    public class CallSamtools : CallBase, ICallProcess
    {
        SamtoolsOptions op;
        string arguments;
        // constractor.
        public CallSamtools(SamtoolsOptions options, IProgress<string> progres) :
            base(Path.GetDirectoryName(options.binaryPath), progres)
        {
            System.Diagnostics.Debug.WriteLine("Call Samtools initialisation.");
            this.op = options;
        }

        // Command execite.
        // private WfComponent.External.Samtools proc;
        public void SetCreateIndex(string indexOp, string targetFile)
        {
            arguments = $" {indexOp}  {targetFile}";
            process.SetArguments("SetCreateIndex : " + arguments);  // コマンドセット
            isEnable = true;
        }

        public void SetSam2BamWithIndex(string samFile, string outSortedBam)
        {
            arguments = $" {SamtoolsOptions.sort}  {SamtoolsOptions.outTypeBam} -o {outSortedBam}  {samFile}";
            System.Diagnostics.Debug.WriteLine("SetSam2BamWithIndex :" + arguments);
            isEnable = true;
        }

        public async Task<string> CallProcessAsync()
        {
            if (string.IsNullOrEmpty(this.arguments))
                return "not set samtools arguments!";

            this.process = process == null ?
                new WfComponent.External.Samtools(op) : this.process;
            process.SetArguments(arguments);  // コマンドセット

            var res = await ExternalProcessStart();
            return res;
        }

    }
}
