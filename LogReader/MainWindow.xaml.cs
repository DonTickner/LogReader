using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Akka.Actor;
using Akka.Configuration;
using Log4Net.Extensions.Configuration.Implementation;
using Log4Net.Extensions.Configuration.Implementation.ConfigObjects;
using LogReader.Akka.Net.Actors;
using LogReader.Configuration;
using LogReader.Forms;
using LogReader.Log4Net;
using LogReader.Structure;
using WPF.BespokeControls.DataGrid;

namespace LogReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Data Objects

        private Log4NetConfig _log4NetConfig;
        public LogViewModel LogViewModel;

        #endregion
        
        public MainWindow()
        {
            InitializeComponent();

            InitialSetup();
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
        }

        /// <summary>
        /// Sets up all necessary work for the Data Binding
        /// </summary>
        private void SetupDataBinding()
        {
            DataContext = LogViewModel;
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

        #endregion

        #endregion

        #region Events

        private void ManualScrollBar_OnScroll(object sender, ScrollEventArgs e)
        {
            if (e.ScrollEventType == ScrollEventType.EndScroll
                || LogViewModel.IsReading)
            {
                e.Handled = true;
                return;
            }

            switch (e.ScrollEventType)
            {
                case ScrollEventType.SmallIncrement:
                {
                    LogViewModel.ReadLinesStartingFromBytePosition(LogViewModel.FirstLineEndingByte, 1, true);
                    break;
                }
                case ScrollEventType.SmallDecrement:
                {
                    LogViewModel.BeginNewReadAtByteOccuranceNumberStartingFromSpecificByteReferemce(LogViewModel.FirstLineStartingByte
                        , FindByteLocationActorMessages.SearchDirection.Backward
                        , 2,
                        true);
                    break;
                }
                default:
                {
                    long startingByte = Math.Max(0, Math.Min(LogViewModel.TotalFileSizesInBytes, (long)(e.NewValue * 10)));
                    LogViewModel.ReadLinesStartingFromBytePosition(startingByte, 1, false);
                    break;
                }
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
            e.CanExecute = LogViewModel?.HasLoaded ?? false;
        }

        private void GoToLineCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Visibility visibility = GotoPopup.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
            GotoPopup.Visibility = visibility;

            switch (visibility)
            {
                case Visibility.Visible:
                {
                    goToNumberTextBox.Focus();
                    break;
                }
                case Visibility.Collapsed:
                {
                    gotoProgressButton.ProgressBarVisibility = visibility;
                    goToNumberFileComboBox.SelectedItem = null;
                    goToNumberTextBox.Text = string.Empty;
                    break;
                }
            }
        }

        private void goToNumberButton_Click(object sender, RoutedEventArgs e)
        {
            if (null == goToNumberFileComboBox.SelectedItem)
            {
                return;
            }

            gotoProgressButton.IsEnabled = false;

            gotoProgressButton.ProgressBarVisibility = Visibility.Visible;
            gotoProgressButton.CurrentValue = 0;
            gotoProgressButton.MinValue = 0;
            gotoProgressButton.MaxValue = Math.Max(goToNumberTextBox.NumericalValue, 1);
            gotoProgressButton.StepValue = 1;
            
            LogViewModel.BeginNewReadAtByteOccurrenceNumber(Math.Max(0, goToNumberTextBox.NumericalValue - 1),
                goToNumberFileComboBox.SelectedItem as string,
                gotoProgressButton.PerformStep);
        }

        private void OpenLog4NetConfig_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void OpenLog4NetConfig_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            SelectTextFileToOpen();
            if (null == _log4NetConfig)
            {
                return;
            }

            LogViewModel.LoadLogFilesIntoLogViewModel(_log4NetConfig);

            LineTextBox.Text = string.Empty;
            ManualScrollBar.IsEnabled = true;
            ManualScrollBar.Value = ManualScrollBar.Minimum;

            Lines.UpdateColumns();
        }

        private void CurrentFileComboBox_OnSelected(object sender, RoutedEventArgs e)
        {
            if (!CurrentFileComboBox.IsDropDownOpen && !CurrentFileComboBox.IsKeyboardFocused)
            {
                return;
            }

            LogViewModel.BeginNewReadInSpecificFileAtByteLocation(0, CurrentFileComboBox.SelectedItem as string);
        }

        private void DataGrid_SizeChanged(object sender, EventArgs e)
        {
            if (null == LogViewModel)
            {
                return;
            }

            if (LogViewModel.IsReading)
            {
                return;
            }

            double dataGridActualHeight = Lines.ActualHeight;
            double rowHeight = Lines.RowHeight * Lines.Items.Count;

            if (dataGridActualHeight < rowHeight)
            {
                return;
            }

            LogViewModel.ExpandingView = true;
            long startingByteForNewRead = LogViewModel.LastLineEndingByte;
            LogViewModel.ContinueReadFromByteLocation(startingByteForNewRead, FindByteLocationActorMessages.SearchDirection.Backward, 1);
        }

        private void RawToggleButton_OnCheckChanged(object sender, RoutedEventArgs e)
        {
            bool rawView = RawToggleButton.IsChecked ?? false;

            LogViewModel.RawDisplayMode = rawView;
        }

        private void SearchCommand_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = LogViewModel?.HasLoaded ?? false;
        }

        private void SearchCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            SearchDialog searchDialog = new SearchDialog(LogViewModel);
            searchDialog.Show();
        }

        private void SeamlessScroll_OnChecked(object sender, RoutedEventArgs e)
        {
            if (null == LogViewModel)
            {
                return;
            }

            if (!LogViewModel.HasLoaded)
            {
                return;
            }

            bool seamlessScroll = SeamlessScroll.IsChecked ?? false;

            LogViewModel.SeamlessScroll = seamlessScroll;
        }
    }
}
