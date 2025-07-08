using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace NanoTools2.Utils
{
    [Obsolete("'use wfcompornent.", true)]
    public static class FileUtils
    {
        public static string[] ReadFile(string filepath, ref string errorMessage)
        {
            filepath = LinuxPath2WindowsPath(filepath);

            if (!File.Exists(filepath))
            {
                errorMessage = "not found file:" + filepath;
                return new string[] { }; // ファイルない
            }
            errorMessage = null;
            string allLine = null;
            try
            {
                using (StreamReader sr = new StreamReader(filepath))
                {
                    allLine = sr.ReadToEnd();
                }
                return allLine.Split('\n').Select(s => s.Trim()).ToArray();
            }
            catch (Exception e)
            {
                Console.WriteLine(filepath);
                Console.WriteLine(e.Message);
                errorMessage = e.Message;
                return new string[] { };
            }
        }

        public static string[] ReadGzFile(string gzfilepath, ref string errorMessage)
        {
            gzfilepath = LinuxPath2WindowsPath(gzfilepath);
            if (!File.Exists(gzfilepath) ||
                !Path.GetExtension(gzfilepath).EndsWith("gz", StringComparison.OrdinalIgnoreCase))
            {
                errorMessage = "not found file:" + gzfilepath;
                return new string[] { }; // gzファイルでない
            }

            // errorMessage = null;
            var gzlines = new List<string>();
            try
            {
                using (FileStream fin = new FileStream(gzfilepath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (GZipStream gzin = new GZipStream(fin, CompressionMode.Decompress))
                using (var reader = new StreamReader(gzin))
                {
                    while (!reader.EndOfStream)
                    {
                        gzlines.Add(reader.ReadLine().Trim());
                    }
                }
            }
            catch (Exception e)
            {
                // エラーの場合からのリストを返すだけ
                errorMessage = e.Message;
                return new string[] { };
            }
            // var lines = gzlines.ToArray();
            return gzlines.ToArray();
        }

        // ファイルの新規or上書き   // isAppend=true で 追記。
        public static void WriteFile(string outFilePath, IEnumerable<string> lines, ref string errorMessage, bool isAppend = false)
        {
            outFilePath = LinuxPath2WindowsPath(outFilePath);

            errorMessage = null;
            Encoding sjisEnc = Encoding.GetEncoding("Shift_JIS");

            try
            {
                using (StreamWriter writer = new StreamWriter(outFilePath, isAppend, sjisEnc))
                {
                    writer.WriteLine(string.Join(Environment.NewLine, lines));
                }
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
            }
        }

        public static void WriteFileFromString(string outFilePath, string line, ref string errorMessage)
        {
            outFilePath = LinuxPath2WindowsPath(outFilePath);

            errorMessage = null;
            Encoding sjisEnc = Encoding.GetEncoding("Shift_JIS");

            try
            {
                using (StreamWriter writer = new StreamWriter(outFilePath, false, sjisEnc))
                {
                    writer.WriteLine(line);
                }
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
            }
        }

        // windows のPath を LinuxSubSystem のPath へ変換
        public static string WindowsPath2LinuxPath(string path)
        {
            var d = string.IsNullOrWhiteSpace(path) ? new string[] { } : path.Split(':');
            var linuxPath = string.Empty;
            if (d.Count() > 1)
            {
                linuxPath = "/mnt/" + d[0].ToLower() + d[1].Replace("\\", "/");
            }
            else
            {
                linuxPath = path.Replace("\\", "/");
                // linuxPath = path;
            }
            return linuxPath;
        }

        public static string LinuxPath2WindowsPath(string linuxPath)
        {
            linuxPath = linuxPath.Replace("\\", "/");
            if (!linuxPath.StartsWith("/mnt/")) return linuxPath;

            var pieces = linuxPath.Split('/');
            var drive = pieces[2].ToUpper();
            var dirct = string.Join("\\", pieces.Skip(3));

            var winPath = drive + ":\\" + dirct;
            return winPath;
        }

        // MiSeq 由来のFastq ファイル名から、共通部分（-- の 前半部分を共通としている）
        // 13I-005_S6_L001_R1_001.fastq.gz / 13I-005_S6_L001_R2_001.fastq.gz
        // -> 13I-005_S6 が basename
        private static readonly string[] basenameSplitStr = new string[] { "--", "_L001_" };
        public static string GetMiseqFastqBaseName(string filePath)
        {
            // C:\test\test.add.txt -> test
            var fileName = System.IO.Path.GetFileNameWithoutExtension(filePath);
            var baseName = fileName.Split(basenameSplitStr, StringSplitOptions.None).First();
            return baseName;
        }

        // 文字列が全て英数字のみで構成されているか（2byte文字が入って居たら false）
        public static bool IsOneByteString(string str)
        {

            System.Diagnostics.Debug.WriteLine("IsOneByteString " + str);
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            byte[] byte_data = Encoding.GetEncoding(932).GetBytes(str);

            // debug....
            sw.Stop();
            System.Diagnostics.Debug.WriteLine("double-byte check end : " + sw.Elapsed);
            
            if (byte_data.Length == str.Length)
                return true;
            else
                return false;
        }

        public static bool IsFileNumberOfLinesAbove(string filePath, long lineCount)
        {
            if (!File.Exists(filePath)) return false;

            var filetext = File.ReadAllText(filePath, Encoding.GetEncoding("shift_jis")).Split('\n'); ;
            if (filetext.Count() >= lineCount) return true;
            return false;

        }

        public static long FileSize(string filePath, ref string message)
        {
            if (!File.Exists(filePath)) return 0L;
            var findfo = new FileInfo(filePath);
            return findfo.Length;
        }

        // 正常終了の場合は False
        public static bool FileCopy(string fromFile, string toFile, ref string message, bool isBackup = true)
        {
            try
            {
                if (isBackup && File.Exists(toFile))
                {
                    File.Move(toFile,
                                   Path.Combine(Path.GetDirectoryName(toFile),
                                          Path.GetFileNameWithoutExtension(toFile)
                                          + UniqueDateString()
                                          + Path.GetExtension(toFile)));
                }
                File.Copy(fromFile, toFile);
            }
            catch (IOException e)
            {
                message = "File copy error, Please check log.\n" + e.Message;
                return true;
            }
            return false;
        }

        public static bool FileMove(string fromFile, string toFile, ref string message, bool isBackup = true)
        {
            try
            {
                if (isBackup && File.Exists(toFile))
                {
                    File.Move(toFile,
                                   Path.Combine(Path.GetDirectoryName(toFile),
                                          Path.GetFileNameWithoutExtension(toFile) + UniqueDateString() + Path.GetExtension(toFile)));
                }
                File.Move(fromFile, toFile);
            }
            catch (IOException e)
            {
                message = "File copy error, Please check log.\n" + e.Message;
                return true;
            }
            return false;
        }

        public static bool FileBackupAddUniqDatetime(string bkFile, ref string message)
        {
            message = string.Empty;
            if (!File.Exists(bkFile)) return false;
            try
            {
                File.Move(bkFile,
                               Path.Combine(Path.GetDirectoryName(bkFile),
                                      Path.GetFileNameWithoutExtension(bkFile) + UniqueDateString() + Path.GetExtension(bkFile)));
            }
            catch (Exception e)
            {
                message = "File backup (System.IO.File.Move) error, Please check log.\n" + e.Message;
                return true;
            }
            return false;

        }


        public static string GetFileBaseName(string file)
        {
            var isValid = true;
            while (isValid)
            {
                file = Path.GetFileNameWithoutExtension(file);
                if (!file.EndsWith(".gz") && !file.EndsWith(".fasta") && !file.EndsWith(".fastq"))
                    isValid = false;
            }

            return file;

            // var splitChars = new char[] { '.', '_' };
            // var name = Path.GetFileNameWithoutExtension(file).Split(splitChars).First();
            // return name;
        }


        /** 複数Fasta/Fastq の マージを行います(省メモリー版?)
         * files : fasta or fastq (full path) file list.
         * outFile : out put merged file name(full path)
        */
        public static string MergeFiles(string outFilePath, IEnumerable<string> files)
        {
            // init.
            var message = string.Empty;

            try
            {
                Encoding sjisEnc = Encoding.GetEncoding("Shift_JIS");
                using (StreamWriter writer = new StreamWriter(outFilePath, false, sjisEnc))
                {
                    foreach (var file in files)
                    {
                        using (StreamReader sr = new StreamReader(file))
                        {
                            writer.Write(sr.ReadToEnd());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                message = e.Message;
            }
            return message;
        }

        /** 複数Fasta/Fastq の マージを行います(省メモリー版)
         * files : fasta or fastq (full path) file list.
         * outFile : out put merged file name(full path)
        */
        public static string MergeGzFiles(string outFilePath, IEnumerable<string> files)
        {
            // init.
            var message = string.Empty;

            try
            {
                Encoding sjisEnc = Encoding.GetEncoding("Shift_JIS");
                using (StreamWriter writer = new StreamWriter(outFilePath, false, sjisEnc))
                {
                    foreach (var file in files)
                    {
                        using (FileStream fin = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                        using (GZipStream gzin = new GZipStream(fin, CompressionMode.Decompress))
                        using (var reader = new StreamReader(gzin))
                        {
                            writer.Write(reader.ReadToEnd());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                message = e.Message;
            }
            return message;
        }




        // date pattern の 集約
        public static string UniqueDateString()
            => DateTime.Now.ToString("yyyyMMddHHmmss");
        // date pattern の 集約
        public static string LogDateString()
            => DateTime.Now.ToString("yyyy/MM/dd-HH:mm:ss");
    }
}
