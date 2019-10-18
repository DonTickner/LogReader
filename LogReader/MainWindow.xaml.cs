using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Akka.Actor;
using Akka.Configuration;
using Akka.Util.Internal;
using Microsoft.Win32;

namespace LogReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// The number of bytes to load in a single chunk of a file.
        /// </summary>
        private const int ChunkSize = 1024;

        private long _currentFileSizeInBytes;
        private string _currentFile;
        private string _currentLine;
        private int _currentByteChunk;
        private double _textBoxHeight;
        private int _linesInFile;

        #region Akka.Net

        private ActorSystem _akkaActorSystem;

        #endregion

        public MainWindow()
        {
            InitializeComponent();

            InitialSetup();
        }
        
        private void ManualScrollBar_OnScroll(object sender, ScrollEventArgs e)
        {
            if (_currentFileSizeInBytes > 0)
            {
                LoadChunkIntoTextBox();
            }
        }

        private void LoadFileButton_Click(object sender, RoutedEventArgs e)
        {
            LineTextBox.Text = string.Empty;

            SelectTextFileToOpen();
            LoadChunkIntoTextBox();
            CountLinesInFile();

            manualScrollBar.Maximum = _linesInFile;
            manualScrollBar.IsEnabled = true;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentFile))
            {
                return;
            }

            _linesInFile = 0;
            CountLinesInFile();

            MessageBox.Show($"There are {_linesInFile} in the current file {_currentFile}", "Lines in File",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (LineTextBox.ActualHeight > _textBoxHeight)
            {
                _textBoxHeight = LineTextBox.ActualHeight;
                if (!string.IsNullOrEmpty(_currentFile))
                {
                    LoadChunkIntoTextBox();
                }
            }
        }

        #region Methods

        #region Setup

        /// <summary>
        /// Sets up all initial work necessary for the LogReader
        /// </summary>
        private void InitialSetup()
        {
            SetupAkkaActorSystem();

            manualScrollBar.Maximum = manualScrollBar.Minimum = 0;
            manualScrollBar.IsEnabled = false;
        }

        /// <summary>
        /// Sets up the Akka.Net Actor System
        /// </summary>
        private void SetupAkkaActorSystem()
        {
            var config = ConfigurationFactory.ParseString(@"akka.actor.default-dispatcher.shutdown { timeout = 0 }");
            _akkaActorSystem = ActorSystem.Create("MyActorSystem", config);
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
            SetCurrentFileSizeInBytes(_currentFile);
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
        /// Loads a memory chunk into the box
        /// </summary>
        private void LoadChunkIntoTextBox()
        {
            int currentChunkToRead = _currentByteChunk;
            byte[] buffer = new byte[ChunkSize];

            using (FileStream fileStream = new FileStream(_currentFile, FileMode.Open, FileAccess.Read))
            {
                int currentLine = LineTextBox.LineCount;

                do
                {
                    fileStream.Seek(Convert.ToInt32(currentChunkToRead), SeekOrigin.Begin);
                    fileStream.Read(buffer, 0, ChunkSize);
                    ReadByteChunkIntoTextBox(buffer);
                    currentChunkToRead += ChunkSize;
                    currentLine++;
                } while (currentLine < LineTextBox.GetLastVisibleLineIndex());

                _currentByteChunk = currentChunkToRead;
            }
        }

        /// <summary>
        /// Reads the current byte array into the Text Box, and returns a bool representing if more lines can be displayed.
        /// </summary>
        /// <param name="bytesToRead">The array of bytes to be read into the Text Box.</param>
        private void ReadByteChunkIntoTextBox(byte[] bytesToRead)
        {
            for (int i = 0; i < bytesToRead.Length; i++)
            {
                byte b = bytesToRead[i];

                if (b != 10)
                {
                    char currentByte = Convert.ToChar(b);
                    _currentLine += currentByte;
                    continue;
                }

                AddCurrentLineToTextBox();
            }
        }

        /// <summary>
        /// Adds the content of the currentLine global variable into the text box
        /// </summary>
        private void AddCurrentLineToTextBox()
        {
            LineTextBox.Text += _currentLine;
            _currentLine = string.Empty;
        }

        /// <summary>
        /// Counts number of physical lines within the current file
        /// </summary>
        private void CountLinesInFile()
        {
            byte[] buffer = new byte[ChunkSize];
            int bytesRead = 0;

            using (FileStream fileStream = new FileStream(_currentFile, FileMode.Open, FileAccess.Read))
            {
                while (bytesRead < _currentFileSizeInBytes)
                {
                    fileStream.Seek(Convert.ToInt32(bytesRead), SeekOrigin.Begin);
                    fileStream.Read(buffer, 0, ChunkSize);
                    CountLinesInChunk(buffer);
                    bytesRead += ChunkSize;
                }
            }

            manualScrollBar.Track.Minimum = 0;
            manualScrollBar.Track.Maximum = _linesInFile;
        }

        /// <summary>
        /// Counts the number of lines in the passed byte array
        /// </summary>
        /// <param name="bytesToRead">The byte array that represent a memory chunk</param>
        private void CountLinesInChunk(byte[] bytesToRead)
        {
            for (int i = 0; i < bytesToRead.Length; i++)
            {
                _linesInFile += bytesToRead[i] == 10 ? 1 : 0;
            }
        }

        #endregion

        #endregion
    }
}
