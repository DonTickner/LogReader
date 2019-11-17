using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using LogReader.Annotations;

namespace LogReader.Structure
{
    public class LogLine: NotifyPropertyChanged
    {
        private string _line;

        public string Line
        {
            get => _line;
            set
            {
                _line = value;
                OnPropertyChanged(nameof(Line));
            }
        }
    }

    public abstract class NotifyPropertyChanged : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
