using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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

        private long _currentFileSizeInBytes = 0;
        private string _currentFile;
        private string _currentLine;
        private int _lastVisibleLineIndex = -1;
        private int _currentByteChunk = 0;

        private double _textBoxHeight;

        public MainWindow()
        {
            InitializeComponent();
        }
        
        /// <summary>
        /// Opens a text file dialog and returns the selected file location.
        /// </summary>
        private string SelectTextFileToOpen()
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

                return selectedFileName;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Error occured opening file: {e.Message}", "File Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            return string.Empty;
        }

        private void LoadChunkIntoTextBox(bool refreshDisplay = false)
        {
            int currentChunkToRead = _currentByteChunk;
            byte[] buffer = new byte[ChunkSize];

            using (FileStream fileStream = new FileStream(_currentFile, FileMode.Open, FileAccess.Read))
            {
                int undisplayedBytes = 0;

                while (undisplayedBytes == 0)
                {
                    fileStream.Seek(Convert.ToInt32(currentChunkToRead), SeekOrigin.Begin);
                    fileStream.Read(buffer, 0, ChunkSize);
                    undisplayedBytes = ReadByteChunkIntoTextBox(buffer);
                    currentChunkToRead += ChunkSize;
                }

                _currentByteChunk = currentChunkToRead + undisplayedBytes;
            }
        }

        /// <summary>
        /// Reads the current byte array into the Text Box, and returns a bool representing if more lines can be displayed.
        /// </summary>
        /// <param name="bytesToRead">The array of bytes to be read into the Text Box.</param>
        private int ReadByteChunkIntoTextBox(byte[] bytesToRead)
        {
            foreach (byte b in bytesToRead)
            {
                if (b != 13)
                {
                    char currentByte = Convert.ToChar(b);
                    _currentLine += currentByte;
                    continue;
                }
                
                int currentIndex = Array.IndexOf(bytesToRead, b);
                if (currentIndex + 1 >= bytesToRead.Length)
                {
                    continue;
                }

                if (bytesToRead[currentIndex + 1] != 10)
                {
                    continue;
                }

                AddCurrentLineToTextBox();
                if (LineTextBox.GetLastVisibleLineIndex() == _lastVisibleLineIndex)
                {
                    LineTextBox.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                    return bytesToRead.Length - currentIndex;
                }

                _lastVisibleLineIndex = LineTextBox.GetLastVisibleLineIndex();
            }

            return 0;
        }

        private void AddCurrentLineToTextBox()
        {
            LineTextBox.Text += _currentLine;
            _currentLine = string.Empty;
        }

        private void ManualScrollBar_OnScroll(object sender, ScrollEventArgs e)
        {
            if (_currentFileSizeInBytes > 0)
            {
                LoadChunkIntoTextBox();
            }
            else
            {
                // TextBox.Text = ManualScrollBar.Value.ToString();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _currentFile = SelectTextFileToOpen();
            _currentFileSizeInBytes = new FileInfo(_currentFile).Length;

            LineTextBox.Text = string.Empty;
            _lastVisibleLineIndex = -1;

            LoadChunkIntoTextBox();
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
    }
}
