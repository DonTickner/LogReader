using System;
using System.Collections.Generic;
using LogReader.Structure;
using NUnit.Framework;

namespace LogReader.UnitTests
{
    [TestFixture]
    public class LogViewModelTests
    {
        [Test]
        public void WhenTranslatingRelativeBytePosition_MidRangeByte_CurrentBytePositionReturned()
        {
            // Setup test objects
            LogViewModel testLogViewModel = new LogViewModel();
            
            List<Tuple<string, long>> testLogFileLocations = new List<Tuple<string, long>>
            {
                new Tuple<string, long>("log1", 103),
                new Tuple<string, long>("log2", 223),
                new Tuple<string, long>("log3", 443),
                new Tuple<string, long>("log4", 663),
                new Tuple<string, long>("log5", 883)
            };
            testLogViewModel.SetLogFileLocations(testLogFileLocations);
            long relativeByte = 800;

            // Perform test

            long specificByte = testLogViewModel.TranslateRelativeBytePosition(relativeByte);

            // Check test result
            long expectedByte = 31;
            Assert.AreEqual(expectedByte, specificByte);
        }

        [Test]
        public void WhenTranslatingRelativeBytePosition_ByteAtStartOfFileCollection_CurrentBytePositionReturned()
        {
            // Setup test objects
            LogViewModel testLogViewModel = new LogViewModel();

            List<Tuple<string, long>> testLogFileLocations = new List<Tuple<string, long>>
            {
                new Tuple<string, long>("log1", 105),
                new Tuple<string, long>("log2", 225),
                new Tuple<string, long>("log3", 445),
                new Tuple<string, long>("log4", 665),
                new Tuple<string, long>("log5", 885)
            };
            testLogViewModel.SetLogFileLocations(testLogFileLocations);
            long relativeByte = 0;

            // Perform test

            long specificByte = testLogViewModel.TranslateRelativeBytePosition(relativeByte);

            // Check test result
            long expectedByte = 0;
            Assert.AreEqual(expectedByte, specificByte);
        }

        [Test]
        public void WhenTranslatingRelativeBytePosition_ByteAtStartOfFile_CurrentBytePositionReturned()
        {
            // Setup test objects
            LogViewModel testLogViewModel = new LogViewModel();

            List<Tuple<string, long>> testLogFileLocations = new List<Tuple<string, long>>
            {
                new Tuple<string, long>("log1", 100),
                new Tuple<string, long>("log2", 220),
                new Tuple<string, long>("log3", 440),
                new Tuple<string, long>("log4", 660),
                new Tuple<string, long>("log5", 880)
            };
            testLogViewModel.SetLogFileLocations(testLogFileLocations);
            long relativeByte =320;

            // Perform test

            long specificByte = testLogViewModel.TranslateRelativeBytePosition(relativeByte);

            // Check test result
            long expectedByte = 0;
            Assert.AreEqual(expectedByte, specificByte);
        }

        [Test]
        public void WhenTranslatingRelativeBytePosition_ByteAtEndOfFile_CurrentBytePositionReturned()
        {
            // Setup test objects
            LogViewModel testLogViewModel = new LogViewModel();

            List<Tuple<string, long>> testLogFileLocations = new List<Tuple<string, long>>
            {
                new Tuple<string, long>("log1", 102),
                new Tuple<string, long>("log2", 222),
                new Tuple<string, long>("log3", 442),
                new Tuple<string, long>("log4", 662),
                new Tuple<string, long>("log5", 882)
            };
            testLogViewModel.SetLogFileLocations(testLogFileLocations);
            long relativeByte = 323;

            // Perform test

            long specificByte = testLogViewModel.TranslateRelativeBytePosition(relativeByte);

            // Check test result
            long expectedByte = 221;
            Assert.AreEqual(expectedByte, specificByte);
        }

        [Test]
        public void WhenTranslatingRelativeBytePosition_ByteAtEndOfCollection_CurrentBytePositionReturned()
        {
            // Setup test objects
            LogViewModel testLogViewModel = new LogViewModel();

            List<Tuple<string, long>> testLogFileLocations = new List<Tuple<string, long>>
            {
                new Tuple<string, long>("log1", 100),
                new Tuple<string, long>("log2", 220),
                new Tuple<string, long>("log3", 440),
                new Tuple<string, long>("log4", 660),
                new Tuple<string, long>("log5", 880)
            };
            testLogViewModel.SetLogFileLocations(testLogFileLocations);
            long relativeByte = 2299;

            // Perform test

            long specificByte = testLogViewModel.TranslateRelativeBytePosition(relativeByte);

            // Check test result
            long expectedByte = 879;
            Assert.AreEqual(expectedByte, specificByte);
        }

