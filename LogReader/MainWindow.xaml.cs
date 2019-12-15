using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Akka.Actor;
using Akka.Configuration;
using Log4Net.Extensions.Configuration.Implementation;
using Log4Net.Extensions.Configuration.Implementation.ConfigObjects;
using LogReader.Akka.Net.Actors;
using LogReader.Configuration;
using LogReader.Log4Net;
using LogReader.Structure;

namespace LogReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Akka.Net

        private ActorSystem _akkaActorSystem;

        private IActorRef _findStartingByteActor;
        private IActorRef _readLineFromFileActor;
        private IActorRef _updateDataSourceActor;
        private IActorRef _updateUIActor;

        #endregion

        #region Data Objects

        private Log4NetConfig _log4NetConfig;
        public LogViewModel LogViewModel;

        #endregion
        
        public MainWindow()
        {
            InitializeComponent();

            InitialSetup();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"There are {LogViewModel.TotalLinesInFiles} in {LogViewModel.TotalNumberOfLogFiles} files.", "Lines in File",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #region Methods

        #region Setup

        /// <summary>
        /// Sets up all initial work necessary for the LogReader
        /// </summary>
        private void InitialSetup()
        {
            ManualScrollBar.IsEnabled = false;
            
            SetupDataBinding();
            SetupAkkaActorSystem();
        }

        /// <summary>
        /// Sets up all necessary work for the Data Binding
        /// </summary>
        private void SetupDataBinding()
        {
            DataContext = LogViewModel;
        }

        /// <summary>
        /// Sets up the Akka.Net Actor System
        /// </summary>
        private void SetupAkkaActorSystem()
        {
            var config = ConfigurationFactory.ParseString(@"akka.actor.default-dispatcher.shutdown { timeout = 0 }");
            _akkaActorSystem = ActorSystem.Create("MyActorSystem", config);

            Props updateUIActorProps = Props.Create(() => new UpdateUIActor(gotoProgressButton.PerformStep)).WithDispatcher("akka.actor.synchronized-dispatcher");
            _updateUIActor = _akkaActorSystem.ActorOf(updateUIActorProps, $"updateUI_{Guid.NewGuid()}");

            Props updateDataSourceActorProps = Props.Create(() => new UpdateDataSourceActor(this)).WithDispatcher("akka.actor.synchronized-dispatcher");
            _updateDataSourceActor = _akkaActorSystem.ActorOf(updateDataSourceActorProps, $"updateDataSource_{Guid.NewGuid()}");

            Props readLineFromFileActorProps = Props.Create(() => new ReadLineFromFileActor(_updateDataSourceActor));
            _readLineFromFileActor = _akkaActorSystem.ActorOf(readLineFromFileActorProps, $"readLineFromFile_{Guid.NewGuid()}");

            Props findStartingByteLocationActorProps = Props.Create(() => new FindByteLocationActor(_readLineFromFileActor));
            _findStartingByteActor = _akkaActorSystem.ActorOf(findStartingByteLocationActorProps, $"findStartingByteLocation_{Guid.NewGuid()}");
        }

        #endregion

        #region File Operations

        /// <summary>
        /// Opens a text file dialog and returns the selected file location.
        /// </summary>
        private void SelectTextFileToOpen()
        {
            _log4NetConfig = null;

            Log4NetOpenFileDialog log4NetOpenFileDialog = new Log4NetOpenFileDialog();
            log4NetOpenFileDialog.ShowDialog();
            if (log4NetOpenFileDialog.Log4NetConfigFileSuccessfullyLoaded)
            {
                _log4NetConfig = log4NetOpenFileDialog.Log4NetConfig;
            }
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

            LogViewModel.SetLogFileLocations(logFileLocations);
        }

        private void ReadLine(long startingByte, bool overrideUi = false)
        {
            LogViewModel.InitiateNewRead();
            _readLineFromFileActor.Tell(new ReadLineFromFileActorMessages.ReadLineFromFileStartingAtByte(
                LogViewModel.LocateLogFileFromByteReference(startingByte), LogViewModel.TranslateRelativeBytePosition(startingByte),
                overrideUi));
        }

        private void BeginNewReadAtByteLocation(long startingByte,
            FindByteLocationActorMessages.SearchDirection searchDirection, int numberOfInstancesToFind, bool overrideUI = false)
        {
            LogViewModel.InitiateNewRead();
            _findStartingByteActor.Tell(
                new FindByteLocationActorMessages.FindNumeredInstanceOfByteLocationInFile(
                    LogViewModel.TranslateRelativeBytePosition(startingByte),
            ProgramConfig.LineFeedByte,
                    searchDirection,
                LogViewModel.LocateLogFileFromByteReference(startingByte),
                numberOfInstancesToFind,
                overrideUI));
        }

        private void BeginReadAtNewSpecificByteIndexLocation(FindByteLocationActorMessages.SearchDirection searchDirection, long numberOfInstancesToFind, string file, bool overrideUI = false)
        {
            BeginReadAtNewSpecificByteIndexLocationAndUpdate(searchDirection, numberOfInstancesToFind, file, null, overrideUI);
        }

        private void BeginReadAtNewSpecificByteIndexLocationAndUpdate(FindByteLocationActorMessages.SearchDirection searchDirection, long numberOfInstancesToFind, string file, IActorRef updateUIActor, bool overrideUI)
        {
            LogViewModel.InitiateNewRead();
            _findStartingByteActor.Tell(
                new FindByteLocationActorMessages.FindNumeredInstanceOfByteLocationInFileAndUpdateOnProgress(
                0,
                ProgramConfig.LineFeedByte,
                searchDirection,
                file,
                numberOfInstancesToFind,
                overrideUI,
                updateUIActor));
        }

        private void ContinueReadFromByteLocation(long startingByte,
            FindByteLocationActorMessages.SearchDirection searchDirection, int numberOfInstancesToFind, bool overrideUI = false)
        {
            _findStartingByteActor.Tell(
                new FindByteLocationActorMessages.FindNumeredInstanceOfByteLocationInFile(
                    LogViewModel.TranslateRelativeBytePosition(startingByte),
                    ProgramConfig.LineFeedByte,
                    searchDirection,
                    LogViewModel.LocateLogFileFromByteReference(startingByte),
                    numberOfInstancesToFind,
                    overrideUI));
        }

        #endregion

        #endregion

        #region Events

        private void LoadFileButton_Click(object sender, RoutedEventArgs e)
        {
            SelectTextFileToOpen();
            if (null == _log4NetConfig)
            {
                return;
            }

            LoadLogFiles();

            LineTextBox.Text = string.Empty;
            
            ManualScrollBar.IsEnabled = true;

            ManualScrollBar.Value = ManualScrollBar.Minimum;

            ReadLine(0);
        }

        private void ManualScrollBar_OnScroll(object sender, ScrollEventArgs e)
        {
            if (e.ScrollEventType == ScrollEventType.EndScroll
                || LogViewModel.IsReading)
            {
                e.Handled = true;
                return;
            }

            if (e.ScrollEventType == ScrollEventType.SmallIncrement)
            {
                ReadLine(LogViewModel.FirstLineEndingByte, true);
            }
            else
            {
                int numberOfInstancesToFind = e.ScrollEventType == ScrollEventType.SmallDecrement ? 2 : 1;
                bool overrideUI = false;
                long startingByte = LogViewModel.FirstLineStartingByte;

                if (e.ScrollEventType != ScrollEventType.SmallDecrement)
                {
                    startingByte =
                        Math.Max(0, Math.Min(LogViewModel.TotalFileSizesInBytes, (long)(e.NewValue * 10)));
                }
                else
                {
                    overrideUI = true;
                }                                                                                                                                                                                  

                BeginNewReadAtByteLocation(startingByte, FindByteLocationActorMessages.SearchDirection.Backward, numberOfInstancesToFind, overrideUI);
            }
        }

        #endregion

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = LogViewModel = new LogViewModel();
        }

        private T FindVisualChild<T>(DependencyObject obj) where T: DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child is T dependencyObject)
                {
                    return dependencyObject;
                }
                
                T childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                {
                    return childOfChild;
                }
            }

            return null;
        }

        private void MainWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (null == LogViewModel)
            {
                return;
            }

            if (LogViewModel.IsReading)
            {
                return;
            }

            if (e.NewSize.Height <= e.PreviousSize.Height)
            {
                return;
            }

            LogViewModel.ExpandingView = true;
            long startingByteForNewRead = LogViewModel.LastLineEndingByte;
            ContinueReadFromByteLocation(startingByteForNewRead, FindByteLocationActorMessages.SearchDirection.Backward, 1);
        }

        void DataGrid_ScrollChanged(object sender, RoutedEventArgs e)
        {
            if (null == LogViewModel)
            {
                return;
            }

            var scrollViewer = FindVisualChild<ScrollViewer>((DependencyObject)sender);

            if (null == scrollViewer)
            {
                return;
            }

            if (scrollViewer.ComputedVerticalScrollBarVisibility != Visibility.Visible)
            {
                return;
            }

            LogViewModel.CapScrollWindow();
            scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
        }

        private void GoToLineCommand_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Visibility visibility = GotoPopup.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
            GotoPopup.Visibility = visibility;

            if (visibility != Visibility.Collapsed)
            {
                return;
            }

            gotoProgressButton.ProgressBarVisibility = visibility;
            goToNumberFileComboBox.SelectedItem = null;
            goToNumberTextBox.Text = string.Empty;
        }

        private void goToNumberButton_Click(object sender, RoutedEventArgs e)
        {
            if (null == goToNumberFileComboBox.SelectionBoxItem)
            {
                return;
            }

            gotoProgressButton.IsEnabled = false;

            gotoProgressButton.ProgressBarVisibility = Visibility.Visible;
            gotoProgressButton.CurrentValue = 0;
            gotoProgressButton.MinValue = 0;
            gotoProgressButton.MaxValue = goToNumberTextBox.NumericalValue;
            gotoProgressButton.StepValue = 1;
            
            BeginReadAtNewSpecificByteIndexLocationAndUpdate(FindByteLocationActorMessages.SearchDirection.Forward, Math.Max(0, goToNumberTextBox.NumericalValue - 1), goToNumberFileComboBox.SelectedItem as string, _updateUIActor, true);
        }
    }
}
