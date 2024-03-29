﻿using System.Text;

namespace MiniEngine.Tools.Compression
{
    public class FileOffset
    {
        public string RelativePath { get; init; } = string.Empty;
        public uint Offset { get; init; }
        public uint CompressedSize { get; internal set; }
        public uint UncompressedSize { get; init; }
        public string Name => Path.GetFileName(RelativePath);
    };

    public static class PackProber
    {
        private static readonly byte[] _magicSignature =
            Encoding.UTF8.GetBytes("MiniEngine Asset Pack Format").Append((byte)0).ToArray();
        private static readonly int _magicOffset = _magicSignature.Length;

        public static IEnumerable<FileOffset> Probe(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("File does not exist.");
            var files = new List<FileOffset>();

            var fileStream = File.OpenRead(path);
            fileStream.Seek(_magicOffset, SeekOrigin.Begin);
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

                var sizeBuffer = new byte[4];
                _ = fileStream.Read(sizeBuffer, 0, 4);
                var size = BitConverter.ToUInt32(sizeBuffer);

                if (prevOffset is not null)
                {
                    prevOffset.CompressedSize = offset - prevOffset.Offset + 1;
                    files.Add(prevOffset);
                }

                prevOffset = new FileOffset
                {
                    Offset = offset,
                    RelativePath = filePath,
                    UncompressedSize = size
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
