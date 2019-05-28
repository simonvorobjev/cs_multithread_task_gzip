using System;

namespace Compressor
{
    class Program
    {
        static BaseCompressor compressor;

        static void Main(string[] args)
        {
            /*Compressor compressorSingleBlock = new Compressor("input.txt", "outputSingle.gzm", 100);
            compressorSingleBlock.CompressOneBlock();
            Compressor compressor = new Compressor("input.txt", "output.gzm", 100);
            compressor.Compress();
            MultithreadCompressor compressor = new MultithreadCompressor("input.txt", "output.gzm", 50);
            compressor.Calculate();
            /*Decomressor decompressor = new Decomressor("output.gzm", "decomressed.txt", 100);
            decompressor.Decompress();
            MultithreadDecompressor decompressor = new MultithreadDecompressor("output.gzm", "decomressed.txt");
            decompressor.Calculate();*/

            Console.CancelKeyPress += new ConsoleCancelEventHandler(CancelKeyPress);

            ArgsChecker.StringReadValidation(args);

            switch (args[0].ToLower())
            {
                case "compress":
                    compressor = new MultithreadCompressor(args[1], args[2], Int32.Parse(args[3]));
                    break;
                case "decompress":
                    compressor = new MultithreadDecompressor(args[1], args[2]);
                    break;
            }

            compressor.Calculate();
        }

        static void CancelKeyPress(object sender, ConsoleCancelEventArgs _args)
        {
            if (_args.SpecialKey == ConsoleSpecialKey.ControlC)
            {
                Console.WriteLine("\nCancelling...");
                _args.Cancel = true;
                compressor.CancelJob();

            }
        }
    }
}
