using System;
using System.IO;
using System.Threading.Tasks;

namespace NanoTools2.Process
{
    public class CallMinimap2 : CallBase , ICallProcess
    {
        // constractor.
        private WfComponent.External.Properties.Minimap2Options op;
        public CallMinimap2(WfComponent.External.Properties.Minimap2Options options , IProgress<string> progres) : 
            base(Path.GetDirectoryName(options.binaryPath) , progres) { 

            this.op = options;
            System.Diagnostics.Debug.WriteLine("Call Minimap2 initialisation.");

            // 実行可能であれば isEnable = true 
            this.isEnable = true;
        }

        // private WfComponent.External.Minimap2 proc;
        public async Task<string> CallProcessAsync()
        {
            System.Diagnostics.Debug.WriteLine("## Call Process Async.....minimap2.");

            process = new WfComponent.External.Minimap2(op);
            var res = await ExternalProcessStart();
            // TODO res による処理分岐。。。

            // IGV の起動
            res = await CallMappingResultsAsync();
            return res;
        }

        private async Task<string> CallMappingResultsAsync()
        {
            // samtools sam->sorted-bam
            var sortedBam = Path.Combine(
                                    Path.GetDirectoryName(op.OutFile),
                                    Path.GetFileNameWithoutExtension(op.OutFile) + ".bam");
            var samtoolsOpt = new WfComponent.External.Properties.SamtoolsOptions()
            {
                binaryPath = op.binaryPath,
                targetFile = op.OutFile,
                outFile = sortedBam
            };

            var callSamtools = new CallSamtools(samtoolsOpt, log);
            callSamtools.SetSam2BamWithIndex(op.OutFile, sortedBam);

            var res = await callSamtools.CallProcessAsync();  // samtools 実行
            if (res != processEndMessage) return res;          // error だったら message リターン


            // 正常終了したらIGV起動。IGV は起動だけ（結果ワカラナイ）static Class
            res = WfComponent.External.IGV.IGVstart(op.binaryPath, op.Reference, sortedBam);
            return res;
        }


    }
}
