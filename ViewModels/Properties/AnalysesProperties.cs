using Prism.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace NanoTools2.ViewModels
{
    partial class AnalysesViewModel
    {

        private const string ConvertAnalysis = "convert";
        private const string MappingAnalysis = "mapping";
        private const string AssembleAnalysis = "assemble";
        private const string VatiantAnalysis = "variant";

        private const string ButtonLabelExecute = "Execute";
        private const string ButtonLabelCancel = "Cancel";
        private const string ButtonLabelExit = "Exit";
        private const string ButtonLabelClear = "Clear";

        private static readonly bool enable = true;
        private static readonly bool disnable = false;

        public DelegateCommand GuppySlectCommand { get; }  // FileSelect
        public DelegateCommand Fast5SelectCommand { get; }  // FileSelect
        public DelegateCommand ConvertCommand { get; }  // FileSelect
        public DelegateCommand ConvertOptionCommand { get; }  // FileSelect

        public DelegateCommand ReferenceSlectCommand { get; }
        public DelegateCommand FastqSelectCommand { get; }
        public DelegateCommand MappingExecuteCommand { get; }
        public DelegateCommand MappingOptionCommand { get; }

        public DelegateCommand AssembleOptionCommand { get; }
        public DelegateCommand AssembleExecuteCommand { get; }
        public DelegateCommand AssembleCycleCommand { get; }
        public DelegateCommand AssembleButtonOver { get; }

        public DelegateCommand BamSelectCommand { get; }
        public DelegateCommand SVOptionCommand { get; }
        public DelegateCommand SVExecuteCommand { get; }

        public DelegateCommand OpenInfomationCommand { get; }

        private bool isCovertSelect = disnable;
        public bool IsCovertSelect
        {
            get { return isCovertSelect; }
            set { SetProperty(ref isCovertSelect, value); }
        }

        private bool isMappingSelect = disnable;
        public bool IsMappingSelect
        {
            get { return isMappingSelect; }
            set { SetProperty(ref isMappingSelect, value); }
        }

        private bool isAssembleSelect = disnable;
        public bool IsAssembleSelect
        {
            get { return isAssembleSelect; }
            set { SetProperty(ref isAssembleSelect, value); }
        }

        private bool isVariantSelect = disnable;
        public bool IsVariantSelect
        {
            get { return isVariantSelect; }
            set { SetProperty(ref isVariantSelect, value); }
        }

        private bool isProgress = disnable;
        public bool IsProgress
        {
            get { return isProgress; }
            set { SetProperty(ref isProgress, value); }
        }

        // 何かで設定したパスを保存する
        private void SetSelectDir(string[] paths, bool isSave = true)
        {
            if (paths == null || paths.Count() == 0 || !isSave) return;

            var path = paths[0];
            if (string.IsNullOrEmpty(path)) return;

            // 余りないけど。ルートディレクトリ(C:\ D:\とか指定された場合は、親ディレクトリ取れないでエラーになる)
            var selectedSaveDir = new DirectoryInfo(path);
            if (selectedSaveDir.Name != selectedSaveDir.Root.Name)
            {  // 今回指定されたところを覚えて置く
                Properties.Settings.Default.SelectDir = selectedSaveDir.Parent.FullName;
                Properties.Settings.Default.Save();
            }
        }

        private void SelectReference()
        {
            var fasta = SelectFiles(
                                    "Select Reference Fasta",
                                     "すべてのファイル|*.*|fasta ファイル|*.fasta|fa ファイル|*.fa",
                                    2,
                                    false);

            if (fasta != null && fasta.Any())
                ReferenceName = fasta.First();
        }

        // ファイル選択
        private string[] SelectFiles(string title, string filter, int filterInd, bool isMulti , string selectedDir = null )
        {
            var openDir = selectedDir ??
                                 Properties.Settings.Default.SelectDir;
            openDir = string.IsNullOrWhiteSpace(openDir) ?
                                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) :
                                openDir;

            string[] fullPaths = null;
            var selectDir = openDir;
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Title = title;
            dialog.Multiselect = isMulti;
            dialog.InitialDirectory = openDir;
            dialog.Filter = filter;
            dialog.FilterIndex = filterInd;
            if (true == dialog.ShowDialog())
            {
                selectDir = Path.GetFileName(dialog.FileName);
                fullPaths = dialog.FileNames;
            }
            SetSelectDir(fullPaths);
            return fullPaths;
        }

        // ディレクトリ選択
        private string[] SelectDirectories(string title, bool isMulti, string selectedDir = null)
        {
            var openDir = selectedDir ??
                     Properties.Settings.Default.SelectDir;
            openDir = string.IsNullOrWhiteSpace(openDir) ?
                                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) :
                                openDir;

            IEnumerable<string> selectDirs = null;
            var dialog = new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog()
            {
                Title = title,
                IsFolderPicker = true,
                InitialDirectory = openDir,
                AddToMostRecentlyUsedList = false,
                AllowNonFileSystemItems = false,
                EnsureFileExists = true,
                EnsurePathExists = true,
                EnsureReadOnly = false,
                EnsureValidNames = true,
                Multiselect = isMulti,
                ShowPlacesList = false
            };

            if (dialog.ShowDialog() == Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult.Ok)
            {
                selectDirs = dialog.FileNames;
                foreach(var dir in selectDirs) {
                    if (!WfComponent.Utils.FileUtils.IsOneByteString(dir))
                    {
                        ShowCution(
                                    "double-byte warning",
                                    "cannot use double-byte characters.");
                        return null;
                    }
                }
            }
            dialog.Dispose();
            if (selectDirs == null) return null;

            SetSelectDir(selectDirs.ToArray());
            return selectDirs.ToArray();
        }

        // 簡単メッセージボックス
        private void ShowCution(string dialogTitle = null , string dialogMessage = null)
        {
            dialogMessage = dialogMessage ?? "Invalid command.";
            dialogTitle = dialogTitle ??  "worning!";
            MessageBox.Show(dialogMessage, dialogTitle);
        }

        private bool IsSetSaveDir()
        {
            var dir = SelectDirectories(
                            "save directory",
                            false,
                            Properties.Settings.Default.SaveDir);
            if (dir == null || !dir.Any())
                return false;

            SaveDir = dir.First();

            // 余りないけど。ルートディレクトリ(C:\ D:\とか指定された場合は、親ディレクトリ取れないでエラーになる)
            var selectedSaveDir = new DirectoryInfo(savedir);
            if (selectedSaveDir.Name != selectedSaveDir.Root.Name)
            {  // 今回指定されたところを覚えて置く
                Properties.Settings.Default.SaveDir = selectedSaveDir.Parent.FullName;
                Properties.Settings.Default.Save();
            }

            return true;
        }

        private void InitSelectTab(bool select = false)
        {
            var selectTabName = Properties.Settings.Default.SelecedTab ?? ConvertAnalysis;
            switch (selectTabName)
            {
                case ConvertAnalysis :
                    IsCovertSelect = enable;
                    IsMappingSelect = disnable;
                    IsAssembleSelect = disnable;
                    IsVariantSelect = disnable;
                    break;
                case MappingAnalysis:
                    IsCovertSelect = disnable;
                    IsMappingSelect = enable;
                    IsAssembleSelect = disnable;
                    IsVariantSelect = disnable;
                    break;
                case AssembleAnalysis:
                    IsCovertSelect = disnable;
                    IsMappingSelect = disnable;
                    IsAssembleSelect = enable;
                    IsVariantSelect = disnable;
                    break;
                case VatiantAnalysis:
                    IsCovertSelect = disnable;
                    IsMappingSelect = disnable;
                    IsAssembleSelect = disnable;
                    IsVariantSelect = enable;
                    break;
            }

        }

    }

}
