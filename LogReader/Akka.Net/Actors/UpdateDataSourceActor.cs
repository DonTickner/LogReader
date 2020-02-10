using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Akka.Actor;
using LogReader.Structure;

namespace LogReader.Akka.Net.Actors
{
    public class UpdateDataSourceActorMessage: IActorMessages
    { 
        public class AddDelimitedLineToDataSource
        {
            public DelimitedLogLine DelimitedLogLine { get; private set; }

            public AddDelimitedLineToDataSource(DelimitedLogLine delimitedLogLine)
            {
                DelimitedLogLine = delimitedLogLine;
            }
        }
    }

    public class UpdateDataSourceActor: BaseActorWithMessages<UpdateDataSourceActorMessage>
    {
        private readonly LogViewModel _logViewModel;

        public UpdateDataSourceActor(LogViewModel logViewModel)
        {
            _logViewModel = logViewModel;
        }

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case UpdateDataSourceActorMessage.AddDelimitedLineToDataSource addDelimitedLineToDataSourceMessage:
                {
                    AddLineToDataSource(addDelimitedLineToDataSourceMessage);
                    break;
                }
            }
        }

        private void AddLineToDataSource(UpdateDataSourceActorMessage.AddDelimitedLineToDataSource message)
        {
            _logViewModel.AddLine(message.DelimitedLogLine);
        }
    }
}
