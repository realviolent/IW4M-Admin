using SharedLibraryCore;
using SharedLibraryCore.Interfaces;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace IW4MAdmin.Application.IO
{
    class GameLogReader : IGameLogReader
    {
        private readonly IEventParser _parser;
        private readonly string _logFile;
        private readonly ILogger _logger;

        public long Length => new FileInfo(_logFile).Length;

        public int UpdateInterval => 300;

        public GameLogReader(string logFile, IEventParser parser, ILogger<GameLogReader> logger)
        {
            _logFile = logFile;
            _parser = parser;
            _logger = logger;
        }

        public async Task<IEnumerable<GameEvent>> ReadEventsFromLog(long fileSizeDiff, long startPosition, Server server = null)
        {
            // allocate the bytes for the new log lines
            List<string> logLines = new List<string>();

            // open the file as a stream
            using (FileStream fs = new FileStream(_logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var length = (int)fileSizeDiff;
                var byteBuff = ArrayPool<byte>.Shared.Rent(length);

                try
                {
                    fs.Seek(startPosition, SeekOrigin.Begin);
                    var bytesRead = await fs.ReadAsync(byteBuff, 0, length);

                    var charCount = Utilities.EncodingType.GetCharCount(byteBuff, 0, bytesRead);
                    var charBuff = ArrayPool<char>.Shared.Rent(charCount);

                    try
                    {
                        var charsWritten = Utilities.EncodingType.GetChars(byteBuff, 0, bytesRead, charBuff, 0);
                        var stringBuilder = new StringBuilder();

                        for (var i = 0; i < charsWritten; i++)
                        {
                            var c = charBuff[i];
                            if (c == '\n')
                            {
                                logLines.Add(stringBuilder.ToString());
                                stringBuilder.Clear();
                            }

                            else if (c != '\r')
                            {
                                stringBuilder.Append(c);
                            }
                        }

                        if (stringBuilder.Length > 0)
                        {
                            logLines.Add(stringBuilder.ToString());
                        }
                    }
                    finally
                    {
                        ArrayPool<char>.Shared.Return(charBuff);
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(byteBuff);
                }
            }

            List<GameEvent> events = new List<GameEvent>();

            // parse each line
            foreach (string eventLine in logLines.Where(_line => _line.Length > 0))
            {
                try
                {
                    var gameEvent = _parser.GenerateGameEvent(eventLine);
                    events.Add(gameEvent);
                }

                catch (Exception e)
                {
                    _logger.LogError(e, "Could not properly parse event line {@eventLine}", eventLine);
                }
            }

            return events;
        }
    }
}
