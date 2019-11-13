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
using LogReader.Akka.Net.Actors;
using LogReader.Configuration;
using LogReader.Structure;
using Microsoft.Win32;

namespace LogReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private long _currentFileSizeInBytes;
        private string _currentFile;
        private int _linesInFile;
        
        #region Akka.Net

        private ActorSystem _akkaActorSystem;

        private IActorRef _findStartingByteActor;
        private IActorRef _readLineFromFileActor;
        private IActorRef _updateDataSourceActor;

        #endregion

        #region Data Objects

        public LogViewModel LogViewModel;

        public ByteWindow onScreenLines = new ByteWindow();
        public ByteWindow firstOnScreenLine = new ByteWindow();
        public bool reading = false;

        #endregion

        public MainWindow()
        {
            InitializeComponent();

            InitialSetup();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentFile))
            {
                return;
            }

            _linesInFile = 1;
            CountLinesInFile();

            MessageBox.Show($"There are {_linesInFile} in the current file {_currentFile}", "Lines in File",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #region Methods

        #region Setup

        /// <summary>
        /// Sets up all initial work necessary for the LogReader
        /// </summary>
        private void InitialSetup()
        {
            LogViewModel = new LogViewModel();
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
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Open Text File",
                DefaultExt = "txt",
                Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*",
                CheckFileExists = true,
                CheckPathExists = true
            };

            openFileDialog.ShowDialog();
            string selectedFileName = openFileDialog.FileName;

            try
            {
                if (!File.Exists(selectedFileName))
                {
                    throw new FileLoadException($"File '{selectedFileName}' does not exist.");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"Error occured opening file: {e.Message}", "File Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            _currentFile = selectedFileName;
            _linesInFile = 1;
        }

        /// <summary>
        /// Sets the current file's file size in bytes
        /// </summary>
        /// <param name="filePath">The physical path to the file.</param>
        private void SetCurrentFileSizeInBytes(string filePath)
        {
            _currentFileSizeInBytes = new FileInfo(_currentFile).Length;
        }
        
        /// <summary>
        /// Counts number of physical lines within the current file
        /// </summary>
        private void CountLinesInFile()
        {
            byte[] buffer = new byte[ProgramConfig.ChunkSize];
            int bytesRead = 0;

            using (FileStream fileStream = new FileStream(_currentFile, FileMode.Open, FileAccess.Read))
            {
                while (bytesRead < _currentFileSizeInBytes)
                {
                    fileStream.Seek(Convert.ToInt32(bytesRead), SeekOrigin.Begin);
                    fileStream.Read(buffer, 0, ProgramConfig.ChunkSize);
                    CountLinesInChunk(buffer);
                    bytesRead += ProgramConfig.ChunkSize;
                }
            }
        }

        /// <summary>
        /// Counts the number of lines in the passed byte array
        /// </summary>
        /// <param name="bytesToRead">The byte array that represent a memory chunk</param>
        private void CountLinesInChunk(byte[] bytesToRead)
        {
            for (int i = 0; i < bytesToRead.Length; i++)
            {
                _linesInFile += bytesToRead[i] == ProgramConfig.LineFeedByte ? 1 : 0;
            }
        }

        #endregion

        #endregion

        #region Events

        private void LoadFileButton_Click(object sender, RoutedEventArgs e)
        {
            SelectTextFileToOpen();
            SetCurrentFileSizeInBytes(_currentFile);

            LineTextBox.Text = string.Empty;

            ManualScrollBar.Maximum = _currentFileSizeInBytes / 10;
            ManualScrollBar.Minimum = 0;
            ManualScrollBar.IsEnabled = true;

            onScreenLines.StartingByte = 0;
            firstOnScreenLine.StartingByte = _currentFileSizeInBytes;

            ManualScrollBar.Value = ManualScrollBar.Minimum;

            _readLineFromFileActor.Tell(new ReadLineFromFileActorMessages.ReadLineFromFileStartingAtByte(_currentFile, 0, false));
        }

        private void LineTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (reading 
                && LineTextBox.LineCount < 20)
            {
                _readLineFromFileActor.Tell(new ReadLineFromFileActorMessages.ReadLineFromFileStartingAtByte(_currentFile, onScreenLines.EndingByte, false));
                ByteWindowLabel.Content = $"{onScreenLines.StartingByte} - {onScreenLines.EndingByte} / {_currentFileSizeInBytes}";
            }
            else
            {
                reading = false;
            }
        }

        private void ManualScrollBar_OnScroll(object sender, ScrollEventArgs e)
        {
            if (reading ||
                e.ScrollEventType == ScrollEventType.EndScroll)
            {
                e.Handled = true;
                return;
            }

            LineTextBox.Text = string.Empty;
            firstOnScreenLine.StartingByte = -1;
            reading = true;

            if (e.ScrollEventType == ScrollEventType.SmallIncrement)
            {
                _readLineFromFileActor.Tell(new ReadLineFromFileActorMessages.ReadLineFromFileStartingAtByte(_currentFile, firstOnScreenLine.EndingByte, true));
            }
            else
            {
                int numberOfInstancesToFind = e.ScrollEventType == ScrollEventType.SmallDecrement ? 2 : 1;
                bool overrideUI = false;

                if (e.ScrollEventType != ScrollEventType.SmallDecrement)
                {
                    onScreenLines.StartingByte =
                        Math.Max(0, Math.Min(_currentFileSizeInBytes, (long)(e.NewValue * 10)));
                }
                else
                {
                    overrideUI = true;
                }

                _findStartingByteActor.Tell(new FindByteLocationActorMessages.FindByteLocationInFile(onScreenLines.StartingByte,
                    ProgramConfig.LineFeedByte,
                    FindByteLocationActorMessages.SearchDirection.Backward,
                    _currentFile,
                    numberOfInstancesToFind,
                    overrideUI));
            }
        }

        private void ManualScrollBar_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ScrollValueLabel.Content = e.NewValue;
        }

        #endregion
    }
}
