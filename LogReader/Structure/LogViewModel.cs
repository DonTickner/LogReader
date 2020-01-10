using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Data;
using Akka.Actor;
using Akka.Configuration;
using Log4Net.Extensions.Configuration.Implementation;
using Log4Net.Extensions.Configuration.Implementation.ConfigObjects;
using LogReader.Akka.Net.Actors;
using LogReader.Annotations;
using LogReader.Configuration;

namespace LogReader.Structure
{
    public class LogViewModel: INotifyPropertyChanged
    {
        #region Log4Net

        private Log4NetConfig _log4NetConfig;

        #endregion

        #region Akka.Net

        private ActorSystem _akkaActorSystem;

        private IActorRef _findStartingByteActor;
        private IActorRef _readLineFromFileActor;
        private IActorRef _updateDataSourceActor;
        private IActorRef _updateUIActor;

        #endregion

        #region Log Files

        /// <summary>
        /// The full name of the currently viewed Log File.
        /// </summary>
        private string _currentLogFile;
        /// <summary>
        /// The full name of the currently viewed Log File.
        /// </summary>
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

        /// <summary>
        /// The list of <see cref="LogFileInfo"/>s that represent the Log Files currently loaded.
        /// </summary>
        private List<LogFileInfo> _logFiles;
        /// <summary>
        /// The <see cref="LogFileInfo"/> for all loaded Log Files.
        /// </summary>
        public List<LogFileInfo> LogFiles
        {
            get { return _logFiles; }
            private set
            {
                _logFiles = value;
            }
        }

        /// <summary>
        /// A list of <see cref="string"/>s that represents the file paths of all loaded Log Files.
        /// </summary>
        public List<string> LogFileLocations
        { 
            get { return _logFiles.Select(l => l.FileLocation).ToList(); }
        }

        /// <summary>
        /// The sum total file size of all currently loaded Log Files.
        /// </summary>
        public long TotalFileSizesInBytes
        {
            get { return _logFiles.Sum(l => l.FileSizeInByte); }
        }

        /// <summary>
        /// The size of the current Log File.
        /// </summary>
        public long CurrentLogFileSizeInBytes
        {
            get { return _logFiles.Where(l => l.FileLocation == CurrentLogFile).Sum(l => l.FileSizeInByte); }
        }

        #endregion

        #region Log Lines

        /// <summary>
        /// The current Log Line displayed on screen to be updated.
        /// </summary>
        private int _currentLineToUpdate = 0;

        /// <summary>
        /// The total number of lines within the current Log File.
        /// </summary>
        public long LinesInCurrentLogFile
        {
            get { return _logFiles.FirstOrDefault(l => l.FileLocation == CurrentLogFile)?.NumberOfLinesInFile ?? 0; }
        }

        private ObservableCollection<LogLine> _logLines = new ObservableCollection<LogLine>();
        /// <summary>
        /// The <see cref="ObservableCollection{T}"/> of <see cref="LogLine"/>s to be displayed.
        /// </summary>
        public ObservableCollection<LogLine> LogLines
        {
            get { return _logLines; }
            set
            {
                _logLines = value;
            }
        }

        /// <summary>
        /// A converted block of text that represents the currently displayed <see cref="LogLine"/>s.
        /// </summary>
        public string LogLineBlock
        {
            get { return String.Join(string.Empty, _logLines); }
            set { return; }
        }

        /// <summary>
        /// The starting byte position of the first line displayed.
        /// </summary>
        public long FirstLineStartingByte => _logLines.Any() ? _logLines[0].StartingByte : 0;

        /// <summary>
        /// The ending byte position of the first line displayed.
        /// </summary>
        public long FirstLineEndingByte => _logLines.Any() ? _logLines[0].EndingByte : 0;

        /// <summary>
        /// The ending byte position of the last line displayed.
        /// </summary>
        public long LastLineEndingByte => _logLines.Any() ? _logLines[^1].EndingByte : 0;

        /// <summary>
        /// The sum total of all lines in all loaded Log files.
        /// </summary>
        public long TotalNumberOfLinesInAllLogFiles
        {
            get { return _logFiles.Sum(l => l.NumberOfLinesInFile); }
        }

        #endregion

        #region Search

