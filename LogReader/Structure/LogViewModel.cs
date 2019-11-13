using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogReader.Akka.Net.Actors;

namespace LogReader.Structure
{
    public class LogViewModel
    {
        private int _currentLineToUpdate = 0;
        private List<long> _logLineStartBytes { get; set; }
        private List<long> _logLineEndBytes { get; set; }

        private const int MaximumLines = 20;
        public ObservableCollection<LogLine> LogLines { get; private set; }

        public ByteWindow OnScreenLines { get; private set; }

        public long FirstLineStartingByte => _logLineStartBytes[0];

        public long FirstLineEndingByte => _logLineStartBytes[LogLines.Count - 1];

        public LogViewModel()
        {
            LogLines = new ObservableCollection<LogLine>();
            _logLineStartBytes = new List<long>();
            _logLineEndBytes = new List<long>();
        }

        /// <summary>
        /// Adds a new line in the correct position to the list of LogLines to be displayed.
        /// </summary>
        /// <param name="lineToAdd">The <see cref="string"/> to be added.</param>
        public void AddLine(ReadLineFromFileActor.ReturnedLine lineToAdd)
        {
            if (LogLines.Count > 20)
            {
                if (_currentLineToUpdate == MaximumLines)
                {
                    _currentLineToUpdate = 0;
                }

                LogLines[_currentLineToUpdate] = CreateNewLogLine(lineToAdd);
                _logLineStartBytes[_currentLineToUpdate] = lineToAdd.LineStartsAtByteLocation;
                _logLineEndBytes[_currentLineToUpdate] = lineToAdd.LineEndsAtByteLocation;

                _currentLineToUpdate++;
            }
            else
            {
                LogLines.Add(CreateNewLogLine(lineToAdd));
                _logLineStartBytes.Add(lineToAdd.LineStartsAtByteLocation);
                _logLineEndBytes.Add(lineToAdd.LineEndsAtByteLocation);
            }
        }

        private LogLine CreateNewLogLine(ReadLineFromFileActor.ReturnedLine line)
        {
            return new LogLine
            {
                Line = line.Line
            };
        }
    }
}
