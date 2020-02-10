using System;
using System.Collections.Generic;
using System.Text;
using Akka.Actor;
using Log4Net.Extensions.Configuration.Implementation.ConfigObjects;
using Log4Net.Extensions.Configuration.Implementation.LogObjects;
using static LogReader.Akka.Net.Actors.ReadLineFromFileActor;

namespace LogReader.Akka.Net.Actors
{
    public struct DelimitedLogLine
    {
        public string RawLine { get; set; }

        public List<LogLineField> Fields { get; set; }

        public string FilePath { get; set; }

        public long LineEndsAtByteLocation { get; set; }
        
        public long LineStartsAtByteLocation { get; set; }
    }

    public class DelimitLogLineActorMessages : IActorMessages
    {
        public class DelimitLogLineMessage
        {
            public ReturnedLine ReturnedLine { get; private set; }

            public DelimitLogLineMessage(ReturnedLine returnedLine)
            {
                ReturnedLine = returnedLine;
            }
        }
    }

    public class DelimitLogLineActor: BaseActorWithMessages<DelimitLogLineActorMessages>
    {
        private readonly IActorRef _updateDataSourceActor;
        private readonly AppenderLayout _appenderLayout;

        public DelimitLogLineActor(IActorRef updateDataSourceActor, AppenderLayout appenderLayout)
        {
            _updateDataSourceActor = updateDataSourceActor;
            _appenderLayout = appenderLayout;
        }

        protected override void OnReceive(object message)
        {
            switch(message)
            {
                case DelimitLogLineActorMessages.DelimitLogLineMessage delimitedLogLineMessage:
                    {
                        _updateDataSourceActor.Tell(new UpdateDataSourceActorMessage.AddDelimitedLineToDataSource(
                            CreateDelimitedLogLineFromReturnedLine(delimitedLogLineMessage.ReturnedLine)));
                        break;
                    }
            }
        }

        private DelimitedLogLine CreateDelimitedLogLineFromReturnedLine(ReturnedLine returnedLine)
        {
            string rawLine = returnedLine.Line;

            List<LogLineField> individualFields = new List<LogLineField>();

            DelimitedLogLine delimitedLogLine = new DelimitedLogLine
            {
                RawLine = rawLine,
                FilePath = returnedLine.FilePath,
                LineStartsAtByteLocation = returnedLine.LineStartsAtByteLocation,
                LineEndsAtByteLocation = returnedLine.LineEndsAtByteLocation
            };

            int i = 0;

            if (rawLine.Contains(_appenderLayout.Header)
                || rawLine.Contains(_appenderLayout.Footer)
                || string.IsNullOrWhiteSpace(rawLine))
            {
                foreach(LogLineField field in _appenderLayout.ConversionPattern.Fields)
                {
                    string content = field.Name == "Message" ? rawLine : string.Empty;

                    individualFields.Add(new LogLineField
                    {
                        Name = field.Name,
                        Content = content
                    });
                }
            }
            else
            {
                try
                {
                    foreach(LogLineField field in _appenderLayout.ConversionPattern.Fields)
                    {
                        if (field.Type == LogLineType.FixedWidth)
                        {
                            individualFields.Add(new LogLineField{
                                Name = field.Name,
                                Content = rawLine.Substring(i, field.FixedWidth).Trim(),
                            });                           
                            
                            i += field.FixedWidth;
                        }
                        else if (field.Type == LogLineType.Delimited)
                        {
                            int startPos = field.MinWidth + i;
                            int fieldEndPos = rawLine.IndexOf(field.EndingCharacter, startPos);

                            if (fieldEndPos == i)
                            {
                                fieldEndPos = rawLine.Length;
                            }

                            int fieldLength = fieldEndPos - i;

                            individualFields.Add(new LogLineField{ 
                                Name = field.Name,
                                Content = rawLine.Substring(i, fieldLength).Trim()
                            });
                        
                            i += fieldLength;
                        }
                    }
                }
                catch
                {
                    individualFields.Clear();

                    foreach(LogLineField field in _appenderLayout.ConversionPattern.Fields)
                    {
                        string content = field.Name == "Message" ? rawLine : string.Empty;

                        individualFields.Add(new LogLineField
                        {
                            Name = field.Name,
                            Content = content
                        });
                    }
                }
            }

            delimitedLogLine.Fields = individualFields;

            return delimitedLogLine;
        }
    }
}
