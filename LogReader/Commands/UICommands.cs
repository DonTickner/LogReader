using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace LogReader.Commands
{
    /// <summary>
    /// A static class to hold all UI commands
    /// </summary>
    public static class UICommands
    {
        /// <summary>
        /// Displays the UI control that allows the user to move to a particular line within the current file.
        /// </summary>
        public static readonly RoutedUICommand GoToLineCommand = new RoutedUICommand
        (
            "Go To Line", "Go To Line", typeof(UICommands), new InputGestureCollection { new KeyGesture(Key.G, ModifierKeys.Control )
            }
        );

        /// <summary>
        /// Triggers the user to select which Log4Net Config file should be loaded.
        /// </summary>
        public static readonly RoutedUICommand OpenLog4NetConfigCommand = new RoutedUICommand(
            "Open Log4Net Config", "Open Log4Net Config", typeof(UICommands), new InputGestureCollection { new KeyGesture(Key.O, ModifierKeys.Control )
            }
        );

        /// <summary>
        /// Displays the UI control that allows the user to search for content within the Log4Net Log files.
        /// </summary>
        public static readonly RoutedUICommand SearchCommand = new RoutedUICommand(
            "Search for Content", "Search for Content", typeof(UICommands), new InputGestureCollection { new KeyGesture(Key.F, ModifierKeys.Control )
            }
        );
    }
}
