using NanoTools2.Process.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NanoTools2.Process
{
    public class CallSVIM : CallBase, ICallProcess
    {
        private string svimResultViewDir = "svim-result";
        private string svimResultViewHta = "svresult.hta";

        // constractor.
        private CallVariantOptions op;
        public CallSVIM(CallVariantOptions options, IProgress<string> progres) :
            base(Path.GetDirectoryName(options.binaryPath), progres)
        {
            System.Diagnostics.Debug.WriteLine("## call SVIM init. ##");
            this.op = options;

            // parameter Check? 
            this.isEnable = true;
        }

        public async Task<string> CallProcessAsync()
        {
            System.Diagnostics.Debug.WriteLine("## Call Process Async.....SVIM.");

            var message = string.Empty;
            var queryFastq = string.Empty;
            if (op.selectedSequences.ToArray().Length == 1)
                queryFastq = op.selectedSequences.First();
            else
                message = MergeSequenceAsync(
                                    Path.Combine(op.outDir, "merged.fastq"), op.selectedSequences);

            if (!string.IsNullOrEmpty(message))
                return message;  // File read-write error!!



            var variantOpt = new WfComponent.External.Properties.SVIMOptions()
            {
                sequence = queryFastq,
                reference = op.reference,
                isNanopore = op.isNanopore,
                isNgmlr = op.isNgmlr,
                isLinux = true,
                minQuality = op.minQuality,
                outDir = op.outDir
            };
            process = new WfComponent.External.SVIM(variantOpt);
            var res = await ExternalProcessStart();
            if (!string.IsNullOrEmpty(res)) log.Report(res);


            res = CallVariantResultsAsync(); // 正常終了のとき IGV 起動
            if (!string.IsNullOrEmpty(res)) log.Report(res);


            var vcf = Path.Combine(op.outDir,"final_results.vcf");
            if(!File.Exists(vcf))
            {
                log.Report("no create final_results.vcf? " + vcf);
                return "final_results.vcf is not create. ";
            }

            // hta のコピーと起動。
            var curHtaDir = Path.Combine(
                                        AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\'),
                                        "bin",
                                        svimResultViewDir);
            var resHtaDir = Path.Combine(
                                        op.outDir,
                                        svimResultViewDir);

            var doHta = Path.Combine(
                                    resHtaDir,
                                    svimResultViewHta);

            WfComponent.Utils.FileUtils.DirectoryCopy(curHtaDir, resHtaDir);


            if (File.Exists(doHta) )
                System.Diagnostics.Process.Start(doHta); // HTA起動

            return res;
        }

        // input fastqs to merge-fastq
        // return merged-fastq path.
        private string MergeSequenceAsync(string outFastqPath, IEnumerable<string> fastqs)
        {
            string message = string.Empty;
            if (File.Exists(outFastqPath))
                WfComponent.Utils.FileUtils.FileBackupAddUniqDatetime(outFastqPath, ref message);

            var gzFastqs = fastqs.Where(s => s.EndsWith(".gz")).ToArray();
            var nogzFastqs = fastqs.Where(s => s.EndsWith(".fastq")).ToArray();
            if (gzFastqs.Any())
                message += WfComponent.Utils.FileUtils.MergeGzFiles(outFastqPath, gzFastqs);

            var isAppend = gzFastqs.Any();
            message += WfComponent.Utils.FileUtils.MergeGzFiles(outFastqPath, nogzFastqs, isAppend);

            return message;
        }



        private string CallVariantResultsAsync()
        {
            var bamFile = string.Empty;
            var bamFiles = Directory.EnumerateFiles(op.outDir, "*.bam", System.IO.SearchOption.AllDirectories);

            // find success. 
            if (bamFiles.Any())
                bamFile = bamFiles.First();
            else
                return string.Empty;   // bam ファイルが見つからない＝正常にSVが終わって居ない・・・

            log.Report("find bam file : " + bamFile);
            // samtools sam->sorted-bam
            var sortedBam = Path.Combine(
                                    Path.GetDirectoryName(op.outDir),
                                    Path.GetFileNameWithoutExtension(bamFile) + ".bam");
            var samtoolsOpt = new WfComponent.External.Properties.SamtoolsOptions()
            {
                binaryPath = op.binaryPath,
                targetFile = bamFile,
                outFile = sortedBam
            };

            //var callSamtools = new CallSamtools(samtoolsOpt, log);
            //callSamtools.SetSam2BamWithIndex(bamFile, sortedBam);

            //var res = await callSamtools.CallProcessAsync();  // samtools 実行
            //if (res != processEndMessage) return res;          // error だったら message リターン


            // 正常終了したらIGV起動。IGV は起動だけ（結果ワカラナイ）static Class
            var res = WfComponent.External.IGV.IGVstart(op.binaryPath, op.reference, bamFile);
            return res;
        }
    }
}