        private List<SearchResult> _searchResults;
        private ICollectionView _searchResultView;
        public ICollectionView SearchResultsView
        {
            get { return _searchResultView; }
        }

        public long NumberOfSearchResults
        {
            get { return _searchResults.Count; }
        }

        #endregion
        
        #region UI

        /// <summary>
        /// The maximum scroll value based on the current sum total file size of all loaded Log Files.
        /// </summary>
        public long CalculatedScrollableMaximum
        {
            get
            {
                if (!_seamlessScroll)
                {
                    return CurrentLogFileSizeInBytes / 10;
                }

                return TotalFileSizesInBytes / 10;
            }
        }

        /// <summary>
        /// Represents if the system is currently reading from a Log File.
        /// </summary>
        public bool IsReading
        {
            get
            {
                return _currentLineToUpdate <= LogLines.Count + 1;
            }
        }

        private bool _isExpanding = true;
        /// <summary>
        /// Represents if the system is currently expanding the number of lines to display.
        /// </summary>
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

        /// <summary>
        /// Determines if the <see cref="LogViewModel"/> represents loaded data, or if a load has not yet been completed.
        /// </summary>
        public bool HasLoaded => _logLines.Any();

        private Visibility _fileControlVisibility;
        /// <summary>
        /// Determines the <see cref="Visibility"/> of File Controls elements.
        /// </summary>
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
        /// <summary>
        /// Determines if system is currently displaying delimited or unformatted Log Lines.
        /// </summary>
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

        private long _currentScrollPosition;
        /// <summary>
        /// The current Manual Scroll Bar's position.
        /// </summary>
        public long CurrentScrollPosition
        {
            get { return _currentScrollPosition; }
            set
            {
                _currentScrollPosition = value; 
                OnPropertyChanged(nameof(CurrentScrollPosition));
            }
        }

        private bool _seamlessScroll;
        public bool SeamlessScroll
        {
            get { return _seamlessScroll; }
            set
            {
                _seamlessScroll = value;
                OnPropertyChanged(nameof(SeamlessScroll));
                OnPropertyChanged(nameof(CalculatedScrollableMaximum));

                if (!_seamlessScroll)
                {
                    CurrentScrollPosition = TranslateRelativeBytePosition(CurrentScrollPosition * 10) / 10;
                }
                else
                {
                    CurrentScrollPosition = CreateRelativeByteReference(CurrentScrollPosition * 10, CurrentLogFile) / 10;
                }
            }
        }

        #endregion
        
        #region Constructor

        public LogViewModel()
        {
            LogLines = new ObservableCollection<LogLine>();
            _logFiles = new List<LogFileInfo>();
            _searchResults = new List<SearchResult>();
            _seamlessScroll = true;

            FileControlVisibility = Visibility.Collapsed;
            SetupAkkaActorSystem();
        }

        #endregion

        #region Methods

        #region Akka.Net Methods

        /// <summary>
        /// Sets up the Akka.Net Actor System
        /// </summary>
        private void SetupAkkaActorSystem()
        {
            var config = ConfigurationFactory.ParseString(@"akka.actor.default-dispatcher.shutdown { timeout = 0 }");
            _akkaActorSystem = ActorSystem.Create("MyActorSystem", config);

            Props updateUIActorProps = Props.Create(() => new UpdateUIActor()).WithDispatcher("akka.actor.synchronized-dispatcher");
            _updateUIActor = _akkaActorSystem.ActorOf(updateUIActorProps, $"updateUI_{Guid.NewGuid()}");

            Props updateDataSourceActorProps = Props.Create(() => new UpdateDataSourceActor(this)).WithDispatcher("akka.actor.synchronized-dispatcher");
            _updateDataSourceActor = _akkaActorSystem.ActorOf(updateDataSourceActorProps, $"updateDataSource_{Guid.NewGuid()}");

            Props readLineFromFileActorProps = Props.Create(() => new ReadLineFromFileActor(_updateDataSourceActor));
            _readLineFromFileActor = _akkaActorSystem.ActorOf(readLineFromFileActorProps, $"readLineFromFile_{Guid.NewGuid()}");

            Props findStartingByteLocationActorProps = Props.Create(() => new FindByteLocationActor(_readLineFromFileActor));
            _findStartingByteActor = _akkaActorSystem.ActorOf(findStartingByteLocationActorProps, $"findStartingByteLocation_{Guid.NewGuid()}");
        }

