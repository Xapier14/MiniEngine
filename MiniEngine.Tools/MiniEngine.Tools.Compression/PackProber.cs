using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace MiniEngine.Tools.Compression
{
    public class FileOffset
    {
        public string RelativePath { get; init; }
        public uint Offset { get; init; }
        public uint CompressedSize { get; internal set; }
        public string Name => Path.GetFileName(RelativePath);
    };

    public static class PackProber
    {
        private static readonly byte[] MAGIC_SIGNATURE =
            Encoding.UTF8.GetBytes("MiniEngine Asset Pack Format").Append((byte)0).ToArray();
        private static readonly int MAGIC_OFFSET = MAGIC_SIGNATURE.Length;

        public static IEnumerable<FileOffset> Probe(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("File does not exist.");
            var files = new List<FileOffset>();

            var fileStream = File.OpenRead(path);
            fileStream.Seek(MAGIC_OFFSET, SeekOrigin.Begin);
            var countBuffer = new byte[4];
            _ = fileStream.Read(countBuffer, 0, 4);
            var fileCount = BitConverter.ToUInt32(countBuffer);

            FileOffset? prevOffset = null;

            for (var i = 0; i < fileCount; i++)
            {
                var lengthBuffer = new byte[2];
                _ = fileStream.Read(lengthBuffer, 0, 2);
                var pathLength = BitConverter.ToUInt16(lengthBuffer);
                var pathBuffer = new byte[pathLength];
                _ = fileStream.Read(pathBuffer, 0, pathLength);
                var filePath = Encoding.UTF8.GetString(pathBuffer);
                var offsetBuffer = new byte[4];
                _ = fileStream.Read(offsetBuffer, 0, 4);
                var offset = BitConverter.ToUInt32(offsetBuffer);

                if (prevOffset is not null)
                {
                    prevOffset.CompressedSize = offset - prevOffset.Offset + 1;
                    files.Add(prevOffset);
                }

                prevOffset = new FileOffset
                {
                    Offset = offset,
                    RelativePath = filePath
                };
            }

            if (prevOffset is null)
                return files;
            prevOffset.CompressedSize = (uint)(fileStream.Length - prevOffset.Offset + 1);
            files.Add(prevOffset);

            fileStream.Close();
            return files;
        }
    }
}
