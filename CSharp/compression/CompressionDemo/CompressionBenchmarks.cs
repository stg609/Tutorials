using System.IO.Compression;
using BenchmarkDotNet.Attributes;
using ICSharpCode.SharpZipLib.BZip2;
using K4os.Compression.LZ4;
using ZstdNet;

namespace CompressionDemo
{
    [MemoryDiagnoser]
    public class CompressionBenchmarks
    {
        private byte[] _data;


        [Params(1024 * 5, 1024 * 30, 1024 * 1204, 1024 * 1024 * 10)]
        public int N;

        [GlobalSetup]
        public void Setup()
        {
            _data = new byte[N];
            new Random().NextBytes(_data);
        }

        [Benchmark]
        public async Task SharpZip()
        {
            await using var input = new MemoryStream(_data);
            await using var output = new MemoryStream();
            BZip2.Compress(input, output, false, 9);

            await output.FlushAsync();
        }

        [Benchmark]
        public async Task Br()
        {
            await using var input = new MemoryStream(_data);
            await using var output = new MemoryStream();
            await using var stream = new BrotliStream(output, CompressionLevel.Fastest);

            await input.CopyToAsync(stream);
            await stream.FlushAsync();
        }

        [Benchmark]
        public async Task Gzip()
        {
            await using var input = new MemoryStream(_data);
            await using var output = new MemoryStream();
            await using var stream = new GZipStream(output, CompressionLevel.Fastest);

            await input.CopyToAsync(stream);
            await stream.FlushAsync();
        }

        [Benchmark]
        public void ZStd_Min()
        {
            using var compressor = new Compressor(new CompressionOptions(CompressionOptions.MinCompressionLevel));
            var compressedData = compressor.Wrap(_data);
        }

        [Benchmark]
        public void ZStd_Max()
        {
            using var compressor = new Compressor(new CompressionOptions(CompressionOptions.MaxCompressionLevel));
            var compressedData = compressor.Wrap(_data);
        }

        [Benchmark]
        public void Lz4_Encode()
        {
            var target = new byte[LZ4Codec.MaximumOutputSize(_data.Length)];
            var encodedLength = LZ4Codec.Encode(
                _data, 0, _data.Length,
                target, 0, target.Length);
        }

        [Benchmark]
        public void Lz4_Pickler()
        {
            LZ4Pickler.Pickle(_data);
        }
    }
}
