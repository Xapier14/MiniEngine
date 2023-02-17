using System.Text;
using MiniEngine.Tools.Compression;

namespace MiniEngine.Tools.CLI
{
    internal static class Pack
    {
        private static readonly Dictionary<string, FileInfo> _files = new();
        private static readonly Dictionary<string, uint> _offsets = new();
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
            var parser = IndexFileParser.Open(Path.Join(path, INDEX_FILE));
            parser.Parse();
            parser.Close();
            _files.Clear();
            foreach (var (relativePath, fileInfo) in parser.Files)
            {
                _files.TryAdd(relativePath, fileInfo);
            }
        }

        public static void WriteIndex(FileStream fileStream)
        {
            var placeholder = new byte[4];
            fileStream.Seek(MAGIC_OFFSET, SeekOrigin.Begin);
            var countBuffer = BitConverter.GetBytes((uint)_files.Count);
            fileStream.Write(countBuffer, 0, 4);
            foreach (var (path, fileInfo) in _files)
            {
                var encodedPath = Encoding.UTF8.GetBytes(path);
                if (encodedPath.Length > ushort.MaxValue)
                {
                    Console.Error.WriteLine("Path is too long! Maximum length is {0}.", ushort.MaxValue);
                    Console.Error.WriteLine("File: {0}", path);
                    Environment.Exit(-3);
                }
                var encodedLength = BitConverter.GetBytes((ushort)encodedPath.Length);
                fileStream.Write(encodedLength, 0, 2);
                fileStream.Write(encodedPath, 0, encodedPath.Length);
                fileStream.Write(placeholder, 0, 4);
                var encodedSize = BitConverter.GetBytes((uint)fileInfo.Length);
                fileStream.Write(encodedSize, 0, 4);
            }
        }

        public static void CompressFiles(string path, FileStream fileStream)
        {
            foreach (var (filePath, fileInfo) in _files)
            {
                var currentOffset = (uint)fileStream.Position;
                var fullPath = Path.Join(path, filePath);
                using var sourceStream = File.OpenRead(fullPath);
                Console.Write("{0}...", filePath);
                Compression.Compression.CompressTo(sourceStream, fileStream);
                _totalSizeInBytes += sourceStream.Length;
                _offsets.TryAdd(filePath, currentOffset);
                Console.WriteLine(" OK");
            }
        }

        public static void WriteOffsets(FileStream fileStream)
        {
            fileStream.Seek(MAGIC_OFFSET + 4, SeekOrigin.Begin);
            foreach (var (path, offset) in _offsets)
            {
                fileStream.Seek(path.Length + 2, SeekOrigin.Current);
                var encodedOffset = BitConverter.GetBytes(offset);
                fileStream.Write(encodedOffset, 0, 4);
                fileStream.Seek(4, SeekOrigin.Current);
            }
        }

        public static FileStream CreateOutputFile(string outFile)
        {
            if (File.Exists(outFile))
                File.Delete(outFile);
            var fileStream = File.OpenWrite(outFile);
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
            Console.WriteLine("Total: {0} file(s)", _files.Count);
            Console.WriteLine();
            
            var fileStream = CreateOutputFile(outFile);
            Console.Write("Writing index...");
            WriteIndex(fileStream);
            Console.WriteLine(" OK");
            Console.WriteLine();

            Console.WriteLine("Compressing files...");
            CompressFiles(path, fileStream);
            Console.WriteLine();

            Console.Write("Writing offsets...");
            WriteOffsets(fileStream);
            Console.WriteLine(" OK");
            Console.WriteLine();

            fileStream.Close();
            var fileSize = GetFinishedFileSize(fileStream.Name);
            Console.WriteLine("Finished writing to file!");
            Console.WriteLine("Output is {0} MB in size. Compression ratio: {1}%",
                fileSize * 0.000001,
                _totalSizeInBytes > 0
                    ? ((double)_totalSizeInBytes / fileSize) * 100.0
                    : "NaN");
        }
    }
}
