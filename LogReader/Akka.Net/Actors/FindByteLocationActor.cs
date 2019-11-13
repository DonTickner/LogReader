﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
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
        public class FindByteLocationInFile
        {
            public long StartingByteNumber { get; private set; }

            public byte ByteToFind { get; private set; }

            public SearchDirection SearchDirection { get; private set; }

            public string FilePath { get; private set; }

            public int InstanceNumberToFind { get; private set; }

            public bool OverrideUI { get; private set; }

            public FindByteLocationInFile(long startingByte, byte byteToFind, SearchDirection searchDirection, string filePath, int instanceNumberToFind, bool overrideUI)
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
        /// Creates a new <see cref="FindByteLocationInFile"/> message.
        /// </summary>
        public FindByteLocationInFile CreateNewFindByteLocationInFileMessage(int startingByte, byte byteToFind, SearchDirection searchDirection, string filePath)
        {
            return new FindByteLocationInFile(
                startingByte,
                byteToFind,
                searchDirection,
                filePath,
                1,
                false);
        }
    }

    /// <summary>
    /// An <see cref="Actor"/> who can locate a specific byte.
    /// </summary>
    public class FindByteLocationActor: BaseActorWithMessages<FindByteLocationActorMessages>
    {
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
                case FindByteLocationActorMessages.FindByteLocationInFile findByteLocationInFileMessage:
                {
                    long byteLocation = SearchForByteInFile(findByteLocationInFileMessage);
                    if (byteLocation >= 0)
                    {
                        _readLineFromFileActor.Tell(new ReadLineFromFileActorMessages.ReadLineFromFileStartingAtByte(findByteLocationInFileMessage.FilePath, byteLocation, findByteLocationInFileMessage.OverrideUI));
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Processes a <see cref="FindByteLocationInFile"/> message.
        /// </summary>
        /// <param name="message">The <see cref="FindByteLocationInFile"/> message to process.</param>
        private long SearchForByteInFile(FindByteLocationActorMessages.FindByteLocationInFile message)
        {
            if (message.StartingByteNumber == 0
                && message.SearchDirection == FindByteLocationActorMessages.SearchDirection.Backward)
            {
                return 0;
            }

            string filePath = message.FilePath;
            if (!File.Exists(filePath))
            {
                return -1;
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

            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                while (chunkStartingAtByte < fileSizeInBytes
                       && numberFound < message.InstanceNumberToFind)
                {
                    fileStream.Seek(Convert.ToInt32(chunkStartingAtByte), SeekOrigin.Begin);
                    fileStream.Read(buffer, 0, ProgramConfig.ChunkSize);
                    long byteLocation = SearchForByteInChunk(buffer, message.ByteToFind, searchDirection, startingByte);

                    if (byteLocation >= 0)
                    {
                        numberFound++;
                        if (numberFound >= message.InstanceNumberToFind)
                        {
                            return chunkStartingAtByte + byteLocation + 1;
                        }

                        startingByte = byteLocation - 1;
                        continue;
                    }
                    startingByte = Math.Max(Math.Min(byteLocation + searchDirection, fileSizeInBytes), 0);

                    chunkStartingAtByte = Math.Max(chunkStartingAtByte + (ProgramConfig.ChunkSize * searchDirection), 0);
                    if (startingByte == 0 || startingByte == fileSizeInBytes)
                    {
                        return 0;
                    }
                }
            }

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

            for (long i = startingPosition; i != loopEnd; i += loopIncrement)
            {
                if (bytesToSearch[i] == byteToFind)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}