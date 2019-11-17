using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Akka.Actor;
using Akka.Configuration;
using Akka.Util.Internal;
using Log4Net.Extensions.Configuration.Implementation;
using Log4Net.Extensions.Configuration.Implementation.ConfigObjects;
using LogReader.Akka.Net.Actors;
using LogReader.Configuration;
using LogReader.Log4Net;
using LogReader.Structure;
using Microsoft.Win32;

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

            Props updateDataSourceActorProps = Props.Create(() => new UpdateDataSourceActor(this)).WithDispatcher("akka.actor.synchronized-dispatcher");
            // Props updateDataSourceActorProps = Props.Create(() => new UpdateDataSourceActor(this));
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
                catch (Exception e)
                {
                    break;
                }

                logFileLocations.Add(new Tuple<string, long>(filePath, fileSizeInBytes));
            }

            LogViewModel.SetLogFileLocations(logFileLocations);
        }

        private void ReadLine(long startingByte, bool overrideUi = false)
        {
            LogViewModel.InitiateNewRead();
            _readLineFromFileActor.Tell(new ReadLineFromFileActorMessages.ReadLineFromFileStartingAtByte(LogViewModel.LocateLogFileFromByteReference(startingByte), startingByte, overrideUi));
        }

        private void BeginNewReadAtByteLocation(long startingByte,
            FindByteLocationActorMessages.SearchDirection searchDirection, int numberOfInstancesToFind, bool overrideUI = false)
        {
            LogViewModel.InitiateNewRead();
            _findStartingByteActor.Tell(
                new FindByteLocationActorMessages.FindByteLocationInFile(
                    startingByte,
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

            ManualScrollBar.Maximum = LogViewModel.TotalFileSizesInBytes / 10;
            ManualScrollBar.Minimum = 0;
            ManualScrollBar.IsEnabled = true;

            ManualScrollBar.Value = ManualScrollBar.Minimum;

            ReadLine(0);
        }

        private void ManualScrollBar_OnScroll(object sender, ScrollEventArgs e)
        {
            var test = Lines;
            if (e.ScrollEventType == ScrollEventType.EndScroll)
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
                long startingByte = LogViewModel.OnScreenLines.StartingByte;

                if (e.ScrollEventType != ScrollEventType.SmallDecrement)
                {
                    startingByte =
                        Math.Max(0, Math.Min(LogViewModel.TotalFileSizesInBytes, (long)(e.NewValue * 10)));

                    FileLocationWindowLabel.Content = LogViewModel.LocateLogFileFromByteReference(startingByte);
                }
                else
                {
                    overrideUI = true;
                }

                BeginNewReadAtByteLocation(startingByte, FindByteLocationActorMessages.SearchDirection.Backward, numberOfInstancesToFind, overrideUI);
            }
        }

        private void ManualScrollBar_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ScrollValueLabel.Content = e.NewValue;
        }

        #endregion

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = LogViewModel = new LogViewModel();
        }
    }
}
