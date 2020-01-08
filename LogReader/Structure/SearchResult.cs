using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogReader.Structure
{
    public class SearchResult
    {
        public long ByteStartingPosition { get; private set; }

        public string LogFilePath { get; private set; }

        public byte[] Result { get; private set; }

        public string ResultString => Encoding.ASCII.GetString(Result);

        public SearchResult(long byteStartingPosition, string logFilePath, byte[] result)
        {
            ByteStartingPosition = byteStartingPosition;
            LogFilePath = logFilePath;
            Result = result;
        }

        public override string ToString()
        {
            return $"{LogFilePath} ({ByteStartingPosition}) - \"{ResultString}\"";
        }
    }

    public class SearchResultSorter : IComparer<SearchResult>
    {
        public int Compare(SearchResult x, SearchResult y)
        {
            if (x == null && y == null)
            {
                return 0;
            }

            if (x.ByteStartingPosition == y.ByteStartingPosition
                && x.LogFilePath == y.LogFilePath
                && x.Result == y.Result)
            {
                return 0;
            }

            if (x.LogFilePath != x.LogFilePath)
            {
                return String.Compare(x.LogFilePath, y.LogFilePath, StringComparison.Ordinal);
            }

            if (x.ByteStartingPosition > y.ByteStartingPosition)
            {
                return -1;
            }

            return 1;
        }
    }
}
