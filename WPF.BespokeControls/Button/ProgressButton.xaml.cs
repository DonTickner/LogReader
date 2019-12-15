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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPF.BespokeControls.Button
{
    /// <summary>
    /// Interaction logic for ProgressButton.xaml
    /// </summary>
    public partial class ProgressButton : System.Windows.Controls.Button
    {
        #region Progress Bar Properties

        private double _minProgressValue;
        public double MinValue
        {
            get
            {
                return _minProgressValue;
            }
            set
            {
                _minProgressValue = value;
            }
        }

        private double _maxProgressValue;
        public double MaxValue
        {
            get
            {
                return _maxProgressValue;
            }
            set
            {
                _maxProgressValue = value;
            }
        }

        private double _currentProgressValue;
        public double CurrentValue
        {
            get
            {
                return _currentProgressValue;
            }
            set
            {
                _currentProgressValue = value;
                UpdateProgressBar();
            }
        }

        private double _stepValue;
        public double StepValue
        {
            get
            {
                return _stepValue;
            }
            set
            {
                _stepValue = value;
            }
        }

        public Brush ProgressBackground
        {
            get
            {
                return rectangle.Fill;
            }
            set
            {
                rectangle.Fill = value;
            }
        }

        public double ProgressOpacity
        {
            get
            {
                return rectangle.Opacity;
            }
            set
            {
                rectangle.Opacity = value;
            }
        }

        public Visibility ProgressBarVisibility
        {
            get
            {
                return rectangle.Visibility;
            }
            set
            {
                rectangle.Visibility = value;
            }
        }

        #endregion


        public new double Width
        {
            get
            {
                return base.Width;
            }
            set
            {
                rectangle.Width = value;
                base.Width = value;
            }
        }

        public new double Height
        {
            get
            {
                return base.Height;
            }
            set
            {
                rectangle.Height = value;
                base.Height = value;
            }
        }

        public string Text
        {
            get
            {
                return textBlock.Text;
            }
            set
            {
                textBlock.Text = value;
            }
        }

        public HorizontalAlignment TextAlignment
        {
            get
            {
                return textBlock.HorizontalAlignment;
            }
            set
            {
                textBlock.HorizontalAlignment = value;
            }
        }

        public ProgressButton()
        {
            InitializeComponent();
        }

        public void PerformStep(double currentProgressPercent = 0)
        {
            _currentProgressValue = (Math.Max(Math.Min(
                currentProgressPercent == 0 ? _currentProgressValue + _stepValue : _maxProgressValue * currentProgressPercent
                , _maxProgressValue), _minProgressValue));

            UpdateProgressBar();
        }

        private void UpdateProgressBar()
        {
            double newProgressWidth = Math.Min(base.ActualWidth, base.ActualWidth * (_currentProgressValue / _maxProgressValue));
            rectangle.Width = double.IsNaN(newProgressWidth) ? 0.00 : newProgressWidth;
            if (Math.Abs(rectangle.Width - base.ActualWidth) < 0.01)
            {
                IsEnabled = true;
            }
        }
    }
}
