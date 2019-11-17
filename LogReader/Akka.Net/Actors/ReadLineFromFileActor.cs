using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using LogReader.Configuration;

namespace LogReader.Akka.Net.Actors
{
    public class ReadLineFromFileActorMessages : IActorMessages
    {
        public class ReadLineFromFileStartingAtByte
        {
            public string FilePath { get; private set; }

            public long StartingByteNumber { get; private set; }

            public bool OverrideUI { get; private set; }

            public ReadLineFromFileStartingAtByte(string filePath, long startingByteNumber, bool overrideUi)
            {
                FilePath = filePath;
                StartingByteNumber = startingByteNumber;
                OverrideUI = overrideUi;
            }
        }

        /// <summary>
        /// Creates a new <see cref="ReadLineFromFileStartingAtByte"/> message.
        /// </summary>
        /// <param name="filePath">The physical file path of the file.</param>
        /// <param name="startByteNumber">The byte number to start reading from/</param>
        public ReadLineFromFileStartingAtByte CreateNewReadLineFromFileStartingAtByteMessage(string filePath, int startByteNumber, bool overriderUi)
        {
            return new ReadLineFromFileStartingAtByte(
                filePath,
                startByteNumber,
                overriderUi);
        }
    }

    public class ReadLineFromFileActor: BaseActorWithMessages<ReadLineFromFileActorMessages>
    {
        public class ReturnedLine
        {
            public string Line { get; set; }

            public long LineEndsAtByteLocation { get; set; }

            public long LineStartsAtByteLocation { get; set; }

            public bool CompletedRead { get; set; }

            /// <summary>
            /// Represents if the line returned is the last line in the file.
            /// </summary>
            public bool LastLine { get; set; }
        }

        private IActorRef _updateDataSourceActor;

        public ReadLineFromFileActor(IActorRef updateDataSourceActor)
        {
            _updateDataSourceActor = updateDataSourceActor ?? throw new ArgumentNullException(nameof(updateDataSourceActor));
        }

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case ReadLineFromFileActorMessages.ReadLineFromFileStartingAtByte readLineFromFileMessage:
                {
                    ReturnedLine returnedLine = ReadLineFromFile(readLineFromFileMessage);
                    _updateDataSourceActor.Tell(new UpdateDataSourceActorMessage.AddLineToDataSource(returnedLine, readLineFromFileMessage.OverrideUI));
                    break;
                }
            }
        }

        private ReturnedLine ReadLineFromFile(ReadLineFromFileActorMessages.ReadLineFromFileStartingAtByte message)
        {
            string filePath = message.FilePath;
            if (!File.Exists(filePath))
            {
                return null;
            }

            long fileSizeInBytes = new FileInfo(filePath).Length;
            long chunkStartingAtByte = message.StartingByteNumber;
            byte[] buffer = new byte[ProgramConfig.ChunkSize];
            ReturnedLine lineRead = new ReturnedLine
            {
                Line = string.Empty,
                LineEndsAtByteLocation = chunkStartingAtByte,
                LineStartsAtByteLocation = message.StartingByteNumber,
                CompletedRead = false
            };

            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                while (!lineRead.CompletedRead
                        && chunkStartingAtByte < fileSizeInBytes)
                {
                    fileStream.Seek(Convert.ToInt32(chunkStartingAtByte), SeekOrigin.Begin);
                    fileStream.Read(buffer, 0, ProgramConfig.ChunkSize);
                    lineRead = ReadLineFromByteChunk(buffer, lineRead);
                    chunkStartingAtByte += lineRead.LineEndsAtByteLocation;
                }

                if(!lineRead.CompletedRead)
                {
                    lineRead.LastLine = lineRead.CompletedRead = true;
                }
            }

            return lineRead;
        }

        private ReturnedLine ReadLineFromByteChunk(byte[] bytesToRead, ReturnedLine lineRead)
        {
            for (int i = 0; i < bytesToRead.Length; i++)
            {
                lineRead.LineEndsAtByteLocation++;
                byte b = bytesToRead[i];

                if (b == ProgramConfig.NullByte)
                {
                    continue;
                }
                
                if (b != ProgramConfig.LineFeedByte)
                {
                    char currentByte = Convert.ToChar(b);
                    lineRead.Line += currentByte;
                    continue;
                }

                lineRead.CompletedRead = true;
                break;
            }

            return lineRead;
        }
    }
}
