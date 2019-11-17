using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogReader.Akka.Net.Actors;
using LogReader.Configuration;

namespace LogReader.Structure
{
    public class LogViewModel: NotifyPropertyChanged
    {
        private int _currentLineToUpdate = 0;

        private List<long> _logLineStartBytes { get; set; }

        private List<long> _logLineEndBytes { get; set; }

        private const int MaximumLines = 20;

        private ObservableCollection<LogLine> _logLines = new ObservableCollection<LogLine>();
        public ObservableCollection<LogLine> LogLines
        {
            get { return _logLines; }
            set
            {
                _logLines = value;
                OnPropertyChanged(nameof(LogLines));
            }
        }

        public ByteWindow OnScreenLines { get; private set; }

        public long FirstLineStartingByte => _logLineStartBytes[0];

        public long FirstLineEndingByte => _logLineStartBytes[LogLines.Count - 1];

        private List<Tuple<string, long>> _logFileLocations;

        private List<Tuple<string, int>> _logFileLines;

        public long TotalFileSizesInBytes
        {
            get { return _logFileLocations.Sum(l => l.Item2); }
        }

        public long TotalLinesInFiles
        {
            get { return _logFileLines.Sum(l => l.Item2); }
        }

        public int TotalNumberOfLogFiles => _logFileLocations.Count;

        public bool NeedToReadMoreLines => _currentLineToUpdate < MaximumLines;

        public LogViewModel()
        {
            LogLines = new ObservableCollection<LogLine>();
            _logLineStartBytes = new List<long>();
            _logLineEndBytes = new List<long>();

            _logFileLines = new List<Tuple<string, int>>();
            _logFileLocations = new List<Tuple<string, long>>();

            OnScreenLines = new ByteWindow();
        }

        /// <summary>
        /// Adds a new line in the correct position to the list of LogLines to be displayed.
        /// </summary>
        /// <param name="lineToAdd">The <see cref="string"/> to be added.</param>
        public void AddLine(ReadLineFromFileActor.ReturnedLine lineToAdd)
        {
            if (LogLines.Count >= MaximumLines)
            {
                int zeroIndex = Math.Max(0, _currentLineToUpdate - 1);
                _logLines[zeroIndex] = CreateNewLogLine(lineToAdd);
                _logLineStartBytes[zeroIndex] = lineToAdd.LineStartsAtByteLocation;
                _logLineEndBytes[zeroIndex] = lineToAdd.LineEndsAtByteLocation;
            }
            else
            {
                if (!LogLines.Any())
                {
                    OnScreenLines.StartingByte = lineToAdd.LineStartsAtByteLocation;
                }

                _logLines.Add(CreateNewLogLine(lineToAdd));
                _logLineStartBytes.Add(lineToAdd.LineStartsAtByteLocation);
                _logLineEndBytes.Add(lineToAdd.LineEndsAtByteLocation);
            }

            OnScreenLines.EndingByte = lineToAdd.LineEndsAtByteLocation;
            _currentLineToUpdate = Math.Min(_currentLineToUpdate + 1, MaximumLines);

            OnPropertyChanged(nameof(LogLines));
        }

        private LogLine CreateNewLogLine(ReadLineFromFileActor.ReturnedLine line)
        {
            return new LogLine
            {
                Line = line.Line
            };
        }

        public void SetLogFileLocations(List<Tuple<string, long>> logFileLocations)
        {
            _logFileLocations = logFileLocations;
            CountLinesInFiles();
        }

        public string LocateLogFileFromByteReference(long byteLocation)
        {
            string filePath = string.Empty;
            long byteOffset = 0;
            foreach (Tuple<string, long> logFileLocation in _logFileLocations)
            {
                if (byteLocation > (logFileLocation.Item2 + byteOffset))
                {
                    byteOffset += logFileLocation.Item2;
                    continue;
                }

                filePath = logFileLocation.Item1;
                break;
            }

            return filePath;
        }

        /// <summary>
        /// Counts number of physical lines within the current file
        /// </summary>
        private void CountLinesInFiles()
        {
            _logFileLines = new List<Tuple<string, int>>();
            foreach (Tuple<string, long> logFile in _logFileLocations)
            {
                byte[] buffer = new byte[ProgramConfig.ChunkSize];
                int bytesRead = 0;
                int linesInFile = 0;

                using (FileStream fileStream = new FileStream(logFile.Item1, FileMode.Open, FileAccess.Read))
                {
                    while (bytesRead < logFile.Item2)
                    {
                        fileStream.Seek(Convert.ToInt32(bytesRead), SeekOrigin.Begin);
                        fileStream.Read(buffer, 0, ProgramConfig.ChunkSize);
                        linesInFile += CountLinesInChunk(buffer);
                        bytesRead += ProgramConfig.ChunkSize;
                    }
                }

                _logFileLines.Add(new Tuple<string, int>(logFile.Item1, linesInFile));
            }
        }

        /// <summary>
        /// Counts the number of lines in the passed byte array
        /// </summary>
        /// <param name="bytesToRead">The byte array that represent a memory chunk</param>
        private int CountLinesInChunk(byte[] bytesToRead)
        {
            int linesInFile = 0;

            for (int i = 0; i < bytesToRead.Length; i++)
            {
                linesInFile += bytesToRead[i] == ProgramConfig.LineFeedByte ? 1 : 0;
            }

            return linesInFile;
        }

        public void InitiateNewRead()
        {
            _currentLineToUpdate = 0;
        }
    }
}
