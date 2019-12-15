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
        /// Allows the user to move to a particular line within the current file.
        /// </summary>
        public static readonly RoutedUICommand GoToLineCommand = new RoutedUICommand
        (
            "Go To Line", "Go To Line", typeof(UICommands), new InputGestureCollection { new KeyGesture(Key.G, ModifierKeys.Control )
            }
        );

        public static readonly RoutedUICommand OpenLog4NetConfigCommand = new RoutedUICommand(
            "Open Log4Net Config", "Open Log4Net Config", typeof(UICommands), new InputGestureCollection { new KeyGesture(Key.O, ModifierKeys.Control )
            }
        );
    }
}
