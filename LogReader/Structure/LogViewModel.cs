using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Akka.Dispatch.SysMsg;
using LogReader.Akka.Net.Actors;
using LogReader.Annotations;
using LogReader.Configuration;

namespace LogReader.Structure
{
    public class LogViewModel: INotifyPropertyChanged
    {
        private int _currentLineToUpdate = 0;

        private readonly List<ByteWindow> _onScreenlogLines;
        
        private ObservableCollection<LogLine> _logLines = new ObservableCollection<LogLine>();
        public ObservableCollection<LogLine> LogLines
        {
            get { return _logLines; }
            set
            {
                _logLines = value;
            }
        }

        public long FirstLineStartingByte => _onScreenlogLines[0].StartingByte;

        public long FirstLineEndingByte => _onScreenlogLines[0].EndingByte;

        private List<Tuple<string, long>> _logFileLocations;

        private List<Tuple<string, int>> _logFileLines;

        public long TotalFileSizesInBytes
        {
            get { return _logFileLocations.Sum(l => l.Item2); }
        }

        public long CalculatedScrollableMaximum
        {
            get { return _logFileLocations.Sum(l => l.Item2) / 10; }
        }

        public long TotalLinesInFiles
        {
            get { return _logFileLines.Sum(l => l.Item2); }
        }

        public int TotalNumberOfLogFiles => _logFileLocations.Count;

        public bool IsReading => _currentLineToUpdate <= LogLines.Count + 1;

        public bool ExpandingView = true;

        public LogViewModel()
        {
            LogLines = new ObservableCollection<LogLine>();

            _onScreenlogLines = new List<ByteWindow>();
            _logFileLines = new List<Tuple<string, int>>();
            _logFileLocations = new List<Tuple<string, long>>();
        }

        /// <summary>
        /// Adds a new line in the correct position to the list of LogLines to be displayed.
        /// </summary>
        /// <param name="lineToAdd">The <see cref="string"/> to be added.</param>
        public void AddLine(ReadLineFromFileActor.ReturnedLine lineToAdd)
        {
            ByteWindow newLine =
                CreateNewByteWindow(lineToAdd.LineStartsAtByteLocation, lineToAdd.LineEndsAtByteLocation);

            if (!ExpandingView)
            {
                int zeroBasedLogLineIndex = Math.Max(0, _currentLineToUpdate - 1);

                if (zeroBasedLogLineIndex < _logLines.Count)
                {
                    _logLines[zeroBasedLogLineIndex] = CreateNewLogLine(lineToAdd);
                    _onScreenlogLines[zeroBasedLogLineIndex] = newLine;
                }
            }
            else
            { 
                _logLines.Add(CreateNewLogLine(lineToAdd));
                _onScreenlogLines.Add(newLine);
            }

            _currentLineToUpdate++;
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
            OnPropertyChanged(nameof(CalculatedScrollableMaximum));
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
            _currentLineToUpdate = 1;
        }

        private ByteWindow CreateNewByteWindow(long start, long end)
        {
            return new ByteWindow
            {
                StartingByte = start,
                EndingByte = end
            };
        }

        public void ResetUserInterface()
        {
            InitiateNewRead();
            _logLines.Clear();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
