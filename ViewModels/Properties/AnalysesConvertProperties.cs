using NanoTools2.Process;
using NanoTools2.Process.Properties;
using NanoTools2.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NanoTools2.ViewModels
{
    partial class AnalysesViewModel
    {

        private string guppyPath;
        public string GuppyPath
        {
            get { return guppyPath; }
            set { if (SetProperty(ref guppyPath, value)) IsConvert(); }
        }

        private List<string> configList;
        public List<string> ConfigList
        {
            get { return configList; }
            set { if (SetProperty(ref configList, value)) IsConvert(); }
        }

        private string selectedConfig;
        public string SelectedConfig
        {
            get { return selectedConfig; }
            set { if (SetProperty(ref selectedConfig, value)) IsConvert(); }
        }

        private List<string> fast5Dirs;
        public List<string> Fast5Dirs
        {
            get { return fast5Dirs; }
            set { if( SetProperty(ref fast5Dirs, value)) IsConvert(); }
        }

        private string convertButtonLabel = ButtonLabelExecute;
        public string ConvertButtonLabel
        {
            get { return convertButtonLabel; }
            set { SetProperty(ref convertButtonLabel, value); }
        }
                     

        private string convertToolTip = MessageValues.notYetSetting;
        public string ConvertToolTip
        {
            get { return convertToolTip; }
            set { SetProperty(ref convertToolTip, value); }
        }

        private string convertOptionText = string.Empty;
        public string ConvertOptionText
        {
            get { return convertOptionText; }
            set { SetProperty(ref convertOptionText, value); }
        }



        private bool isConvertEnable = enable;
        public bool IsConvertEnable
        {
            get { return isConvertEnable; }
            set { SetProperty(ref isConvertEnable, value); }
        }

        // 以前にGuppy を設定してればPathをTextBoxに反映
        private void DefaultGuppyPath()
        {
            if (string.IsNullOrEmpty(Properties.Settings.Default.GuppyPath))
                return;   // guppy path ない。

            if (System.IO.File.Exists(Properties.Settings.Default.GuppyPath))
                GuppyPath = Properties.Settings.Default.GuppyPath;

            SetGuppyConfigList();  // config list => conbo box
        }

        // GuppyのPathを設定しているならConfigをリスト化する。
        private void SetGuppyConfigList()
        {
            if (!System.IO.File.Exists(Properties.Settings.Default.GuppyPath))
                return;            // Guppy 無いならConfigもなし。

            var searchPath = Path.GetDirectoryName(
                            Path.GetDirectoryName(guppyPath)); 

            System.Diagnostics.Debug.WriteLine("Search guppy configs : " + searchPath);
            var di = new DirectoryInfo(searchPath);
            var files = di.EnumerateFiles("dna*.cfg", SearchOption.AllDirectories);

            if (files.Any())
            {
                ConfigList = files.Select(s => s.Name).ToList();

                if (string.IsNullOrEmpty(Properties.Settings.Default.UseConfig) ||
                   !selectedConfig.Contains(Properties.Settings.Default.UseConfig))
                {
                    var fastCfg = files.Where(s => s.Name.Contains("fast"));
                    if (fastCfg.Any())
                        SelectedConfig = fastCfg
                                                    .OrderByDescending(s => s.Name)
                                                    .First().Name;
                }
                else
                {
                    SelectedConfig = Properties.Settings.Default.UseConfig;
                }
            }
        }


        private void IsConvert()
        {
            if (!string.IsNullOrEmpty(guppyPath) && fast5Dirs != null && fast5Dirs.Any())
                ConvertToolTip = MessageValues.enableSetting;
        }

        // Guppy Path.
        public static string ontProgramDir = @"C:\Program Files\OxfordNanopore";
        public void SelectGuppyPath()
        {
            string dialogOpen = null;
            if (! string.IsNullOrEmpty(Properties.Settings.Default.GuppyPath) ){
                var oldParh = new DirectoryInfo(Properties.Settings.Default.GuppyPath);
                dialogOpen = oldParh.Parent.FullName;
            }
            else
            {  //setting になかったけどデフォルトのインストール場所にあったとか。
                if (Directory.Exists(ontProgramDir))
                    dialogOpen = ontProgramDir;
            }

            var picFiles = SelectFiles(
                                    "Select Guppy Basecaller",
                                    "すべてのファイル|*.*|exe ファイル|guppy_basecaller*.exe", 
                                    2,
                                    false,
                                    dialogOpen);

            if (picFiles != null && picFiles.Length > 0) { 
                GuppyPath = picFiles.First();
                Properties.Settings.Default.GuppyPath = GuppyPath;
                Properties.Settings.Default.Save();

                // config 更新。
                SetGuppyConfigList();
            }
        }

        // fast5 directries
        public void SelectFast5Directories()
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            var picDirs = SelectDirectories(
                                    "Select Include-fast5 Directories",
                                    true // multi select
                                    );
            if (picDirs != null && picDirs.Length > 0) {
                Fast5Dirs = picDirs.ToList();
            }

            sw.Stop();
            System.Diagnostics.Debug.WriteLine(sw.Elapsed);
        }

        // Guppy の オプション表示。 TODO：別画面
        public void ConvertOption()
        {
            // System.Diagnostics.Process.Start("https://www.w-fusion.co.jp/J/contactus.php");
        }


        public void CallConvert()
        {
            // 状態チェック
            switch (convertButtonLabel)
            {
                case ButtonLabelExecute:
                    Properties.Settings.Default.SelecedTab = ConvertAnalysis;
                    Properties.Settings.Default.UseConfig = selectedConfig;
                    Properties.Settings.Default.Save();
                    ConvertStart();
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


        internal void ConvertStart() { 
            // 必須チェック
            if (convertToolTip != MessageValues.enableSetting) return;
            if (!IsSetSaveDir()) return;  // save dir setting.

            System.Diagnostics.Debug.WriteLine("## call convert. ##");
            var inFast5outFastq = new Dictionary<string, string>();
            foreach(var dir in fast5Dirs)
            {
                inFast5outFastq.Add(dir,
                                              Path.Combine(savedir,
                                                Path.GetFileNameWithoutExtension(dir),
                                                WfComponent.External.GuppyCommand.GuppyBasecallOutDir)); // basecall
            }

            var options = new CallGuppyOption()
            {
                OutDir = savedir,
                binaryPath = this.guppyPath,
                inDir2outDir = inFast5outFastq,
                selectedConfig = selectedConfig,
                otherOptions = convertOptionText
            };

            AnalysisExecute(new CallGuppy(options, mainLog));
        }

    }
}
