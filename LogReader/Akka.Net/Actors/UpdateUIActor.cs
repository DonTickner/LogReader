using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Akka.Actor;

namespace LogReader.Akka.Net.Actors
{
    public class UpdateUIActorMessage: IActorMessages
    {
        public class UpdateProgressBar
        {
            public double Step { get; }
            public UpdateProgressBar(double step = 0)
            {
                Step = step;
            }
        }

        public class DisplayMessageBox
        {
            public string MessageText { get; }

            public string MessageCaption { get; }

            public MessageBoxButton Buttons { get; }

            public MessageBoxImage Image { get; }

            public DisplayMessageBox(string messageText, string messageCaption, MessageBoxButton buttons, MessageBoxImage image)
            {
                MessageText = messageText;
                MessageCaption = messageCaption;
                Buttons = buttons;
                Image = image;
            }
        }
    }

    public class UpdateUIActor : BaseActorWithMessages<UpdateUIActorMessage>
    {
        private readonly Action<double> _updateProgressBarMethod;

        public UpdateUIActor(Action<double> updateProgressBarMethod)
        {
            _updateProgressBarMethod = updateProgressBarMethod;
        }

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case UpdateUIActorMessage.UpdateProgressBar updateProgressBar:
                {
                    _updateProgressBarMethod.Invoke(updateProgressBar.Step);
                    break;
                }
                case UpdateUIActorMessage.DisplayMessageBox messageBoxMessage:
                {
                    MessageBox.Show(messageBoxMessage.MessageText, messageBoxMessage.MessageCaption, messageBoxMessage.Buttons, messageBoxMessage.Image);
                    break;
                }
            }
        }
    }
}