        public void ReadLinesStartingFromBytePosition(long startingByte, int numberOfInstancesToFind, bool overrideUI = false)
        {
            InitiateNewRead();

            long byteToReadFrom = SeamlessScroll ? TranslateRelativeBytePosition(startingByte) : startingByte;
            string fileToReadFrom = SeamlessScroll ? LocateLogFileFromByteReference(startingByte) : CurrentLogFile;

            TriggerNewLogFileRead(byteToReadFrom, ProgramConfig.LineFeedByte, FindByteLocationActorMessages.SearchDirection.Backward, fileToReadFrom, numberOfInstancesToFind, overrideUI, null, null);
        }

        /// <summary>
        /// Begins a new Log File Read from a specific occurrence of a byte within the file.
        /// </summary>
        /// <param name="byteToFind">The <see cref="byte"/> whose occurrence triggers the read.</param>
        /// <param name="occurrenceToReadFrom">The numbered occurrence of the <see cref="byte"/> to begin reading from.</param>
        /// <param name="file">The log file we want to read from.</param>
        /// <param name="updateAction">The <see cref="Action"/> to perform as we progress towards the read.</param>
        public void BeginNewReadAtByteOccurrenceNumber(long occurrenceToReadFrom, string file, Action<double> updateAction)
        {
            TriggerNewLogFileRead(0, ProgramConfig.LineFeedByte, FindByteLocationActorMessages.SearchDirection.Forward, file, occurrenceToReadFrom, true, _updateUIActor, updateAction);
        }

        /// <summary>
        /// Begins a new Log File read from a specific file at a specific byte location.
        /// </summary>
        /// <param name="startingByte">The byte location within the log file to begin reading from.</param>
        /// <param name="filePath">The file path of the log file to read from.</param>
        public void BeginNewReadInSpecificFileAtByteLocation(long startingByte, string filePath)
        {
            TriggerNewLogFileRead(startingByte, ProgramConfig.LineFeedByte, FindByteLocationActorMessages.SearchDirection.Backward, filePath, 1, true, null, null);
        }

        private void TriggerNewLogFileRead(long startingByte, byte byteToFind, FindByteLocationActorMessages.SearchDirection searchDirection, string filePath, long numberOfInstancesToFind, bool updateUi, IActorRef updateActorRef, Action<double> updateAction)
        {
            InitiateNewRead();
            _findStartingByteActor.Tell(
                new FindByteLocationActorMessages.FindNumberedInstanceOfByteLocationInFileAndUpdateOnProgress(
                    startingByte,
                    byteToFind,
                    searchDirection,
                    filePath,
                    numberOfInstancesToFind,
                    updateUi,
                    updateActorRef,
                    updateAction));
        }

        public void ReadLine(long startingByte, bool overrideUi = false)
        {
            InitiateNewRead();
            _readLineFromFileActor.Tell(new ReadLineFromFileActorMessages.ReadLineFromFileStartingAtByte(
                LocateLogFileFromByteReference(startingByte), TranslateRelativeBytePosition(startingByte),
                overrideUi));
        }

        public void ContinueReadFromByteLocation(long startingByte,
            FindByteLocationActorMessages.SearchDirection searchDirection, int numberOfInstancesToFind, bool overrideUI = false)
        {
            _findStartingByteActor.Tell(
                new FindByteLocationActorMessages.FindNumberedInstanceOfByteLocationInFile(
                    TranslateRelativeBytePosition(startingByte),
                    ProgramConfig.LineFeedByte,
                    searchDirection,
                    LocateLogFileFromByteReference(startingByte),
                    numberOfInstancesToFind,
                    overrideUI));
        }

        #endregion
        
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

            UpdateLogLinesWithNewLine(lineToAdd);

            _currentLineToUpdate++;
            OnPropertyChanged(nameof(_currentLineToUpdate));
            OnPropertyChanged(nameof(IsReading));

            if (RawDisplayMode)
            {
                if (_currentLineToUpdate >= _logLines.Count)
                {
                    OnPropertyChanged(nameof(LogLineBlock));
                }
            }
        }

