using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Tools.Compression
{
    public static class Compression
    {
        public static void CompressTo(Stream source, Stream destination)
        {
            using var compressor = new GZipStream(destination, CompressionLevel.SmallestSize, true);
            source.CopyTo(compressor);
        }

        public static void DecompressTo(
            Stream source, uint sourceSeek, uint sourceLength,
            Stream destination, uint destinationSeek = 0)
        {
            source.Seek(sourceSeek, SeekOrigin.Current);
            destination.Seek(destinationSeek, SeekOrigin.Current);
            using var decompressor = new GZipStream(source, CompressionMode.Decompress);
            decompressor.CopyTo(destination, (int)sourceLength);
        }
    }
}
