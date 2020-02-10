using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Log4Net.Extensions.Configuration.Implementation.LogObjects;
using LogReader.Annotations;

namespace LogReader.Structure
{
    public class LogLine
    {
        public string Name { get; set; }
        public ObservableCollection<LogLineField> Fields { get; set; }
        public string RawLine { get; set; }
        public long StartingByte { get; set; }
        public long EndingByte { get; set; }
        public string File { get; set; }
    }
}
