using System;
using System.Collections.Generic;
using System.Linq;

namespace NanoTools2.Utils
{
    [Obsolete("'use wfcompornent.", true)]
    public static class Fasta
    {

        //　指定したfastaReadName の　Name->Nucs を返します。
        public static IDictionary<string, string> FastaSelectSequence(string fastaFilePath, string fastaReadName)
        {
            string errorMessage = null;
            var fastaLines = WfComponent.Utils.FileUtils.ReadFile(fastaFilePath, ref errorMessage);
            if (!string.IsNullOrEmpty(errorMessage))
                return new Dictionary<string, string>();            // return null; // ? 

            return FastaSequence(fastaLines, fastaReadName);
        }

        public static IDictionary<string, string> FastaSequence(string[] fastaLines, string fastaReadName)
        {
            var resRefName2RefNucs = new Dictionary<string, string>();

            var readNameLine = string.Empty;
            var nucLines = new List<string>();
            var isThisReference = false;

            foreach (var line in fastaLines)
            {
                if (line.Replace(">", "").StartsWith(fastaReadName))
                {
                    isThisReference = true;
                    readNameLine = line;
                    continue;
                }

                if (line.StartsWith(">")) isThisReference = false;  // 次のリード
                if (isThisReference) nucLines.Add(line);

            }

            resRefName2RefNucs.Add(readNameLine, string.Join(string.Empty, nucLines));
            return resRefName2RefNucs;
        }

        public static IEnumerable<KeyValuePair<string, string>> ConsensusFasta(string fastaFilePath)
        {
            var resRefName2RefNucs = new SortedDictionary<string, string>();  // Cons1 ~ Cons3 なので Sortすると順番になる（）

            string errorMessage = null;
            string[] fastaLines = fastaLines = WfComponent.Utils.FileUtils.ReadFile(fastaFilePath, ref errorMessage);
            if (!string.IsNullOrEmpty(errorMessage)) return resRefName2RefNucs;  // 読み込みできなければ空のDicを返す。

            var readNameLine = string.Empty;
            var nucLines = new List<string>();
            foreach (var line in fastaLines.Reverse())
            {
                if (line.StartsWith(">"))
                {
                    readNameLine = line.Replace(">", "");
                    nucLines.Reverse();
                    resRefName2RefNucs.Add(readNameLine, string.Join(string.Empty, nucLines));
                    nucLines = new List<string>();
                    continue;
                }
                nucLines.Add(line);
            }
            return resRefName2RefNucs;
        }

        public static IEnumerable<KeyValuePair<string, string>> FastaFile2Dic(string fastaFilePath, bool isDelSpaceName = false)
        {
            var readName2nucs = new Dictionary<string, string>();
            string errorMessage = null;

            string[] fastaLines = null;
            if (System.IO.Path.GetExtension(fastaFilePath).EndsWith("gz"))
                fastaLines = WfComponent.Utils.FileUtils.ReadGzFile(fastaFilePath, ref errorMessage);
            else
                fastaLines = WfComponent.Utils.FileUtils.ReadFile(fastaFilePath, ref errorMessage);

            if (!string.IsNullOrEmpty(errorMessage)) return readName2nucs;  // 読み込みできなければ空のDicを返す。

            var readNameLine = string.Empty;
            var readCount = 0;
            var nucLines = new List<string>();
            foreach (var line in fastaLines.Reverse())
            {
                if (line.StartsWith(">"))
                {
                    readNameLine = line.Replace(">", "");
                    if (isDelSpaceName)  // ユーザ指定のFasta　同じ名前とかあるものを救済
                    {
                        readCount++;
                        readNameLine = readNameLine.Replace(' ', '_').Replace('?', '_').ToUpper() + "_UserSeq-" + readCount.ToString("000000");
                        // if (readName2nucs.ContainsKey(readNameLine)) continue;
                    }
                    nucLines.Reverse();
                    readName2nucs.Add(readNameLine, string.Join(string.Empty, nucLines).Trim());
                    nucLines = new List<string>();
                    continue;
                }
                nucLines.Add(line);
            }
            return readName2nucs;
        }

        public static IEnumerable<string> FastaNames(string fastaFilePath)
        {
            string errorMessage = null;
            var fastaLines = WfComponent.Utils.FileUtils.ReadFile(fastaFilePath, ref errorMessage);
            if (!string.IsNullOrEmpty(errorMessage)) return new string[] { };  // 空配列返す？ null 返す？

            var names = new List<string>();
            foreach (var line in fastaLines)
            {
                if (line.StartsWith(">"))
                    names.Add(line.Split(' ').First().Replace(">", ""));
            }
            // 読み込んだFASTAの名前（順番大事なやつ）
            return names;
        }

        // Fastaに含まれる’N'の個数が、閾値を超えるとtrue
        public static bool IsApplyConsensus(string fastaNuc, int cutoffRatio)
        {
            double nCnt = fastaNuc.ToUpper().Where(s => s == 'N').Count();
            double fastaCnt = fastaNuc.Count();
            double ratio = nCnt / fastaCnt * 100;

            return ratio < (double)cutoffRatio;
        }
    }
}
