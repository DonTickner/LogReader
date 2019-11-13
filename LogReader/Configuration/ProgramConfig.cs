using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogReader.Configuration
{
    public static class ProgramConfig
    {
        /// <summary>
        /// The number of bytes to load in a single chunk of a file.
        /// </summary>
        public const int ChunkSize = 1024;

        /// <summary>
        /// The LineFeed byte value
        /// </summary>
        public const byte LineFeedByte = 10;

        /// <summary>
        /// The NULL byte value
        /// </summary>
        public const byte NullByte = 00;
    }
}
