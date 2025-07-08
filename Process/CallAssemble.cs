using NanoTools2.Utils;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NanoTools2.Process
{
    public class CallAssemble : CallBase, ICallProcess
    {
        private Properties.CallAssembleOptions op;
        public string lastAssembled;
        public readonly string lastAssembledName = "final-assemble.fasta";
        public static readonly string cycleno = "3";

        //constractor
        public CallAssemble(Properties.CallAssembleOptions options, IProgress<string> progres) :
                     base(Path.GetDirectoryName(options.binaryPath), progres)
        {
            this.op = options;
            // System.Diagnostics.Debug.WriteLine("Call Assemble initialisation.");
            log.Report("Call Assemble initialisation.");

            // 実行可能であれば isEnable = true 
            this.isEnable = true;
        }

        // Assemble workflow.
        // minimap -> miniasm -> racon -> minimap -> racon.....
        public async Task<string> CallProcessAsync()
        {
            System.Diagnostics.Debug.WriteLine("## Call Process Assemble.");
                        
            if (op.fastqs.Count() > 1)　// 入力Fastq をまとめる。
            {
                var mergedFastq = Path.Combine(op.outDir, "merged.fastq");
                var mergeMes = WfComponent.Utils.FileUtils.MergeFiles(mergedFastq, op.fastqs);
                if (string.IsNullOrEmpty(mergeMes))
                {
                    queryFastq = mergedFastq;
                }
                else
                {
                    // File merge error....
                    return ConstantValues.ErrorMessage + Environment.NewLine + mergeMes;
                }
            }
            else
            {
                // 入力FASTQが1本しかない場合は、そのまま使う。
                queryFastq = op.fastqs.First();
            }

            var res = await CallAssembleResultsAsync();


            log.Report("Call Assemble end..");
            log.Report(res);


            // 結果表示
            var mes = ViewAssembleResults();
            if (!string.IsNullOrEmpty(mes)) log.Report(mes);

            return res;
        }

        string queryFastq;
        private async Task<string> CallAssembleResultsAsync()
        {

            var res = string.Empty;
            var message = string.Empty;

            // 1st mapping to paf
            var outPaf = Path.Combine(op.outDir,
                                                    "out-0.paf");
            var fstFastq = new string[] { queryFastq };
            res = await DoMinimap2Paf(queryFastq, queryFastq, outPaf);
            log.Report(res);

            // TODO check results....
            if(WfComponent.Utils.FileUtils.FileSize(outPaf, ref message) <= 0L)
            {
                log.Report(message);
                var errorMessage = "minimap2 command is not create valid paf-file, Please check input files.";
                log.Report(errorMessage);
                // return ConstantValues.ErrorMessage;
                return errorMessage;
            }


            // 1st miniasm
            var outGfa = Path.Combine(op.outDir,
                                                    "out-0.gfa");
            res = await DoMiniasm(queryFastq, outPaf, outGfa);
            log.Report(res);

            // TODO check results....
            if (WfComponent.Utils.FileUtils.FileSize(outGfa, ref message) <= 0L)
            {
                var errorMessage = "miniasm command is not create valid gfa-file, Please check input files.";
                log.Report(message);
                log.Report(errorMessage);
                return errorMessage;
            }

            // 1st raw assembled fasta
            var outFasta = Path.Combine(op.outDir,
                                                        "out-0.fasta");
            res = WfComponent.External.Miniasm.CreateRawFasta(outGfa, outFasta);
            log.Report(res);

            // TODO check results....
            if (WfComponent.Utils.FileUtils.FileSize(outFasta, ref message) <= 0L)
            {
                var errorMessage = "miniasm command is not create valid fasta-file, Please check input files.";
                log.Report(message);
                log.Report(errorMessage);
                return errorMessage;
            }

            var cycle = int.Parse(cycleno); //default = 3
            int.TryParse(op.cycleNo.Trim(), out cycle); // 画面に入力された数字 ダメならデフォルト値

            if (cycle > 0)
                res = await FiniteCycle(cycle);
            else
                res = await UnlimitCycle();

            return res;
        }

        private async Task<string> FiniteCycle(int cycleNo)
        {
            // 有限回数分
            var res = string.Empty;
            // 1st mapping to paf
            var outPaf = Path.Combine(op.outDir,
                                                    "out-1.paf");
            foreach (var num in Enumerable.Range(0, cycleNo))
            {
                var next = num + 1;
                var referFasta = Path.Combine(op.outDir, "out-" + num + ".fasta");
                var rawFasta = Path.Combine(op.outDir, "out-" + next + ".fasta");

                res = await DoMinimap2Paf(referFasta, queryFastq, outPaf);
                if (!string.IsNullOrEmpty(res)) return res;
                res = await DoRacon(queryFastq, outPaf, referFasta, rawFasta);
                if (!string.IsNullOrEmpty(res)) return res;

                var rawsize = WfComponent.Utils.FileUtils.FileSize(rawFasta, ref res);
                if (!string.IsNullOrEmpty(res)) return res;
                if (rawsize <= 0L) return "miniasm command is not create valid fasta-file, Please check input files.";

                this.lastAssembled = rawFasta;
            }
            return res;
        }

        private async Task<string> UnlimitCycle()
        {
            var isAlmostSame = true;
            var outNo = 0;
            var outPaf = Path.Combine(op.outDir,
                                                    "out-1.paf");
            var res = string.Empty;
            while (isAlmostSame)
            {
                var next = outNo + 1;
                var referFasta = Path.Combine(op.outDir, "out-" + outNo + ".fasta");
                var rawFasta = Path.Combine(op.outDir, "out-" + next + ".fasta");

                res = await DoMinimap2Paf(referFasta, queryFastq, outPaf);
                if (!string.IsNullOrEmpty(res)) return res;
                res = await DoRacon(queryFastq, outPaf, referFasta, rawFasta);
                if (!string.IsNullOrEmpty(res)) return res;

                this.lastAssembled = rawFasta;
                outNo += 1;

                var rawsize = WfComponent.Utils.FileUtils.FileSize(rawFasta, ref res);
                if (!string.IsNullOrEmpty(res)) return res;
                var refersize = WfComponent.Utils.FileUtils.FileSize(referFasta, ref res);
                if (!string.IsNullOrEmpty(res)) return res;
                if (rawsize <= 0L) return "miniasm command is not create valid fasta-file, Please check input files.";

                // 同じFASTAとする判定 ファイルサイズが一緒。　// TODO 塩基を精査する？
                isAlmostSame = (rawsize != refersize) ;

            }
            return res;
        }


        private async Task<string> DoRacon(string sequence, string paf, string rowRefernce, string outFasta)
        {
            System.Diagnostics.Debug.WriteLine("racon process start.");

            process = new WfComponent.External.Racon(
                 new WfComponent.External.Properties.RaconOptions()
                 {
                     isLinux = true,
                     useCore = Environment.ProcessorCount.ToString(),
                     sequences = sequence,
                     overlaps = paf,
                     target = rowRefernce,
                     outFile = outFasta
                 });
            if (!process.IsProcessSuccess())  // インスタンス作成時にエラー
            {
                System.Diagnostics.Debug.WriteLine("##### Error. #####");
                return ConstantValues.ErrorMessage + Environment.NewLine +
                            "Racon Initialize error." + Environment.NewLine +
                            process.GetMessage();
            }

            var res = await ExternalProcessStart();
            if (!process.IsProcessSuccess())  // 実行時にエラー
            {
                System.Diagnostics.Debug.WriteLine("##### Error. #####");
                return ConstantValues.ErrorMessage + Environment.NewLine +
                            "Racon Execute error." + Environment.NewLine +
                            process.GetMessage();
            }
            return res;
        }

        private async Task<string> DoMinimap2Paf(string refer, string query, string outPaf)
        {
            System.Diagnostics.Debug.WriteLine("minimap2 process start.");

            // minimap2 で、PAF 出力
            var queryFastqs = new string[]
            {
                query
            };
            var preset = WfComponent.External.Properties.Minimap2Options.avaont;

            process = new WfComponent.External.Minimap2(
                new WfComponent.External.Properties.Minimap2Options()
                {
                    Reference = refer,
                    QueryFastqs = queryFastqs,
                    OutFile = outPaf,
                    UseCore = Environment.ProcessorCount.ToString(),
                    Preset = preset,
                    isMapping = false
                });

            if (!process.IsProcessSuccess())  // インスタンス作成時にエラー
            {
                System.Diagnostics.Debug.WriteLine("##### Error. #####");
                return ConstantValues.ErrorMessage + Environment.NewLine +
                            "Minimap2 Initialize error." + Environment.NewLine +
                            process.GetMessage();
            }

            var res = await ExternalProcessStart(); 
            if (!process.IsProcessSuccess())  // 実行時にエラー
            {
                System.Diagnostics.Debug.WriteLine("##### Error. #####");
                return ConstantValues.ErrorMessage + Environment.NewLine +
                            "Minimap2 Execute error." + Environment.NewLine +
                            process.GetMessage() + Environment.NewLine +
                            res;
            }
            return res;
        }

        private async Task<string> DoMiniasm(string sequence, string targetPaf, string outGaf)
        {
            System.Diagnostics.Debug.WriteLine("miniasm process start.");

            process = new WfComponent.External.Miniasm(
                new WfComponent.External.Properties.MiniasmOptions()
                {
                    sequences = sequence,
                    referencePaf = targetPaf,
                    outGaf = outGaf,
                    isLinux = true
                });

            if (!process.IsProcessSuccess())  // インスタンス作成時にエラー
            {
                System.Diagnostics.Debug.WriteLine("##### Error. #####");
                return ConstantValues.ErrorMessage + Environment.NewLine +
                            "Racon Execute error." + Environment.NewLine +
                            process.GetMessage() + Environment.NewLine;
            
            }

            var res = await ExternalProcessStart();
            if (!process.IsProcessSuccess())  // 実行時にエラー
            {
                System.Diagnostics.Debug.WriteLine("##### Error. #####");
                return ConstantValues.ErrorMessage + Environment.NewLine +
                            "Racon Execute error." + Environment.NewLine +
                            process.GetMessage() + Environment.NewLine +
                            res;
            }
            return res;
        }


        // 実行時後に表示する内容を作成する。
        /**
        *  N50
        *  MinLingth
        *  MaxLength
        *  Avalage
        *  Count
        *  All nuc-length
        *  -> assemble.hta
        */
        private string ViewAssembleResults()
        {
            var mes = string.Empty;
            var fasta = WfComponent.Utils.FileUtils.ReadFile(lastAssembled, ref mes);
            if (!string.IsNullOrEmpty(mes)) return mes;

            // rename
            var outFinalFasta = Path.Combine(
                                            Path.GetDirectoryName(lastAssembled),
                                            lastAssembledName);
            WfComponent.Utils.FileUtils.FileMove(lastAssembled, outFinalFasta, ref mes);
            if (!string.IsNullOrEmpty(mes)) return mes;

            var nucs = fasta.Where(s => !string.IsNullOrEmpty(s)).
                       Where(s => !s.StartsWith(">")).
                       ToArray();

            var sum = nucs.Sum(s => s.Length);

            var avg = nucs.Average(s => s.Length);

            var max = nucs.Max(s => s.Length);

            var min = nucs.Min(s => s.Length);

            var fastaCnt = nucs.Length;

            var n = 0;
            var n50 = 0;
            foreach (var nuc in nucs)
            {
                n += nuc.Length;
                if (n > sum / 2)
                {
                    n50 = nuc.Length;
                    break;
                }
            }

            // template file
            var binDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
            var hta = WfComponent.CommandUtils.FindProgramFile(binDir, "assemble.hta", true,false);

            var htaLine = string.Join("\n", WfComponent.Utils.FileUtils.ReadFile(hta, ref mes));
            if (!string.IsNullOrEmpty(mes)) {
                log.Report(hta + " " + mes);
                return  mes; ;
            }
            htaLine = htaLine.Replace("%N50%", n50.ToString());
            htaLine = htaLine.Replace("%MAX%", max.ToString());
            htaLine = htaLine.Replace("%MIN%", min.ToString());
            htaLine = htaLine.Replace("%AVG%", avg.ToString());
            htaLine = htaLine.Replace("%CNT%", sum.ToString());
            htaLine = htaLine.Replace("%CONTIG%", fastaCnt.ToString());

            var outHta = Path.Combine(op.outDir, "assemble.hta");
            WfComponent.Utils.FileUtils.WriteFileFromString(Path.Combine(op.outDir, "assemble.hta"), htaLine, ref mes);
            if (!string.IsNullOrEmpty(mes)) return mes;

            System.Diagnostics.Process.Start(outHta); // hta起動
            return mes;  
        }

    }
}
