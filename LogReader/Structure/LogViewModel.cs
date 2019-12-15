using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using LogReader.Akka.Net.Actors;
using LogReader.Annotations;
using LogReader.Configuration;

namespace LogReader.Structure
{
    public class LogViewModel: INotifyPropertyChanged
    {
        public int CurrentLineToUpdate = 0;

        public string CurrentLogFile { get; set; }

        private ObservableCollection<LogLine> _logLines = new ObservableCollection<LogLine>();
        public ObservableCollection<LogLine> LogLines
        {
            get { return _logLines; }
            set
            {
                _logLines = value;
            }
        }

        public long FirstLineStartingByte => _logLines.Any() ? _logLines[0].StartingByte : 0;

        public long FirstLineEndingByte => _logLines.Any() ? _logLines[0].EndingByte : 0;

        public long LastLineEndingByte => _logLines.Any() ? _logLines[^1].EndingByte : 0;

        private List<Tuple<string, long>> _logFileLocations;

        private List<Tuple<string, int>> _logFileLines;

        public List<string> LogFiles { get
            {
                return _logFileLocations.Select(l => l.Item1).ToList();
            }
        }

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

        public bool IsReading
        {
            get
            {
                return CurrentLineToUpdate <= LogLines.Count + 1;
            }
        }

        private bool _isExpanding = true;

        public bool ExpandingView
        {
            get
            {
                return _isExpanding;
            }
            set
            {
                _isExpanding = value;
                OnPropertyChanged(nameof(ExpandingView));
            }
        }

        public bool HasLoaded => _logLines.Any();

        public LogViewModel()
        {
            LogLines = new ObservableCollection<LogLine>();

            _logFileLines = new List<Tuple<string, int>>();
            _logFileLocations = new List<Tuple<string, long>>();
        }

        /// <summary>
        /// Adds a new line in the correct position to the list of LogLines to be displayed.
        /// </summary>
        /// <param name="lineToAdd">The <see cref="string"/> to be added.</param>
        public void AddLine(ReadLineFromFileActor.ReturnedLine lineToAdd)
        {
            if (null == lineToAdd)
            {
                return;
            }

            if (!ExpandingView)
            {
                int zeroBasedLogLineIndex = Math.Max(0, CurrentLineToUpdate - 1);

                if (zeroBasedLogLineIndex < _logLines.Count)
                {
                    _logLines[zeroBasedLogLineIndex] = CreateNewLogLine(lineToAdd);
                    CurrentLogFile = lineToAdd.FilePath;
                }
            }
            else
            { 
                _logLines.Add(CreateNewLogLine(lineToAdd));
                CurrentLogFile = lineToAdd.FilePath;
            }

            CurrentLineToUpdate++;
            OnPropertyChanged(nameof(CurrentLineToUpdate));
            OnPropertyChanged(nameof(IsReading));
            OnPropertyChanged(nameof(CurrentLogFile));
        }

        private LogLine CreateNewLogLine(ReadLineFromFileActor.ReturnedLine line)
        {
            return new LogLine
            {
                Line = line.Line,
                StartingByte = CreateRelativeByteReference(line.LineStartsAtByteLocation, line.FilePath),
                EndingByte = CreateRelativeByteReference(line.LineEndsAtByteLocation, line.FilePath),
                File = line.FilePath
            };
        }

        /// <summary>
        /// Creates a zero-based relative byte reference within the total byte window based on a position within a current file.
        /// </summary>
        /// <param name="byteLocationWithinFile">The zero-based byte location within the log file.</param>
        /// <param name="file">The log file that the byte is within.</param>
        public long CreateRelativeByteReference(long byteLocationWithinFile, string file)
        {
            long relativeByteReference = byteLocationWithinFile;
            relativeByteReference = Math.Max(0, Math.Min(_logFileLocations.FirstOrDefault(t => t.Item1 == file)?.Item2 ?? 0, relativeByteReference));

            foreach (Tuple<string, long> logFileLocation in _logFileLocations)
            {
                if (logFileLocation.Item1 == file)
                {
                    break;
                }

                relativeByteReference += logFileLocation.Item2;
            }

            return relativeByteReference;
        }

        public void SetLogFileLocations(List<Tuple<string, long>> logFileLocations)
        {
            _logFileLocations = logFileLocations;
            // CountLinesInFiles();
            OnPropertyChanged(nameof(CalculatedScrollableMaximum));
            OnPropertyChanged(nameof(LogFiles));
        }

        public string LocateLogFileFromByteReference(long byteLocation)
        {
            long clampedByteLocation = ClampByteLocation(byteLocation);

            string filePath = string.Empty;
            long byteOffset = 0;
            foreach (Tuple<string, long> logFileLocation in _logFileLocations)
            {
                if (clampedByteLocation >= (logFileLocation.Item2 + byteOffset))
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
        /// Translates a zero-based byte position from within the total byte field into a zero-based specific file location's byte.
        /// </summary>
        /// <param name="byteLocation">The starting byte location.</param>
        public long TranslateRelativeBytePosition(long byteLocation)
        {
            long clampedByteLocation = ClampByteLocation(byteLocation);

            long relativeByte = clampedByteLocation;
            long byteOffset = 0;
            foreach (Tuple<string, long> logFileLocation in _logFileLocations)
            {
                if (_logFileLocations.IndexOf(logFileLocation) == _logFileLocations.Count - 1)
                {
                    break;
                }

                if (clampedByteLocation < (logFileLocation.Item2 + byteOffset))
                {
                    continue;
                }

                relativeByte -= logFileLocation.Item2;
                byteOffset += logFileLocation.Item2;
            }

            return relativeByte;
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
            CurrentLineToUpdate = 1;
            OnPropertyChanged(nameof(CurrentLineToUpdate));
            OnPropertyChanged(nameof(IsReading));
        }

        private ByteWindow CreateNewByteWindow(long start, long end)
        {
            return new ByteWindow
            {
                StartingByte = start,
                EndingByte = end
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void CapScrollWindow()
        {
            ExpandingView = false;
            if (_logLines.Count <= 0)
            {
                return;
            }

            _logLines.RemoveAt(_logLines.Count - 1);
        }

        private long ClampByteLocation(long byteLocation)
        {
            long totalBytes = _logFileLocations.Sum(a => a.Item2) - 1;
            long clampedByteLocation = Math.Max(0, Math.Min(totalBytes, byteLocation));
            return clampedByteLocation;
        }
    }
}
