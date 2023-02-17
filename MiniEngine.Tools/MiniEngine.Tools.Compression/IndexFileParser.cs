using System.Linq.Expressions;

namespace MiniEngine.Tools.Compression
{
    public class IndexFileParser
    {
        private readonly StreamReader _stream;
        private readonly string _dirRoot;
        private readonly Dictionary<string, FileInfo> _files = new();

        public IReadOnlyDictionary<string, FileInfo> Files => _files;

        private IndexFileParser(string path, string dirRoot = "")
        {
            _stream = File.OpenText(path);
            _dirRoot = dirRoot;
        }

        public static IndexFileParser Open(string path)
            => new(path);

        public void Parse()
        {
            var parsedFiles = new Dictionary<string, FileInfo>();
            while (_stream.ReadLine() is { } line)
            {
                var fullPath = Path.Join(_dirRoot, line);
                if (IsFile(fullPath))
                    parsedFiles.TryAdd(fullPath.ToLower(), new FileInfo(fullPath));

                if (IsFolder(fullPath))
                    _ = GetFilesRecursively(fullPath)
                        .All(file =>
                        {
                            var (path, fileInfo) = file;
                            parsedFiles.TryAdd(path.ToLower(), fileInfo);
                            return true;
                        });
            }
            _files.Clear();
            foreach (var kp in parsedFiles)
                _files.TryAdd(kp.Key, kp.Value);
        }

        public void Close()
            => _stream.Close();

        private static bool IsFolder(string path)
            => Directory.Exists(path);

        private static bool IsFile(string path)
            => File.Exists(path);

        private static IEnumerable<(string, FileInfo)> GetFilesRecursively(string path)
        {
            if (!IsFolder(path))
                return Array.Empty<(string, FileInfo)>();
            var dirInfo = new DirectoryInfo(path);
            var files = dirInfo.GetFiles();
            var ret = files.Select(file => (Path.Join(path, file.Name), file)).ToList();
            var dirs = dirInfo.GetDirectories();
            return dirs.Aggregate(ret,
                (current, dir)
                    => current.Concat(GetFilesRecursively(Path.Join(path, dir.Name)))
                        .ToList());
        }
    }
}