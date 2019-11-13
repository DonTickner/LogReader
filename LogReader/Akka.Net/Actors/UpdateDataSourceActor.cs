﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

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
        private readonly MainWindow _dataSourceToUpdate;

        public UpdateDataSourceActor(MainWindow dataSource)
        {
            _dataSourceToUpdate = dataSource;
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
            _dataSourceToUpdate.reading = !message.ReturnedLine.LastLine;

            if (message.OverrideUI)
            {
                _dataSourceToUpdate.ManualScrollBar.Value = message.ReturnedLine.LineStartsAtByteLocation / 10;
            }

            if (_dataSourceToUpdate.firstOnScreenLine.StartingByte == -1)
            {
                _dataSourceToUpdate.firstOnScreenLine.StartingByte = message.ReturnedLine.LineStartsAtByteLocation;
                _dataSourceToUpdate.firstOnScreenLine.EndingByte = message.ReturnedLine.LineEndsAtByteLocation;
                _dataSourceToUpdate.onScreenLines.StartingByte = message.ReturnedLine.LineStartsAtByteLocation;
            }

            _dataSourceToUpdate.LogViewModel.AddLine(message.ReturnedLine);

            _dataSourceToUpdate.onScreenLines.EndingByte = message.ReturnedLine.LineEndsAtByteLocation;
            _dataSourceToUpdate.LineTextBox.Text += message.ReturnedLine.Line;

        }
    }
}