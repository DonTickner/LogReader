using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Akka.Util;
using LogReader.Akka.Net.Actors;
using LogReader.Structure;

namespace LogReader.Forms
{
    /// <summary>
    /// Interaction logic for SearchDialog.xaml
    /// </summary>
    public partial class SearchDialog : Window
    {
        private LogViewModel _logViewModel;

        public SearchDialog(LogViewModel logViewModel)
        {
            _logViewModel = logViewModel;
            DataContext = _logViewModel;
            InitializeComponent();
            InitialiseDialog();
        }

        private void InitialiseDialog()
        {
            CurrentFileRadioButton.IsChecked = true;
            AllFilesRadioButton.IsChecked = false;
        }

        private void SearchProgressButton_OnClick(object sender, RoutedEventArgs e)
        {
            ResultsGrid.Visibility = Visibility.Visible;

            bool currentFileOnly = CurrentFileRadioButton.IsChecked.HasValue && CurrentFileRadioButton.IsChecked.Value;

            _logViewModel.BeginSearch(FindTextTextBox.Text, currentFileOnly);
        }

        private void ResultsListView_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ResultsListView.SelectedItem is SearchResult result)
            {
                _logViewModel.BeginNewReadInSpecificFileAtByteLocation(result.ByteStartingPosition, result.LogFilePath);
            }
        }
    }
}
