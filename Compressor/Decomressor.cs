using System.IO;
using System.IO.Compression;
using System;

namespace Compressor
{
    class Decomressor
    {
        private string _inputFile, _outputFile;
        private byte[] _buffer;

        public Decomressor(string inputFile, string outputFile, int chunkSize)
        {
            _inputFile = inputFile;
            _outputFile = outputFile;
            if (File.Exists(outputFile))
            {
                File.Delete(outputFile);
            }
            _buffer = new byte[chunkSize];
        }

        public bool DecompressSingle()
        {
            byte[] file = File.ReadAllBytes(_inputFile);
            byte[] _outputBuffer = new byte[_buffer.Length];
            using (GZipStream gzipStream = new GZipStream(new MemoryStream(file), CompressionMode.Decompress))
            {
                const int size = 4096;
                byte[] buffer = new byte[size];
                byte[] outputBuffer;
                using (MemoryStream memory = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = gzipStream.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    }
                    while (count > 0);
                    outputBuffer = memory.ToArray();
                }
                using (FileStream _outputFileStream = new FileStream(_outputFile, FileMode.Create))
                {
                    _outputFileStream.Write(outputBuffer, 0, outputBuffer.Length);
                }
            }
            return true;
        }

        public bool Decompress()
        {
            using (FileStream _inputFileStream = new FileStream(_inputFile, FileMode.Open))
            {
                while (_inputFileStream.Position < _inputFileStream.Length)
                {
                    byte[] lengthBytes = new byte[4];
                    _inputFileStream.Read(lengthBytes, 0, 4);
                    var lengthCompressed = BitConverter.ToInt32(lengthBytes, 0);
                    byte[] _compressedBuffer = new byte[lengthCompressed];
                    _inputFileStream.Read(lengthBytes, 0, 4);
                    var lengthOriginal = BitConverter.ToInt32(lengthBytes, 0);
                    _inputFileStream.Read(_compressedBuffer, 0, lengthCompressed);
                    using (GZipStream gzipStream = new GZipStream(new MemoryStream(_compressedBuffer), CompressionMode.Decompress))
                    {
                        var result = new byte[lengthOriginal];
                        gzipStream.Read(result, 0, lengthOriginal);
                        using (FileStream _outputFileStream = new FileStream(_outputFile, FileMode.Append))
                        {
                            _outputFileStream.Write(result, 0, result.Length);
                        }
                    }
                }
            }
            return true;
        }
    }
}