        private void UpdateLogLinesWithNewLine(ReadLineFromFileActor.ReturnedLine lineToAdd)
        {
            if (!ExpandingView)
            {
                int zeroBasedLogLineIndex = Math.Max(0, _currentLineToUpdate - 1);

                if (zeroBasedLogLineIndex < _logLines.Count)
                {
                    if (!SeamlessScroll)
                    {
                        if (lineToAdd.FilePath == CurrentLogFile)
                        {
                            _logLines[zeroBasedLogLineIndex] = CreateNewLogLine(lineToAdd);
                        }
                        else
                        {
                            ReadLineFromFileActor.ReturnedLine lastLine = new ReadLineFromFileActor.ReturnedLine
                            {
                                LineStartsAtByteLocation = _logLines[zeroBasedLogLineIndex - 1].StartingByte,
                                LineEndsAtByteLocation = _logLines[zeroBasedLogLineIndex - 1].EndingByte,
                                FilePath = CurrentLogFile,
                                Line = ""
                            };

                            _logLines[zeroBasedLogLineIndex] = CreateNewLogLine(lastLine);
                        }
                    }
                    else
                    {
                        _logLines[zeroBasedLogLineIndex] = CreateNewLogLine(lineToAdd);
                    }
                }
            }
            else
            {
                if (!SeamlessScroll)
                {
                    if (lineToAdd.FilePath == CurrentLogFile)
                    {
                        _logLines.Add(CreateNewLogLine(lineToAdd));
                    }
                }
                else
                {
                    _logLines.Add(CreateNewLogLine(lineToAdd));
                }
            }
        }

