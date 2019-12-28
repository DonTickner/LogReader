using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using LogReader.Annotations;

namespace LogReader.Structure
{
    public class LogLine
    {
        public string Line { get; set; }
        public long StartingByte { get; set; }
        public long EndingByte { get; set; }
        public string File { get; set; }

        public override string ToString()
        {
            return Line;
        }
    }
}
