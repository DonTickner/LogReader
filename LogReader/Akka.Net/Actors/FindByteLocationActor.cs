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
        public class FindNumberedInstanceOfByteLocationInFile
        {
            public long StartingByteNumber { get; private set; }

            public byte ByteToFind { get; private set; }

            public SearchDirection SearchDirection { get; private set; }

            public string FilePath { get; private set; }

            public long InstanceNumberToFind { get; private set; }

            public bool OverrideUI { get; private set; }

            public FindNumberedInstanceOfByteLocationInFile(long startingByte, byte byteToFind, SearchDirection searchDirection, string filePath, long instanceNumberToFind, bool overrideUI)
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
        public class FindNumberedInstanceOfByteLocationInFileAndUpdateOnProgress
        {
            public long StartingByteNumber { get; private set; }

            public byte ByteToFind { get; private set; }

            public SearchDirection SearchDirection { get; private set; }

            public string FilePath { get; private set; }

            public long InstanceNumberToFind { get; private set; }

            public bool OverrideUI { get; private set; }

            public IActorRef UpdateUIActor { get; private set; }

            public Action<double> UpdateAction { get; private set; }

            public FindNumberedInstanceOfByteLocationInFileAndUpdateOnProgress(long startingByte, byte byteToFind, SearchDirection searchDirection, string filePath, long instanceNumberToFind, bool overrideUI, IActorRef updateUIActor, Action<double> updateAction)
            {
                StartingByteNumber = startingByte;
                ByteToFind = byteToFind;
                SearchDirection = searchDirection;
                FilePath = filePath;
                InstanceNumberToFind = instanceNumberToFind;
                OverrideUI = overrideUI;
                UpdateUIActor = updateUIActor;
                UpdateAction = updateAction;
            }
        }

        /// <summary>
        /// A message that triggers a <see cref="FindByteLocationActor"/> to being counting the number of occurrences of a byte within a file.
        /// </summary>
        public class CountByteOccurrencesInFile
        {
            public byte ByteToFind { get; private set; }

            public string FilePath { get; private set; }

            public IActorRef ActorToAdviseCountOf { get; private set; }

            public Action<string, long> MethodToUpdateCount { get; private set; }

            public CountByteOccurrencesInFile(byte byteToFind, string filePath, IActorRef actorToAdviseCountOf, Action<string, long> methodToUpdateCount)
            {
                ByteToFind = byteToFind;
                FilePath = filePath;
                ActorToAdviseCountOf = actorToAdviseCountOf;
                MethodToUpdateCount = methodToUpdateCount;
            }
        }

        public class FindByteOccurrencesWithinAFile
        {
            public bool StopAfterSearch { get; set; }
            public byte[] BytesToFind { get; set; }
            public string FilePath { get; set; }
            public IActorRef ActorToAdviseHitOf { get; set; }
            public Action<string, long, byte[]> MethodToRegisterHit { get; set; }

            public FindByteOccurrencesWithinAFile(byte[] bytesToFind, string filePath, IActorRef actorToAdviseHitOf, Action<string, long, byte[]> methodToRegisterHit, bool stopAfterSearch)
            {
                BytesToFind = bytesToFind;
                FilePath = filePath;
                ActorToAdviseHitOf = actorToAdviseHitOf;
                MethodToRegisterHit = methodToRegisterHit;
                StopAfterSearch = stopAfterSearch;
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
            _readLineFromFileActor = readLineFromFileActor;
        }

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case FindByteLocationActorMessages.FindNumberedInstanceOfByteLocationInFile findInstanceInFileMessage:
                    {
                        long byteLocation = InitiateSearchForByteInFile(findInstanceInFileMessage);
                        if (byteLocation >= 0)
                        {
                            _readLineFromFileActor.Tell(new ReadLineFromFileActorMessages.ReadLineFromFileStartingAtByte(findInstanceInFileMessage.FilePath, byteLocation, findInstanceInFileMessage.OverrideUI));
                        }
                        break;
                    }
                case FindByteLocationActorMessages.FindNumberedInstanceOfByteLocationInFileAndUpdateOnProgress findInstanceInFileMessage:
                    {
                        long byteLocation = InitiateSearchForByteInFile(findInstanceInFileMessage);
                        if (byteLocation >= 0)
                        {
                            _readLineFromFileActor.Tell(new ReadLineFromFileActorMessages.ReadLineFromFileStartingAtByte(findInstanceInFileMessage.FilePath, byteLocation, findInstanceInFileMessage.OverrideUI));
                        }
                        break;
                    }
                case FindByteLocationActorMessages.CountByteOccurrencesInFile countByteMessage:
                    {
                        long byteCount = CountByteOccurrencesInFile(countByteMessage);
                        countByteMessage.ActorToAdviseCountOf.Tell(new UpdateUIActorMessage.UpdateTotalLinesInLogFile(byteCount, countByteMessage.FilePath, countByteMessage.MethodToUpdateCount));
                        break;
                    }
                case FindByteLocationActorMessages.FindByteOccurrencesWithinAFile searchByteMessage:
                    {
                        SearchByteBytesInFileAndAction(searchByteMessage.FilePath, searchByteMessage.BytesToFind, searchByteMessage.ActorToAdviseHitOf, searchByteMessage.MethodToRegisterHit);
                        if (searchByteMessage.StopAfterSearch)
                        {
                            searchByteMessage.ActorToAdviseHitOf.Tell(PoisonPill.Instance);
                        }
                        break;
                    }
            }
        }

        private long InitiateSearchForByteInFile(FindByteLocationActorMessages.FindNumberedInstanceOfByteLocationInFileAndUpdateOnProgress message)
        {
            return SearchForByteInFile(message.StartingByteNumber, message.SearchDirection, message.UpdateUIActor,
                message.UpdateAction, message.FilePath, message.InstanceNumberToFind, message.ByteToFind);
        }

        private long InitiateSearchForByteInFile(FindByteLocationActorMessages.FindNumberedInstanceOfByteLocationInFile message)
        {
            return SearchForByteInFile(message.StartingByteNumber, message.SearchDirection, null, null,
                message.FilePath, message.InstanceNumberToFind, message.ByteToFind);
        }

        private long SearchForByteInFile(long startingByteNumber, FindByteLocationActorMessages.SearchDirection searchDirection, IActorRef updateUIActor, Action<double> uiUpdateMethod, string pathOfFileToSearch, long numberOfByteInstancesToFind, byte byteToFind)
        {
            if (startingByteNumber == 0
                && searchDirection == FindByteLocationActorMessages.SearchDirection.Backward)
            {
                CompleteSearch(updateUIActor, uiUpdateMethod);
                return 0;
            }

            string filePath = pathOfFileToSearch;
            if (!File.Exists(filePath))
            {
                return -1;
            }

            if (numberOfByteInstancesToFind == 0)
            {
                CompleteSearch(updateUIActor, uiUpdateMethod);
                return 0;
            }

            long fileSizeInBytes = new FileInfo(filePath).Length;

            long chunkStartingAtByte = startingByteNumber;
            int searchDirectionInt = 1;

            if (searchDirection == FindByteLocationActorMessages.SearchDirection.Backward)
            {
                searchDirectionInt = -1;
                chunkStartingAtByte = chunkStartingAtByte - ProgramConfig.ChunkSize;
            }

            byte[] buffer = new byte[ProgramConfig.ChunkSize];

            int numberFound = 0;

            decimal numberOfLoops = numberOfByteInstancesToFind + 1;
            decimal uiUpdates = Math.Min(MaximumUIUpdates, numberOfLoops);

            decimal progressStep = (numberOfLoops) / uiUpdates;

            decimal progressToProgressStep = progressStep;
            long amountToRead = ProgramConfig.ChunkSize;

            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                while (chunkStartingAtByte < fileSizeInBytes
                       && numberFound < numberOfByteInstancesToFind)
                {

                    long startingByte = Math.Max(ProgramConfig.ChunkSize * -searchDirectionInt, 0);

                    if (searchDirectionInt > 0)
                    {
                        amountToRead = Math.Min(ProgramConfig.ChunkSize, fileSizeInBytes - chunkStartingAtByte);
                    }
                    else
                    {
                        amountToRead = Math.Min(amountToRead + chunkStartingAtByte, ProgramConfig.ChunkSize);
                        chunkStartingAtByte = Math.Max(chunkStartingAtByte, 0);
                        startingByte = amountToRead;
                    }

                    if (amountToRead <= 0)
                    {
                        return 0;
                    }

                    fileStream.Seek(Convert.ToInt32(chunkStartingAtByte), SeekOrigin.Begin);
                    fileStream.Read(buffer, 0, (int)amountToRead);

                    long byteLocation = SearchForByteInChunk(buffer, byteToFind, searchDirectionInt, startingByte);
                    bool byteFound = byteLocation > -1;

                    if (byteFound)
                    {
                        numberFound += 1;
                        progressToProgressStep += 1;

                        if (progressToProgressStep >= progressStep)
                        {
                            progressToProgressStep = Math.Max(0, progressToProgressStep - progressStep);
                            UpdateProgress(updateUIActor, uiUpdateMethod, numberFound / numberOfLoops);
                        }
                    }
                    else if (searchDirectionInt < 0)
                    {
                        if (chunkStartingAtByte - amountToRead <= 0)
                        {
                            return 0;
                        }

                    }

                    if (numberFound >= numberOfByteInstancesToFind)
                    {
                        CompleteSearch(updateUIActor, uiUpdateMethod);
                        return chunkStartingAtByte + byteLocation + 1;
                    }

                    if (searchDirectionInt > 0)
                    {
                        chunkStartingAtByte = chunkStartingAtByte + (byteFound ? byteLocation + 1 : amountToRead);
                    }
                    else
                    {
                        chunkStartingAtByte = chunkStartingAtByte + (byteFound ? byteLocation - 1 - amountToRead
                            : amountToRead);
                    }
                }
            }

            SearchError(updateUIActor, uiUpdateMethod);
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

        /// <summary>
        /// Counts the number of lines in the passed byte array
        /// </summary>
        /// <param name="bytesToRead">The byte array that represent a memory chunk</param>
        private long CountByteOccurrencesInChunk(byte[] bytesToRead, byte byteToFind)
        {
            return bytesToRead.Sum(currentByte => currentByte == byteToFind ? 1 : 0);
        }

        private long CountByteOccurrencesInFile(FindByteLocationActorMessages.CountByteOccurrencesInFile message)
        {
            string filePath = message.FilePath;
            byte byteToCount = message.ByteToFind;

            if (!File.Exists(filePath))
            {
                return -1;
            }

            long fileSizeInBytes = new FileInfo(filePath).Length;
            long chunkStartingAtByte = 0;
            byte[] buffer = new byte[ProgramConfig.ChunkSize];

            long numberFound = 0;

            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                while (chunkStartingAtByte < fileSizeInBytes)
                {
                    fileStream.Seek(Convert.ToInt32(chunkStartingAtByte), SeekOrigin.Begin);
                    long amountToRead = Math.Min(ProgramConfig.ChunkSize, fileSizeInBytes - chunkStartingAtByte);
                    fileStream.Read(buffer, 0, (int)amountToRead);
                    numberFound += CountByteOccurrencesInChunk(buffer, message.ByteToFind);
                    chunkStartingAtByte = Math.Max(chunkStartingAtByte + amountToRead, 0);
                }
            }

            return numberFound;
        }

        private long SearchForBytesInChunkAndActionResults(Action<string, long, byte[]> updateAction, string filePath, long chunkStartBytePos, byte[] bytesToSearch, long startingPosition, byte[] bytesToFind, IActorRef actorToReportResultTo, long indexOfFirstByteToFind = 0)
        {
            if (!bytesToFind.Any())
            {
                return 0;
            }

            long indexOfHitByte = indexOfFirstByteToFind;
            byte firstByteToFind = bytesToFind[indexOfHitByte];

            int loopEnd = bytesToSearch.Length;
            int loopIncrement = 1;
            int safeStartingPosition = (int)Math.Min(Math.Max(0, startingPosition), bytesToSearch.Length - 1);

            bool hit = false;
            long currentHitPosition = 0;
            
            for (long i = safeStartingPosition; i < loopEnd; i += loopIncrement)
            {
                hit = false;
                currentHitPosition = 0;

                if (bytesToSearch[i] != firstByteToFind)
                {
                    continue;
                }

                currentHitPosition = indexOfHitByte;
                hit = true;
                bool found = true;
                
                for (long nextByteToFind = indexOfHitByte + 1; nextByteToFind < bytesToFind.Length; nextByteToFind++)
                {
                    long nextByte = i + nextByteToFind;
                    if (nextByte >= loopEnd)
                    {
                        found = false;
                        i += bytesToFind.Length;
                        break;
                    }

                    if (bytesToSearch[nextByte] == bytesToFind[nextByteToFind])
                    {
                        currentHitPosition = nextByteToFind;
                        continue;
                    }

                    found = false;
                    indexOfHitByte = 0;
                    i += nextByteToFind;
                    break;
                }

                if (!found)
                {
                    continue;
                }

                i += bytesToFind.Length - 1;
                LogSearchResult(actorToReportResultTo,i + chunkStartBytePos, filePath, updateAction, bytesToFind);
            }

            return hit ? currentHitPosition : 0;
        }

        private void SearchByteBytesInFileAndAction(string pathOfFileToSearch, byte[] bytesToFind, IActorRef actorToReportSearchResultTo, Action<string, long, byte[]> updateAction)
        {
            string filePath = pathOfFileToSearch;

            if (!File.Exists(filePath))
            {
                return;
            }

            long fileSizeInBytes = new FileInfo(filePath).Length;
            long chunkStartingAtByte = 0;
            byte[] buffer = new byte[ProgramConfig.ChunkSize];

            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                long hitPosition = 0;

                while (chunkStartingAtByte < fileSizeInBytes)
                {
                    fileStream.Seek(Convert.ToInt32(chunkStartingAtByte), SeekOrigin.Begin);
                    long amountToRead = Math.Min(ProgramConfig.ChunkSize, fileSizeInBytes - chunkStartingAtByte);
                    fileStream.Read(buffer, 0, (int)amountToRead);
                    hitPosition = SearchForBytesInChunkAndActionResults(updateAction, filePath, chunkStartingAtByte, buffer, 0, bytesToFind, actorToReportSearchResultTo, hitPosition);
                    chunkStartingAtByte = Math.Max(chunkStartingAtByte + amountToRead, 0);
                }
            }
        }

        private void LogSearchResult(IActorRef actorToReportSearchResultTo, long resultPosition, string filePath, Action<string, long, byte[]> updateAction, byte[] itemFound)
        {
            actorToReportSearchResultTo.Tell(new UpdateUIActorMessage.LogSearchResult(resultPosition, filePath, updateAction, itemFound));
        }

        private void UpdateProgress(IActorRef actor, Action<double> updateAction, decimal percentageComplete)
        {
            if (null == updateAction)
            {
                return;
            }

            actor?.Tell(new UpdateUIActorMessage.UpdateProgressBar((double)percentageComplete, updateAction));
        }

        private void CompleteSearch(IActorRef actor, Action<double> updateAction)
        {
            UpdateProgress(actor, updateAction, 1);
        }

        private void SearchError(IActorRef actor, Action<double> updateAction)
        {
            actor?.Tell(new UpdateUIActorMessage.DisplayMessageBox("End of File exceeded. Unable to proceed.", "File Navigation Error", MessageBoxButton.OK, MessageBoxImage.Exclamation));
            CompleteSearch(actor, updateAction);
        }
    }
}