        [Test]
        public void WhenTranslatingRelativeBytePosition_ByteAboveTotalBytes_ReturnMaximumValue()
        {
            // Setup test objects
            LogViewModel testLogViewModel = new LogViewModel();

            List<Tuple<string, long>> testLogFileLocations = new List<Tuple<string, long>>
            {
                new Tuple<string, long>("log1", 100),
                new Tuple<string, long>("log2", 220),
                new Tuple<string, long>("log3", 440),
                new Tuple<string, long>("log4", 660),
                new Tuple<string, long>("log5", 880)
            };
            testLogViewModel.SetLogFileLocations(testLogFileLocations);
            long relativeByte = 2666;

            // Perform test

            long specificByte = testLogViewModel.TranslateRelativeBytePosition(relativeByte);

            // Check test result
            long expectedByte = 879;
            Assert.AreEqual(expectedByte, specificByte);
        }

        [Test]
        public void WhenTranslatingRelativeBytePosition_ByteBeneathZero_ReturnZeroValue()
        {
            // Setup test objects
            LogViewModel testLogViewModel = new LogViewModel();

            List<Tuple<string, long>> testLogFileLocations = new List<Tuple<string, long>>
            {
                new Tuple<string, long>("log1", 100),
                new Tuple<string, long>("log2", 220),
                new Tuple<string, long>("log3", 440),
                new Tuple<string, long>("log4", 660),
                new Tuple<string, long>("log5", 880)
            };
            testLogViewModel.SetLogFileLocations(testLogFileLocations);
            long relativeByte = -2666;

            // Perform test

            long specificByte = testLogViewModel.TranslateRelativeBytePosition(relativeByte);

            // Check test result
            long expectedByte = 0;
            Assert.AreEqual(expectedByte, specificByte);
        }

        [Test]
        public void WhenLocatingLogFileFromByteReference_MidRangeByte_ReturnCorrectFile()
        {
            // Setup test objects
            LogViewModel testLogViewModel = new LogViewModel();

            List<Tuple<string, long>> testLogFileLocations = new List<Tuple<string, long>>
            {
                new Tuple<string, long>("log1", 100),
                new Tuple<string, long>("log2", 220),
                new Tuple<string, long>("log3", 440),
                new Tuple<string, long>("log4", 660),
                new Tuple<string, long>("log5", 880)
            };
            testLogViewModel.SetLogFileLocations(testLogFileLocations);
            long relativeByte = 1430;

            // Perform test
            string logFileFromByteReference = testLogViewModel.LocateLogFileFromByteReference(relativeByte);

            // Check test result
            string expectedLogFile = "log5";
            Assert.AreEqual(expectedLogFile, logFileFromByteReference);
        }

        [Test]
        public void WhenLocatingLogFileFromByteReference_ByteAtStartOfFileCollection_ReturnCorrectFile()
        {
            // Setup test objects
            LogViewModel testLogViewModel = new LogViewModel();

            List<Tuple<string, long>> testLogFileLocations = new List<Tuple<string, long>>
            {
                new Tuple<string, long>("log1", 100),
                new Tuple<string, long>("log2", 220),
                new Tuple<string, long>("log3", 440),
                new Tuple<string, long>("log4", 660),
                new Tuple<string, long>("log5", 880)
            };
            testLogViewModel.SetLogFileLocations(testLogFileLocations);
            long relativeByte = 0;

            // Perform test
            string logFileFromByteReference = testLogViewModel.LocateLogFileFromByteReference(relativeByte);

            // Check test result
            string expectedLogFile = "log1";
            Assert.AreEqual(expectedLogFile, logFileFromByteReference);
        }

        [Test]
        public void WhenLocatingLogFileFromByteReference_ByteAtEndOfFileCollection_ReturnCorrectFile()
        {
            // Setup test objects
            LogViewModel testLogViewModel = new LogViewModel();

            List<Tuple<string, long>> testLogFileLocations = new List<Tuple<string, long>>
            {
                new Tuple<string, long>("log1", 100),
                new Tuple<string, long>("log2", 220),
                new Tuple<string, long>("log3", 440),
                new Tuple<string, long>("log4", 660),
                new Tuple<string, long>("log5", 880)
            };
            testLogViewModel.SetLogFileLocations(testLogFileLocations);
            long relativeByte = 2299;

            // Perform test
            string logFileFromByteReference = testLogViewModel.LocateLogFileFromByteReference(relativeByte);

            // Check test result
            string expectedLogFile = "log5";
            Assert.AreEqual(expectedLogFile, logFileFromByteReference);
        }

