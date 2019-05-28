using System;
using System.IO;

namespace Compressor
{
    class ArgsChecker
    {
        public static void StringReadValidation(string[] args)
        {

            if (args.Length < 3 || (args[0].ToLower().Equals("compress") && args.Length != 4) || (args[0].ToLower().Equals("decompress") && args.Length != 3))
            {
                throw new Exception("Please enter arguments up to the following pattern:\ncompress [Source file] [Destination file] [compression block size]\ndecompress [Source file] [Destination file]\n");
            }

            int x = 0;

            if (args[0].ToLower().Equals("compress") && !Int32.TryParse(args[3], out x))
            {
                throw new Exception("Compress block size should be valid integer value.");
            }

            if (args[0].ToLower() != "compress" && args[0].ToLower() != "decompress")
            {
                throw new Exception("First argument shall be \"compress\" or \"decompress\".");
            }

            if (args[1].Length == 0)
            {
                throw new Exception("No source file name was specified.");
            }

            if (!File.Exists(args[1]))
            {
                throw new Exception("No source file was found.");
            }

            FileInfo _fileIn = new FileInfo(args[1]);
            FileInfo _fileOut = new FileInfo(args[2]);

            if (args[1] == args[2])
            {
                throw new Exception("Source and destination files shall be different.");
            }

            if (args[2].Length == 0)
            {
                throw new Exception("No destination file name was specified.");
            }
        }
    }
}
