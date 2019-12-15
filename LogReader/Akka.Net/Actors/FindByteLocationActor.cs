using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Akka.Actor;
using LogReader.Configuration;

namespace LogReader.Akka.Net.Actors
{
    public class FindByteLocationActorMessages : IActorMessages
    {
        public enum SearchDirection
        {
            Forward,
            Backward
        }

        /// <summary>
        /// A message that triggers a <see cref="FileByteLocationActor"/> to begin searching for a specific byte.
        /// </summary>
        public class FindNumeredInstanceOfByteLocationInFile
        {
            public long StartingByteNumber { get; private set; }

            public byte ByteToFind { get; private set; }

            public SearchDirection SearchDirection { get; private set; }

            public string FilePath { get; private set; }

            public long InstanceNumberToFind { get; private set; }

            public bool OverrideUI { get; private set; }

            public FindNumeredInstanceOfByteLocationInFile(long startingByte, byte byteToFind, SearchDirection searchDirection, string filePath, long instanceNumberToFind, bool overrideUI)
            {
                StartingByteNumber = startingByte;
                ByteToFind = byteToFind;
                SearchDirection = searchDirection;
                FilePath = filePath;
                InstanceNumberToFind = instanceNumberToFind;
                OverrideUI = overrideUI;
            }
        }

        /// <summary>
        /// A message that triggers a <see cref="FileByteLocationActor"/> to begin searching for a specific byte.
        /// </summary>
        public class FindNumeredInstanceOfByteLocationInFileAndUpdateOnProgress
        {
            public long StartingByteNumber { get; private set; }

            public byte ByteToFind { get; private set; }

            public SearchDirection SearchDirection { get; private set; }

            public string FilePath { get; private set; }

            public long InstanceNumberToFind { get; private set; }

            public bool OverrideUI { get; private set; }

            public IActorRef UpdateUIActor { get; private set; }

            public FindNumeredInstanceOfByteLocationInFileAndUpdateOnProgress(long startingByte, byte byteToFind, SearchDirection searchDirection, string filePath, long instanceNumberToFind, bool overrideUI, IActorRef updateUIActor)
            {
                StartingByteNumber = startingByte;
                ByteToFind = byteToFind;
                SearchDirection = searchDirection;
                FilePath = filePath;
                InstanceNumberToFind = instanceNumberToFind;
                OverrideUI = overrideUI;
                UpdateUIActor = updateUIActor;
            }
        }
    }

    /// <summary>
    /// An <see cref="Actor"/> who can locate a specific byte.
    /// </summary>
    public class FindByteLocationActor: BaseActorWithMessages<FindByteLocationActorMessages>
    {
        private const int MaximumUIUpdates = 100;

        private readonly IActorRef _readLineFromFileActor;

