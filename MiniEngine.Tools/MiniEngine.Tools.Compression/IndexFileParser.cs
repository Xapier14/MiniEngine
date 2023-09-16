namespace MiniEngine.Tools.Compression
{
    public class IndexFileParser
    {
        private readonly StreamReader _stream;
        private readonly string _dirRoot;
        private readonly Dictionary<string, FileInfo> _files = new();

        public IReadOnlyDictionary<string, FileInfo> Files => _files;

        private IndexFileParser(string path)
        {
            _stream = File.OpenText(path);
            _dirRoot = new FileInfo(path).DirectoryName ?? string.Empty;
        }

        public static IndexFileParser Open(string path)
            => new(path);

        public void Parse()
        {
            var parsedFiles = new Dictionary<string, FileInfo>();
            while (_stream.ReadLine() is { } line)
            {
                if (IsFile(Path.Join(_dirRoot, line)))
                    parsedFiles.TryAdd(line.ToLower(), new FileInfo(Path.Join(_dirRoot, line)));

                if (IsFolder(Path.Join(_dirRoot, line)))
                    _ = GetFilesRecursively(line, _dirRoot)
                        .All(file =>
                        {
                            var (path, fileInfo) = file;
                            parsedFiles.TryAdd(path.ToLower(), fileInfo);
                            return true;
                        });
            }
            _files.Clear();
            foreach (var (relativePath, file) in parsedFiles)
            {
                if (relativePath == "/.indexfile")
                    continue;
                _files.TryAdd(relativePath, file);
            }
        }

        public void Close()
            => _stream.Close();

        private static bool IsFolder(string path)
            => Directory.Exists(path);

        private static bool IsFile(string path)
            => File.Exists(path);

        private static IEnumerable<(string, FileInfo)> GetFilesRecursively(string path, string dirRoot = "")
        {
            if (!IsFolder(Path.Join(dirRoot, path)))
                return Array.Empty<(string, FileInfo)>();
            var dirInfo = new DirectoryInfo(Path.Join(dirRoot, path));
            if (dirInfo.Name.Contains('.'))
                throw new InvalidDataException("Directory name cannot contain '.'.");
            var files = dirInfo.GetFiles();
            var ret = files.Select(file => (Path.Join(path, file.Name), file)).ToList();
            var dirs = dirInfo.GetDirectories();
            return dirs.Aggregate(ret,
                (current, dir)
                    => current.Concat(GetFilesRecursively(Path.Join(path, dir.Name), dirRoot))
                        .ToList());
        }
    }
}