        [Test]
        public void WhenLocatingLogFileFromByteReference_ByteAtStartOfFile_ReturnCorrectFile()
        {
            // Setup test objects
            LogViewModel testLogViewModel = new LogViewModel();

            List<Tuple<string, long>> testLogFileLocations = new List<Tuple<string, long>>
            {
                new Tuple<string, long>("log1", 100),
                new Tuple<string, long>("log2", 220),
                new Tuple<string, long>("log3", 440),
                new Tuple<string, long>("log4", 660),
                new Tuple<string, long>("log5", 880)
            };
            testLogViewModel.SetLogFileLocations(testLogFileLocations);
            long relativeByte = 100;

            // Perform test
            string logFileFromByteReference = testLogViewModel.LocateLogFileFromByteReference(relativeByte);

            // Check test result
            string expectedLogFile = "log2";
            Assert.AreEqual(expectedLogFile, logFileFromByteReference);
        }

        [Test]
        public void WhenLocatingLogFileFromByteReference_ByteAtEndOfFile_ReturnCorrectFile()
        {
            // Setup test objects
            LogViewModel testLogViewModel = new LogViewModel();

            List<Tuple<string, long>> testLogFileLocations = new List<Tuple<string, long>>
            {
                new Tuple<string, long>("log1", 100),
                new Tuple<string, long>("log2", 220),
                new Tuple<string, long>("log3", 440),
                new Tuple<string, long>("log4", 660),
                new Tuple<string, long>("log5", 880)
            };
            testLogViewModel.SetLogFileLocations(testLogFileLocations);
            long relativeByte = 99;

            // Perform test
            string logFileFromByteReference = testLogViewModel.LocateLogFileFromByteReference(relativeByte);

            // Check test result
            string expectedLogFile = "log1";
            Assert.AreEqual(expectedLogFile, logFileFromByteReference);
        }

        [Test]
        public void WhenLocatingLogFileFromByteReference_ByteBeneathZero_ReturnFirstFile()
        {
            // Setup test objects
            LogViewModel testLogViewModel = new LogViewModel();

            List<Tuple<string, long>> testLogFileLocations = new List<Tuple<string, long>>
            {
                new Tuple<string, long>("log1", 100),
                new Tuple<string, long>("log2", 220),
                new Tuple<string, long>("log3", 440),
                new Tuple<string, long>("log4", 660),
                new Tuple<string, long>("log5", 880)
            };
            testLogViewModel.SetLogFileLocations(testLogFileLocations);
            long relativeByte = -99;

            // Perform test
            string logFileFromByteReference = testLogViewModel.LocateLogFileFromByteReference(relativeByte);

            // Check test result
            string expectedLogFile = "log1";
            Assert.AreEqual(expectedLogFile, logFileFromByteReference);
        }

        [Test]
        public void WhenLocatingLogFileFromByteReference_ByteAboveTotalBytes_ReturnLastFile()
        {
            // Setup test objects
            LogViewModel testLogViewModel = new LogViewModel();

            List<Tuple<string, long>> testLogFileLocations = new List<Tuple<string, long>>
            {
                new Tuple<string, long>("log1", 100),
                new Tuple<string, long>("log2", 220),
                new Tuple<string, long>("log3", 440),
                new Tuple<string, long>("log4", 660),
                new Tuple<string, long>("log5", 880)
            };
            testLogViewModel.SetLogFileLocations(testLogFileLocations);
            long relativeByte = 6600;

            // Perform test
            string logFileFromByteReference = testLogViewModel.LocateLogFileFromByteReference(relativeByte);

            // Check test result
            string expectedLogFile = "log5";
            Assert.AreEqual(expectedLogFile, logFileFromByteReference);
        }

