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
        public class AddLineToDataSource
        {
            public ReadLineFromFileActor.ReturnedLine ReturnedLine { get; private set; }

            public bool OverrideUI { get; private set; }

            public AddLineToDataSource(ReadLineFromFileActor.ReturnedLine returnedLine, bool overrideUi)
            {
                ReturnedLine = returnedLine;
                OverrideUI = overrideUi;
            }
        }

        public AddLineToDataSource CreateNewAddLineToDataSourceMessage(ReadLineFromFileActor.ReturnedLine returnedLine, bool overrideUi)
        {
            return new AddLineToDataSource(returnedLine, overrideUi);
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
                case UpdateDataSourceActorMessage.AddLineToDataSource addLineToDataSourceMessage:
                {
                    AddLineToDataSource(addLineToDataSourceMessage);
                    break;
                }
            }
        }

        private void AddLineToDataSource(UpdateDataSourceActorMessage.AddLineToDataSource message)
        {
            if (message.OverrideUI)
            {
                long relativeStartReference =
                    _logViewModel.CreateRelativeByteReference(message.ReturnedLine.LineStartsAtByteLocation,
                        message.ReturnedLine.FilePath);

                _logViewModel.CurrentScrollPosition = relativeStartReference / 10;
            }

            _logViewModel.AddLine(message.ReturnedLine);

            long startingByte = message.ReturnedLine.LineEndsAtByteLocation;

            if (_logViewModel.IsReading
            || _logViewModel.ExpandingView)
            {
                long relativeReference =
                    _logViewModel.CreateRelativeByteReference(startingByte,
                        message.ReturnedLine.FilePath);

                Sender.Tell(
                    new ReadLineFromFileActorMessages.ReadLineFromFileStartingAtByte(
                        _logViewModel.LocateLogFileFromByteReference(relativeReference),
                        _logViewModel.TranslateRelativeBytePosition(relativeReference),
                        false));
            }
        }
    }
}
