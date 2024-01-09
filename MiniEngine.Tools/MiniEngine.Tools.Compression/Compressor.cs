using System.IO.Compression;

namespace MiniEngine.Tools.Compression
{
    public static class Compression
    {
        public static void CompressTo(Stream source, Stream destination)
        {
            using var compressor = new GZipStream(destination, CompressionLevel.Optimal, true);
            source.CopyTo(compressor);
        }

        public static void DecompressTo(
            Stream source, long sourceSeek, uint sourceLength,
            Stream destination, long destinationSeek = 0)
        {
            if (sourceSeek != 0)
                source.Seek(sourceSeek, SeekOrigin.Current);
            if (destinationSeek != 0)
                destination.Seek(destinationSeek, SeekOrigin.Current);
            using var decompressor = new GZipStream(source, CompressionMode.Decompress, true);
            CopyStream(decompressor, destination, (int)sourceLength);
        }

        private static void CopyStream(Stream input, Stream output, int bytes)
        {
            var buffer = new byte[32768];
            int read;
            while (bytes > 0 &&
                   (read = input.Read(buffer, 0, Math.Min(buffer.Length, bytes))) > 0)
            {
                output.Write(buffer, 0, read);
                bytes -= read;
            }
        }
    }
}
