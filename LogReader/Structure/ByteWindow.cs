using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Converters;

namespace LogReader.Structure
{
    public class ByteWindow
    {
        public long StartingByte { get; set; }

        public long EndingByte { get; set; }
    }
}