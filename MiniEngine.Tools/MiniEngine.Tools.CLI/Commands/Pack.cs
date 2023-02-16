using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MiniEngine.Tools.CLI
{
    internal static class Pack
    {
        private static readonly List<string> _folders = new();
        private static readonly List<string> _files = new();
        private static long _totalSizeInBytes;
        private const string INDEX_FILE = ".indexfile";

        private static readonly byte[] MAGIC_SIGNATURE =
            Encoding.UTF8.GetBytes("MiniEngine Asset Pack Format").Append((byte)0).ToArray();
        private static readonly int MAGIC_OFFSET = MAGIC_SIGNATURE.Length;

        public static bool CheckIfIndexFileIsMissing(string path)
        {
            var dir = new DirectoryInfo(path);
            var enumerator = dir.EnumerateFiles();
            return enumerator.All(file => !file.Name.Equals(INDEX_FILE, StringComparison.CurrentCultureIgnoreCase));
        }

        public static bool CheckIfAssetFileAlreadyExists(string outFile)
        {
            return File.Exists(outFile);
        }

        public static void ParseIndexFile(string path)
        {

        }

        public static void CompressFiles(FileStream fileStream)
        {

        }

        public static void WriteIndex(FileStream fileStream)
        {

        }

        public static FileStream CreateOutputFile(string outFile)
        {
            if (File.Exists(outFile))
                File.Delete(outFile);
            var fileStream = File.Create(outFile);
            fileStream.Write(MAGIC_SIGNATURE, 0, MAGIC_SIGNATURE.Length);
            return fileStream;
        }

        public static long GetFinishedFileSize(string absolutePathOfFile)
            => new FileInfo(absolutePathOfFile).Length;

        public static void DoAction(string path, string outFile)
        {
            if (CheckIfIndexFileIsMissing(path))
            {
                Console.Error.WriteLine("Asset root directory invalid! \"{0}\" is missing from directory.", INDEX_FILE);
                Environment.Exit(-1);
            }

            if (CheckIfAssetFileAlreadyExists(outFile))
            {
                Console.WriteLine("Output pack file \"{0}\" already exists.", outFile);
                Console.Write("Do you want to overwrite it? (y/n): ");
                char key;
                do
                {
                    key = char.ToLower(Console.ReadKey(true).KeyChar);
                } while (key != 'y' && key != 'n');
                Console.WriteLine(key);

                if (key == 'n')
                {
                    Console.Error.WriteLine("Cancelling operation...");
                    Environment.Exit(-2);
                }

                Console.WriteLine("Operation will overwrite existing file.");
                Console.WriteLine();
            }
            Console.Write("Reading index file...");
            ParseIndexFile(path);
            Console.WriteLine(" OK");

            Console.Write("Compressing files...");
            var fileStream = CreateOutputFile(outFile);
            CompressFiles(fileStream);
            Console.WriteLine(" OK");

            Console.Write("Writing index...");
            WriteIndex(fileStream);
            Console.WriteLine(" OK");

            fileStream.Close();
            var fileSize = GetFinishedFileSize(fileStream.Name);
            Console.WriteLine("Finished writing to file!");
            Console.WriteLine("Output is {0} MB in size. Compression ratio: {1}", fileSize * 0.000001, _totalSizeInBytes / fileSize);
            Console.WriteLine();
            Console.WriteLine("Total");
            Console.WriteLine("\tFiles: {0}", _files.Count);
            Console.WriteLine("\tDirectories: {0}", _folders.Count);
        }
    }
}
