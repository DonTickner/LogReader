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
            ResultsListView.Items.Clear();
            ResultsGrid.Visibility = Visibility.Visible;
        }
    }
}