        [Test]
        public void WhenCreatingRelativeByteReference_MidRangeByte_ReturnCorrectFile()
        {
            // Setup test objects
            LogViewModel testLogViewModel = new LogViewModel();

            List<Tuple<string, long>> testLogFileLocations = new List<Tuple<string, long>>
            {
                new Tuple<string, long>("log1", 100),
                new Tuple<string, long>("log2", 220),
                new Tuple<string, long>("log3", 440),
                new Tuple<string, long>("log4", 660),
                new Tuple<string, long>("log5", 880)
            };
            testLogViewModel.SetLogFileLocations(testLogFileLocations);

            long byteWithinFile = 5;
            string logFileName = "log3";

            // Perform test
            long relativeByteReference = testLogViewModel.CreateRelativeByteReference(byteWithinFile, logFileName);

            // Check test result
            long expectedByteReference = 325;
            Assert.AreEqual(expectedByteReference, relativeByteReference);
        }

        [Test]
        public void WhenCreatingRelativeByteReference_ByteAtEndOfFile_ReturnCorrectFile()
        {
            // Setup test objects
            LogViewModel testLogViewModel = new LogViewModel();

            List<Tuple<string, long>> testLogFileLocations = new List<Tuple<string, long>>
            {
                new Tuple<string, long>("log1", 100),
                new Tuple<string, long>("log2", 220),
                new Tuple<string, long>("log3", 440),
                new Tuple<string, long>("log4", 660),
                new Tuple<string, long>("log5", 880)
            };
            testLogViewModel.SetLogFileLocations(testLogFileLocations);

            long byteWithinFile = 220;
            string logFileName = "log2";

            // Perform test
            long relativeByteReference = testLogViewModel.CreateRelativeByteReference(byteWithinFile, logFileName);

            // Check test result
            long expectedByteReference = 320;
            Assert.AreEqual(expectedByteReference, relativeByteReference);
        }

        [Test]
        public void WhenCreatingRelativeByteReference_ByteAtStartOfFile_ReturnCorrectFile()
        {
            // Setup test objects
            LogViewModel testLogViewModel = new LogViewModel();

            List<Tuple<string, long>> testLogFileLocations = new List<Tuple<string, long>>
            {
                new Tuple<string, long>("log1", 100),
                new Tuple<string, long>("log2", 220),
                new Tuple<string, long>("log3", 440),
                new Tuple<string, long>("log4", 660),
                new Tuple<string, long>("log5", 880)
            };
            testLogViewModel.SetLogFileLocations(testLogFileLocations);

            long byteWithinFile = 0;
            string logFileName = "log4";

            // Perform test
            long relativeByteReference = testLogViewModel.CreateRelativeByteReference(byteWithinFile, logFileName);

            // Check test result
            long expectedByteReference = 760;
            Assert.AreEqual(expectedByteReference, relativeByteReference);
        }

        [Test]
        public void WhenCreatingRelativeByteReference_ByteAboveTotalBytesOfFile_ReturnCorrectFile()
        {
            // Setup test objects
            LogViewModel testLogViewModel = new LogViewModel();

            List<Tuple<string, long>> testLogFileLocations = new List<Tuple<string, long>>
            {
                new Tuple<string, long>("log1", 100),
                new Tuple<string, long>("log2", 220),
                new Tuple<string, long>("log3", 440),
                new Tuple<string, long>("log4", 660),
                new Tuple<string, long>("log5", 880)
            };
            testLogViewModel.SetLogFileLocations(testLogFileLocations);

            long byteWithinFile = 455;
            string logFileName = "log3";

            // Perform test
            long relativeByteReference = testLogViewModel.CreateRelativeByteReference(byteWithinFile, logFileName);

            // Check test result
            long expectedByteReference = 760;
            Assert.AreEqual(expectedByteReference, relativeByteReference);
        }

        [Test]
        public void WhenCreatingRelativeByteReference_ByteBeneathZero_ReturnCorrectFile()
        {
            // Setup test objects
            LogViewModel testLogViewModel = new LogViewModel();

            List<Tuple<string, long>> testLogFileLocations = new List<Tuple<string, long>>
            {
                new Tuple<string, long>("log1", 100),
                new Tuple<string, long>("log2", 220),
                new Tuple<string, long>("log3", 440),
                new Tuple<string, long>("log4", 660),
                new Tuple<string, long>("log5", 880)
            };
            testLogViewModel.SetLogFileLocations(testLogFileLocations);

            long byteWithinFile = -455;
            string logFileName = "log3";

            // Perform test
            long relativeByteReference = testLogViewModel.CreateRelativeByteReference(byteWithinFile, logFileName);

            // Check test result
            long expectedByteReference = 320;
            Assert.AreEqual(expectedByteReference, relativeByteReference);
        }
    }
}