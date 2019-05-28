using System;
using System.IO;
using System.IO.Compression;

namespace Compressor
{
    class Compressor
    {
        private string _inputFile, _outputFile;
        private byte[] _buffer;

        public Compressor(string inputFile, string outputFile, int chunkSize)
        {
            _inputFile = inputFile;
            _outputFile = outputFile;
            if (File.Exists(outputFile))
            {
                File.Delete(outputFile);
            }
            _buffer = new byte[chunkSize];
        }

        public bool Compress()
        {
            using (FileStream _inputFileStream = new FileStream(_inputFile, FileMode.Open))
            {
                int dataRead;
                while ((dataRead = _inputFileStream.Read(_buffer, 0, _buffer.Length)) > 0)
                {
                    using (MemoryStream _memoryStream = new MemoryStream())
                    {
                        using (GZipStream cs = new GZipStream(_memoryStream, CompressionMode.Compress))
                        {
                            cs.Write(_buffer, 0, dataRead);
                        }

                        byte[] compressedData = _memoryStream.ToArray();
                        byte[] compressedDataWithLength = new byte[compressedData.Length + 8];
                        byte[] lengthCompressed = BitConverter.GetBytes(compressedData.Length);
                        byte[] lengthOriginal = BitConverter.GetBytes(dataRead);
                        lengthCompressed.CopyTo(compressedDataWithLength, 0);
                        lengthOriginal.CopyTo(compressedDataWithLength, 4);
                        compressedData.CopyTo(compressedDataWithLength, 8);
                        using (FileStream _outputFileStream = new FileStream(_outputFile, FileMode.Append))
                        {
                            _outputFileStream.Write(compressedDataWithLength, 0, compressedDataWithLength.Length);
                        }
                    }
                    Array.Clear(_buffer, 0, _buffer.Length);
                }
            }
            return true;
        }

        public bool CompressOneBlock()
        {
            byte[] file = System.IO.File.ReadAllBytes(_inputFile);
            using (MemoryStream _memoryStream = new MemoryStream())
            {
                using (GZipStream cs = new GZipStream(_memoryStream, CompressionMode.Compress))
                {
            
                    cs.Write(file, 0, file.Length);
                }
                byte[] compressedData = _memoryStream.ToArray();
                using (FileStream _outputFileStream = new FileStream(_outputFile, FileMode.Append))
                {
                    _outputFileStream.Write(compressedData, 0, compressedData.Length);
                }
            }
            Array.Clear(_buffer, 0, _buffer.Length);
            return true;
        }
    }
}
