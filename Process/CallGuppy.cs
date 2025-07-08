using NanoTools2.Process.Properties;
using NanoTools2.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace NanoTools2.Process
{
    public class CallGuppy : CallBase, ICallProcess
    {

        public string FastqDir = "fastq";
        public int minFastqSize = 15;
        private CallGuppyOption options;

        public CallGuppy(CallGuppyOption options, IProgress<string> progres) :
                 base (Path.GetDirectoryName(options.binaryPath), progres)
        {
            // System.Diagnostics.Debug.WriteLine("## call gappy init. ##");
            progres.Report("## call gappy init. ##");
            this.options = options;

            // parameter Check? 
            this.isEnable = true;
        }

        public async Task<string> CallProcessAsync()
        {
            // System.Diagnostics.Debug.WriteLine("## Call Process Async.....guppy.");
            var res = string.Empty;
            var sucessOut = new List<string>();
            foreach (var in2out in options.inDir2outDir)
            {

                log.Report("## guppy start.");

                if (!Directory.Exists(in2out.Value)) Directory.CreateDirectory(in2out.Value);

                string basecallResult = await GuppyBasecallAsync(options.binaryPath, in2out.Key, in2out.Value, options.selectedConfig);

                log.Report("## request end.");

                // basecall results
                if (basecallResult == processEndMessage) {

                    var pycoQcResult = await CallPycoQCAsync(in2out.Value);
                    // System.Diagnostics.Debug.WriteLine("pycoQcResult " + pycoQcResult);
                  

                    var barcodeDir = Path.Combine(
                                                Path.GetDirectoryName(in2out.Value),
                                                WfComponent.External.GuppyCommand.GuppyBarcodeOutDir); // barcode.

                    var barcodeResult = await GuppyBarcodeAsync(in2out.Value, barcodeDir);

                    if (barcodeResult == processEndMessage)
                            sucessOut.Add(barcodeDir); // guppy で出来たFastq 格納ディレクトリ
                }
                else
                { 
                    res += "input directory " + in2out.Key + System.Environment.NewLine +
                                basecallResult + System.Environment.NewLine;
                }
            }
            res = CombineFastq(sucessOut);
            return res;
        }

        public async Task<string> CallPycoQCAsync(string guppyOutDir)
        {
            log.Report("## PycoQC start.");
            var dirInfo = new DirectoryInfo(guppyOutDir);
            var files = dirInfo.EnumerateFiles("seq*.txt", SearchOption.AllDirectories);

            if (!files.Any()) return string.Empty;
            var summarytxt = files.First().FullName;
            var outhtml = Path.Combine(
                                        Path.GetDirectoryName(summarytxt),
                                        Path.GetFileNameWithoutExtension(summarytxt) + ".html");

            var op = new WfComponent.External.Properties.PycoQCOptions()
            {
                summaryText = summarytxt,
                outHtml = outhtml,
            };
            var pycoQcProc = new WfComponent.External.PycoQC(op);
            if (!pycoQcProc.isSuccess) return string.Empty;
            
            var currentResult = await OtherProcess(pycoQcProc);
            if(File.Exists(outhtml))
                System.Diagnostics.Process.Start(outhtml); // html ブラウザ表示


            log.Report("## request end. response " + currentResult);
            return outhtml;
        }

        // guppy basecall
        public async Task<string> GuppyBasecallAsync(string basecallerPath, string fast5InDir, string basecallOutDir, string basecallConfig)
        {
            this.process = new WfComponent.External.Guppy3FastBasecall(
                                        new WfComponent.External.Properties.GuppyOptions()
                                        {
                                            binaryPath = basecallerPath,   // 画面で決め打ちしているはず。無ければエラーになるだけ。
                                            Fast5Dir = fast5InDir,
                                            OutDir = basecallOutDir,
                                            Config = basecallConfig
                                        });

            if (!this.process.IsProcessSuccess()) { 
                log.Report("guppy basecall init error, : " + this.process.GetMessage());
                return ConstantValues.ErrorMessage;
            }

            var basecallResult = await ExternalProcessStart();
            return basecallResult;

        }

        // guppy barcoder
        public async Task<string> GuppyBarcodeAsync(string basecallOutDir, string barcodeOutDir)
        {

            if (!Directory.Exists(barcodeOutDir)) Directory.CreateDirectory(barcodeOutDir);

            var guppyBarcoder = Path.Combine(this.binaryDirectory,
                                                                  WfComponent.External.GuppyCommand.GuppyBarcoderBinName);
            if(!File.Exists(guppyBarcoder))
            {
                log.Report("not found barcoder program, search path :" + guppyBarcoder);
                return ConstantValues.ErrorMessage;
            }

            this.process = new WfComponent.External.Guppy3Barcode(
                new WfComponent.External.Properties.GuppyOptions()
                {
                        binaryPath = guppyBarcoder,  
                        FastqDir = basecallOutDir,
                        OutDir = barcodeOutDir,
                    });

            if (!this.process.IsProcessSuccess())
                log.Report("barcode process initi error :" + this.process.GetMessage());

            var barcodeResult = await ExternalProcessStart();
            return barcodeResult;
        }

        private string CombineFastq(IEnumerable<string> outDirs)
        {

            log.Report("## basecalled fastq was finished, it performs check.....");
            var message = string.Empty;
            var bardodedFastqs = new List<string>();
            foreach(var outDir in outDirs)   // search create barcoded-fastq
            {
                log.Report("CombineFastq : " + outDir);
                // cancel or error.
                if (token.IsCancellationRequested) return processCancelMessage;

                var barcodedDir = Path.Combine(  // Barcode 分割したディレクトリのトップ
                                                outDir);

                log.Report("barcode dir search " + barcodedDir);
                if (! Directory.Exists(barcodedDir)) continue;

                var fastqs = Directory.EnumerateFiles(
                                    barcodedDir, "*.fastq", SearchOption.AllDirectories) ;
                if (fastqs != null && fastqs.Any())
                    bardodedFastqs.AddRange(fastqs);  // AddRange call Null Exception 
            }

            // barcode per fastq-files
            var barcoadeGroups = bardodedFastqs.GroupBy(s => Path.GetFileName(
                                                                        Path.GetDirectoryName(s)))
                                                                    .ToDictionary(s => s);

            // barcode ごとに 1本のFastq を作成する
            foreach(var dat in barcoadeGroups)
            {
                var err = string.Empty;
                var groupFastq = dat.Value.Select(s => WfComponent.Utils.FileUtils.ReadFile(s, ref err))
                                                        .SelectMany(_ => _).Select(d => (string)d).ToArray();

                if (groupFastq.Count() > minFastqSize * 4) {
                    // var fastqName = Path.Combine(fastqOutDir, dat.Key.Key + ".fastq");
                    WfComponent.Utils.FileUtils.WriteFile(Path.Combine(options.OutDir, dat.Key.Key + ".fastq"),
                                               groupFastq,
                                               ref err);
                    if (!string.IsNullOrEmpty(err)) message += err;
                }
            }

            System.Diagnostics.Debug.WriteLine("CombineFastq end : " + message);
            return message;
        }

    }
}