        /// <summary>
        /// Creates a new <see cref="LogLine"/> based on the content of a <see cref="ReadLineFromFileActor.ReturnedLine"/>.
        /// </summary>
        /// <param name="line">The <see cref="ReadLineFromFileActor.ReturnedLine"/> to construct the <see cref="LogLine"/>.</param>
        private LogLine CreateNewLogLine(ReadLineFromFileActor.ReturnedLine line)
        {
            CurrentLogFile = line.FilePath;
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

        /// <summary>
        /// Creates a list of <see cref="LogFileInfo"/>s based on the provided list of <see cref="Tuple"/>s.
        /// </summary>
        /// <param name="logFileLocations">The list of <see cref="Tuple{T, T1}"/> to create the <see cref="LogFileInfo"/>.</param>
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

        /// <summary>
        /// Takes an absolute byte reference, and uses it to determine which currently loaded Log File is is contained within.
        /// </summary>
        /// <param name="byteLocation">The absolute byte reference.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Triggers the system to prepare for a new data read.
        /// </summary>
        private void InitiateNewRead()
        {
            _currentLineToUpdate = 1;
            OnPropertyChanged(nameof(_currentLineToUpdate));
            OnPropertyChanged(nameof(IsReading));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Caps the UI to display the current number of log lines as its maximum.
        /// </summary>
        public void CapScrollWindow()
        {
            ExpandingView = false;
            if (_logLines.Count <= 0)
            {
                return;
            }

            _logLines.RemoveAt(_logLines.Count - 1);
        }

        /// <summary>
        /// Clamps a byte reference to within acceptable bounds.
        /// </summary>
        /// <param name="byteLocation">The byte reference to clamp.</param>
        /// <returns></returns>
        private long ClampByteLocation(long byteLocation)
        {
            long totalBytes = _logFiles.Sum(a => a.FileSizeInByte) - 1;
            long clampedByteLocation = Math.Max(0, Math.Min(totalBytes, byteLocation));
            return clampedByteLocation;
        }

        /// <summary>
        /// Sets the total number of lines for a specific Log File.
        /// </summary>
        /// <param name="logFile">The log file for whom the line count is being set.</param>
        /// <param name="lineCount">The number of numbers contained within the log file.</param>
        public void SetLogFileLineCount(string logFile, long lineCount)
        {
            _logFiles.FirstOrDefault(l => l.FileLocation == logFile)?.SetLineCount(lineCount);
            OnPropertyChanged(nameof(LinesInCurrentLogFile));
            OnPropertyChanged(nameof(TotalNumberOfLinesInAllLogFiles));
        }

        public void LoadLogFilesIntoLogViewModel(Log4NetConfig log4NetConfigToLoad)
        {
            _log4NetConfig = log4NetConfigToLoad;
            LoadLogFiles();

            FileControlVisibility = Visibility.Visible;
        }

        private void LoadLogFiles()
        {
            Appender rollingFileAppender =
                _log4NetConfig.AppendersOfType(AppenderType.RollingFileAppender).FirstOrDefault();

            if (null == rollingFileAppender)
            {
                throw new ArgumentNullException($"No Rolling File Appender found.");
            }

            DirectoryInfo logDirectory = new DirectoryInfo(rollingFileAppender.FolderPath);
            FileInfo[] filesInDirectory = logDirectory.GetFiles().OrderByDescending(f => f.LastWriteTime).ToArray();

            List<FileInfo> log4NetConfigFiles = new List<FileInfo>();
            long totalFileSizes = 0;

            List<Tuple<string, long>> logFileLocations = new List<Tuple<string, long>>();
            foreach (var logFile in filesInDirectory.Where(l => l.FullName.Contains(rollingFileAppender.StaticFileNameMask)))
            {
                string filePath = logFile.FullName;
                long fileSizeInBytes = logFile.Length;

                try
                {
                    totalFileSizes += fileSizeInBytes;
                }
                catch
                {
                    // TODO: Implement handling for when total size of files exceeds content maximum of the Long
                    break;
                }

                logFileLocations.Add(new Tuple<string, long>(filePath, fileSizeInBytes));
            }

            CreateLogFiles(logFileLocations);

            ReadLinesStartingFromBytePosition(0, 1);

            Props findStartingByteLocationActorProps = Props.Create(() => new FindByteLocationActor(null));
            IActorRef findStartingByteActor = _akkaActorSystem.ActorOf(findStartingByteLocationActorProps, $"findStartingByteLocation_{Guid.NewGuid()}");

            foreach (LogFileInfo logFileInfo in LogFiles)
            {
                findStartingByteActor.Tell(new FindByteLocationActorMessages.CountByteOccurrencesInFile(ProgramConfig.LineFeedByte, logFileInfo.FileLocation, _updateUIActor, SetLogFileLineCount));
            }

            findStartingByteActor.Tell(PoisonPill.Instance);
        }

        public void CreateNewSearchResult(string filePath, long byteStartLocation, byte[] itemFound)
        {
            _searchResults.Add(new SearchResult(byteStartLocation, filePath, itemFound));

            _searchResultView = CollectionViewSource.GetDefaultView(_searchResults.OrderByDescending(r => r.LogFilePath).ThenBy(r => r.ByteStartingPosition));

            OnPropertyChanged(nameof(NumberOfSearchResults));
            OnPropertyChanged(nameof(SearchResultsView));
        }

        public void BeginSearch(string stringToFind, bool currentFileOnly)
        {
            byte[] bytesToFind = Encoding.ASCII.GetBytes(stringToFind);
            _searchResults.Clear();

            if (currentFileOnly)
            {
                TriggerFileSearch(bytesToFind, CurrentLogFile, true);
                return;
            }

            foreach (LogFileInfo logFileInfo in LogFiles)
            {
                TriggerFileSearch(bytesToFind, logFileInfo.FileLocation, true);
            }
        }

        private void TriggerFileSearch(byte[] bytesToFind, string filePath, bool stopActorAfterSearch)
        {
            Props updateUIActorProps = Props.Create(() => new UpdateUIActor()).WithDispatcher("akka.actor.synchronized-dispatcher");
            IActorRef updateUIActor = _akkaActorSystem.ActorOf(updateUIActorProps, $"updateUI_{Guid.NewGuid()}");

            Props findStartingByteLocationActorProps = Props.Create(() => new FindByteLocationActor(null));
            IActorRef findStartingByteActor = _akkaActorSystem.ActorOf(findStartingByteLocationActorProps, $"findStartingByteLocation_{Guid.NewGuid()}");

            findStartingByteActor.Tell(new FindByteLocationActorMessages.FindByteOccurrencesWithinAFile(bytesToFind, filePath, updateUIActor, CreateNewSearchResult, stopActorAfterSearch));
            findStartingByteActor.Tell(PoisonPill.Instance);
        }

        #endregion
    }
}
