using System;
using System.Collections.Generic;
using System.Text;

namespace LogReader.Structure
{
    public class LogFileInfo
    {
        public string FileLocation { get; private set; }
        public long FileSizeInByte { get; private set; }
        public long NumberOfLinesInFile { get; private set; }

        public LogFileInfo(long numberOfLinesInFile, string fileLocation, long fileSizeInByte)
        {
            NumberOfLinesInFile = numberOfLinesInFile;
            FileLocation = fileLocation;
            FileSizeInByte = fileSizeInByte;
        }

        public void SetLineCount(long lineCount)
        {
            NumberOfLinesInFile = lineCount;
        }
    }
}
