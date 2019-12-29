using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using LogReader.Akka.Net.Actors;
using LogReader.Annotations;
using LogReader.Configuration;

namespace LogReader.Structure
{
    public class LogViewModel: INotifyPropertyChanged
    {
        public int CurrentLineToUpdate = 0;

        private string _currentLogFile;
        public string CurrentLogFile
        {
            get { return _currentLogFile; }
            set
            {
                _currentLogFile = value;
                OnPropertyChanged(nameof(CurrentLogFile));
                OnPropertyChanged(nameof(LinesInCurrentLogFile));
            }
        }

        public long LinesInCurrentLogFile
        {
            get { return _logFiles.FirstOrDefault(l => l.FileLocation == CurrentLogFile)?.NumberOfLinesInFile ?? 0; }
        }

        private ObservableCollection<LogLine> _logLines = new ObservableCollection<LogLine>();
        public ObservableCollection<LogLine> LogLines
        {
            get { return _logLines; }
            set
            {
                _logLines = value;
            }
        }

        public string LogLineBlock
        {
            get { return String.Join(string.Empty, _logLines); }
            set { return; }
        }

        public long FirstLineStartingByte => _logLines.Any() ? _logLines[0].StartingByte : 0;

        public long FirstLineEndingByte => _logLines.Any() ? _logLines[0].EndingByte : 0;

        public long LastLineEndingByte => _logLines.Any() ? _logLines[^1].EndingByte : 0;

        private List<LogFileInfo> _logFiles;
        public List<LogFileInfo> LogFiles
        {
            get { return _logFiles; }
            set { _logFiles = value; }
        }

        public List<string> LogFileLocations
        { 
            get { return _logFiles.Select(l => l.FileLocation).ToList(); }
        }

        public long TotalFileSizesInBytes
        {
            get { return _logFiles.Sum(l => l.FileSizeInByte); }
        }

        public long CalculatedScrollableMaximum
        {
            get { return TotalFileSizesInBytes / 10; }
        }

        public long TotalNumberOfLinesInAllLogFiles
        {
            get { return _logFiles.Sum(l => l.NumberOfLinesInFile); }
        }

        public int TotalNumberOfLogFiles => _logFiles.Count;

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

            _logFiles = new List<LogFileInfo>();

            FileControlVisibility = Visibility.Collapsed;
        }

        private Visibility _fileControlVisibility;

        public Visibility FileControlVisibility
        {
            get { return _fileControlVisibility; }
            set
            {
                _fileControlVisibility = value;
                OnPropertyChanged(nameof(FileControlVisibility));
            }
        }

        private bool _rawDisplayMode;
        public bool RawDisplayMode
        {
            get { return _rawDisplayMode; }
            set
            {
                _rawDisplayMode = value;
                OnPropertyChanged(nameof(RawDisplayMode));
                if (_rawDisplayMode)
                {
                    OnPropertyChanged(nameof(LogLineBlock));
                }
            }
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

            if (RawDisplayMode)
            {
                if (CurrentLineToUpdate >= _logLines.Count)
                {
                    OnPropertyChanged(nameof(LogLineBlock));
                }
            }
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
            relativeByteReference = Math.Max(0, Math.Min(_logFiles.FirstOrDefault(t => t.FileLocation == file)?.FileSizeInByte ?? 0, relativeByteReference));

            foreach (LogFileInfo logFileLocation in _logFiles)
            {
                if (logFileLocation.FileLocation == file)
                {
                    break;
                }

                relativeByteReference += logFileLocation.FileSizeInByte;
            }

            return relativeByteReference;
        }

        public void CreateLogFiles(List<Tuple<string, long>> logFileLocations)
        {
            foreach (Tuple<string, long> logFileLocation in logFileLocations)
            {
                _logFiles.Add(new LogFileInfo(0, logFileLocation.Item1, logFileLocation.Item2));
            }

            OnPropertyChanged(nameof(CalculatedScrollableMaximum));
            OnPropertyChanged(nameof(LogFiles));
            OnPropertyChanged(nameof(LogFileLocations));
        }

        public string LocateLogFileFromByteReference(long byteLocation)
        {
            long clampedByteLocation = ClampByteLocation(byteLocation);

            string filePath = string.Empty;
            long byteOffset = 0;
            foreach (LogFileInfo logFile in _logFiles)
            {
                if (clampedByteLocation >= (logFile.FileSizeInByte + byteOffset))
                {
                    byteOffset += logFile.FileSizeInByte;
                    continue;
                }

                filePath = logFile.FileLocation;
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
            foreach (LogFileInfo logFile in _logFiles)
            {
                if (_logFiles.IndexOf(logFile) == _logFiles.Count - 1)
                {
                    break;
                }

                if (clampedByteLocation < (logFile.FileSizeInByte + byteOffset))
                {
                    continue;
                }

                relativeByte -= logFile.FileSizeInByte;
                byteOffset += logFile.FileSizeInByte;
            }

            return relativeByte;
        }

        public void InitiateNewRead()
        {
            CurrentLineToUpdate = 1;
            OnPropertyChanged(nameof(CurrentLineToUpdate));
            OnPropertyChanged(nameof(IsReading));
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
            long totalBytes = _logFiles.Sum(a => a.FileSizeInByte) - 1;
            long clampedByteLocation = Math.Max(0, Math.Min(totalBytes, byteLocation));
            return clampedByteLocation;
        }

        public void SetLogFileLineCount(string logFile, long lineCount)
        {
            _logFiles.FirstOrDefault(l => l.FileLocation == logFile)?.SetLineCount(lineCount);
            OnPropertyChanged(nameof(LinesInCurrentLogFile));
            OnPropertyChanged(nameof(TotalNumberOfLinesInAllLogFiles));
        }
    }
}
