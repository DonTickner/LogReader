using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPF.BespokeControls.TextBox
{
    /// <summary>
    /// Interaction logic for NumericalTextBox.xaml
    /// </summary>
    public partial class NumericalTextBox : System.Windows.Controls.TextBox
    {
        public NumericalTextBox()
        {
            InitializeComponent();

            this.PreviewKeyDown += OnPreviewKeyDown;
            this.PreviewTextInput += OnPreviewTextInput;
        }

        /// <summary>
        /// The actions to be performed when the user presses a key.
        /// </summary>
        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = e.Key == Key.Space;
        }

        /// <summary>
        /// The actions to be performed to validate the text entered into the text box.
        /// </summary>
        private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = int.TryParse(e.Text, out int result);
        }
    }
}