        public FindByteLocationActor(IActorRef readLineFromFileActor)
        {
            _readLineFromFileActor =
                readLineFromFileActor ?? throw new ArgumentNullException(nameof(readLineFromFileActor));
        }

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case FindByteLocationActorMessages.FindNumeredInstanceOfByteLocationInFile findInstanceInFileMessage:
                {
                    FindByteLocationActorMessages.FindNumeredInstanceOfByteLocationInFileAndUpdateOnProgress newMessage =
                            new FindByteLocationActorMessages.FindNumeredInstanceOfByteLocationInFileAndUpdateOnProgress(findInstanceInFileMessage.StartingByteNumber, findInstanceInFileMessage.ByteToFind, findInstanceInFileMessage.SearchDirection, findInstanceInFileMessage.FilePath, findInstanceInFileMessage.InstanceNumberToFind, findInstanceInFileMessage.OverrideUI, null);

                    long byteLocation = SearchForByteInFile(newMessage);
                    if (byteLocation >= 0)
                    {
                        _readLineFromFileActor.Tell(new ReadLineFromFileActorMessages.ReadLineFromFileStartingAtByte(findInstanceInFileMessage.FilePath, byteLocation, findInstanceInFileMessage.OverrideUI));
                    }
                    break;
                }
                case FindByteLocationActorMessages.FindNumeredInstanceOfByteLocationInFileAndUpdateOnProgress findInstanceInFileMessage:
                    {
                        long byteLocation = SearchForByteInFile(findInstanceInFileMessage);
                        if (byteLocation >= 0)
                        {
                            _readLineFromFileActor.Tell(new ReadLineFromFileActorMessages.ReadLineFromFileStartingAtByte(findInstanceInFileMessage.FilePath, byteLocation, findInstanceInFileMessage.OverrideUI));
                        }
                        break;
                    }
            }
        }

        /// <summary>
        /// Processes a <see cref="FindByteLocationInFile"/> message.
        /// </summary>
        /// <param name="message">The <see cref="FindByteLocationInFile"/> message to process.</param>
        private long SearchForByteInFile(FindByteLocationActorMessages.FindNumeredInstanceOfByteLocationInFileAndUpdateOnProgress message)
        {
            IActorRef updateUIActor = message.UpdateUIActor;

            if (message.StartingByteNumber == 0
                && message.SearchDirection == FindByteLocationActorMessages.SearchDirection.Backward)
            {
                CompleteSearch(updateUIActor);
                return 0;
            }

            string filePath = message.FilePath;
            if (!File.Exists(filePath))
            {
                return -1;
            }

            if (message.InstanceNumberToFind == 0)
            {
                CompleteSearch(updateUIActor);
                return 0;
            }

            long fileSizeInBytes = new FileInfo(filePath).Length;

            long startingByte = message.StartingByteNumber;
            long chunkStartingAtByte = message.StartingByteNumber;
            int searchDirection = 1;

            if (message.SearchDirection == FindByteLocationActorMessages.SearchDirection.Backward)
            {
                searchDirection = -1;
                chunkStartingAtByte = Math.Max(0, chunkStartingAtByte - ProgramConfig.ChunkSize);
                startingByte = Math.Min(ProgramConfig.ChunkSize, startingByte) - 1;
            }
            else
            {
                if (startingByte > ProgramConfig.ChunkSize)
                {
                    startingByte = startingByte % ProgramConfig.ChunkSize;
                }
            }

            byte[] buffer = new byte[ProgramConfig.ChunkSize];

            int numberFound = 0;

            decimal numberOfLoops = message.InstanceNumberToFind + 1;
            decimal uiUpdates = Math.Min(MaximumUIUpdates, numberOfLoops);

            decimal progressStep = (numberOfLoops) / uiUpdates;

            decimal progressToProgressStep = progressStep;

            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                while (chunkStartingAtByte < fileSizeInBytes
                       && numberFound < message.InstanceNumberToFind)
                {
                    fileStream.Seek(Convert.ToInt32(chunkStartingAtByte), SeekOrigin.Begin);

                    long amountToRead = Math.Min(ProgramConfig.ChunkSize, fileSizeInBytes - chunkStartingAtByte);

                    fileStream.Read(buffer, 0, (int)amountToRead);

                    long byteLocation = SearchForByteInChunk(buffer, message.ByteToFind, searchDirection, startingByte);
                    bool byteFound = byteLocation > -1;

                    startingByte = Math.Max(ProgramConfig.ChunkSize * -searchDirection, 0);
                    if (byteFound)
                    {
                        numberFound += 1;
                        progressToProgressStep += 1;

                        if (progressToProgressStep >= progressStep)
                        {
                            progressToProgressStep = Math.Max(0, progressToProgressStep - progressStep);
                            UpdateProgress(updateUIActor, numberFound / numberOfLoops);
                        }
                    }

                    if (numberFound >= message.InstanceNumberToFind)
                    {
                        CompleteSearch(updateUIActor);
                        return chunkStartingAtByte + byteLocation + 1;
                    }

                    chunkStartingAtByte = Math.Max(chunkStartingAtByte + (byteFound ? (byteLocation + 1) : amountToRead), 0);                                                          
                }
            }

            SearchError(updateUIActor);
            return -1;
        }

        /// <summary>
        /// Searches through the <see cref="byte"/>[] for the specified <see cref="byte"/>, in the direction indicated.
        /// </summary>
        /// <param name="bytesToSearch">The array of <see cref="byte"/>s to search through.</param>
        /// <param name="byteToFind">The <see cref="byte"/> to locate.</param>
        /// <param name="searchDirection">The iteration direction for the <see cref="byte"/> array. 1 for Forward, -1 for Backward.</param>
        private long SearchForByteInChunk(byte[] bytesToSearch, byte byteToFind, int searchDirection, long startingPosition)
        {
            int loopEnd = searchDirection > 0 ? bytesToSearch.Length : 0;
            int loopIncrement = 1 * searchDirection;
            int safeStartingPosition = (int)Math.Min(Math.Max(0, startingPosition), bytesToSearch.Length - 1);

            for (long i = safeStartingPosition; i != loopEnd; i += loopIncrement)
            {
                if (bytesToSearch[i] == byteToFind)
                {
                    return i;
                }
            }

            return -1;
        }

        private void UpdateProgress(IActorRef actor, decimal percentageComplete)
        {
            actor?.Tell(new UpdateUIActorMessage.UpdateProgressBar((double)percentageComplete));
        }

        private void CompleteSearch(IActorRef actor)
        {
            UpdateProgress(actor, 1);
        }

        private void SearchError(IActorRef actor)
        {
            actor.Tell(new UpdateUIActorMessage.DisplayMessageBox("End of File exceeded. Unable to proceed.", "File Navigation Error", MessageBoxButton.OK, MessageBoxImage.Exclamation));
            CompleteSearch(actor);
        }
    }
}
