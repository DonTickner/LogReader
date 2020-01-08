using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using Akka.Actor;

namespace LogReader.Akka.Net.Actors
{
    public class UpdateUIActorMessage: IActorMessages
    {
        public class UpdateProgressBar
        {
            public Action<double> UpdateProgressBarMethod { get; }
            public double Step { get; }
            public UpdateProgressBar(double step, Action<double> updateProgressBarMethod)
            {
                Step = step;
                UpdateProgressBarMethod = updateProgressBarMethod;
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

        public class UpdateTotalLinesInLogFile
        {
            public long NumberOfLinesInLogFile { get; private set; }
            public string FilePathOfLogFile { get; private set; }
            public Action<string, long> UpdateMethod { get; private set; }
            public UpdateTotalLinesInLogFile(long numberOfLinesInLogFile, string filePathOfLogFile, Action<string, long> updateMethod)
            {
                NumberOfLinesInLogFile = numberOfLinesInLogFile;
                FilePathOfLogFile = filePathOfLogFile;
                UpdateMethod = updateMethod;
            }
        }

        public class LogSearchResult
        {
            public long ByteStartLocation { get; set; }
            public string FilePathOfLogFile { get; set; }
            public Action<string, long, byte[]> LogSearchResultAction { get; set; }
            public byte[] ItemFound { get; set; }

            public LogSearchResult(long byteStartLocation, string filePathOfLogFile, Action<string, long, byte[]> logSearchResultAction, byte[] itemFound)
            {
                ByteStartLocation = byteStartLocation;
                FilePathOfLogFile = filePathOfLogFile;
                LogSearchResultAction = logSearchResultAction;
                ItemFound = itemFound;
            }
        }
    }

    public class UpdateUIActor : BaseActorWithMessages<UpdateUIActorMessage>
    {
        public UpdateUIActor()
        {
        }

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case UpdateUIActorMessage.UpdateProgressBar updateProgressBar:
                {
                    updateProgressBar.UpdateProgressBarMethod.Invoke(updateProgressBar.Step);
                    break;
                }
                case UpdateUIActorMessage.DisplayMessageBox messageBoxMessage:
                {
                    MessageBox.Show(messageBoxMessage.MessageText, messageBoxMessage.MessageCaption, messageBoxMessage.Buttons, messageBoxMessage.Image);
                    break;
                }
                case UpdateUIActorMessage.UpdateTotalLinesInLogFile byteCountInFile:
                {
                    byteCountInFile.UpdateMethod.Invoke(byteCountInFile.FilePathOfLogFile, byteCountInFile.NumberOfLinesInLogFile);
                    break;
                }
                case UpdateUIActorMessage.LogSearchResult resultMessage:
                {
                    resultMessage.LogSearchResultAction.Invoke(resultMessage.FilePathOfLogFile, resultMessage.ByteStartLocation, resultMessage.ItemFound);
                    break;
                }
            }
        }
    }
}
