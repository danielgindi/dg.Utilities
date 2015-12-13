﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace dg.Utilities.CSV
{
    public class CsvReader : IDisposable
    {
        private StreamReader StreamReader = null;
        private Stream _Stream = null;
        private bool _MultilineSupport = true;

        public CsvReader(Stream inputStream)
        {
            _Stream = inputStream;
        }

        public CsvReader(Stream inputStream, bool multilineSupport)
        {
            _Stream = inputStream;
            _MultilineSupport = multilineSupport;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (StreamReader != null)
                {
                    StreamReader.Dispose();
                }
                StreamReader = null;
                if (_Stream != null)
                {
                    _Stream.Dispose();
                }
                _Stream = null;
            }
            // Now clean up Native Resources (Pointers)
        }

        public Stream Stream
        {
            get { return _Stream; }
        }

        public bool MultilineSupport
        {
            get { return _MultilineSupport; }
            set { _MultilineSupport = value; }
        }

        /// <summary>
        /// Will read the next row
        /// </summary>
        /// <returns>Array of columns if successful and there's something to read. Otherwise <value>null</value></returns>
        public string[] ReadRow()
        {
            if (null == _Stream) return null;
            lock (this)
            {
                if (null == StreamReader)
                {
                    StreamReader = new StreamReader(_Stream, Encoding.Default, true);
                }

                if (StreamReader.EndOfStream)
                {
                    return null;
                }

                List<string> columns = new List<string>();
                StringBuilder sbColumn = new StringBuilder();

                if (_MultilineSupport)
                {
                    bool isQuoted = false;

                    while (!StreamReader.EndOfStream)
                    {
                        char c = (char)StreamReader.Read();

                        if (isQuoted)
                        {
                            if (c == '"')
                            {
                                if (!StreamReader.EndOfStream && (char)StreamReader.Peek() == '"')
                                {
                                    sbColumn.Append('"');
                                    StreamReader.Read(); // Skip next quote mark (")
                                }
                                else
                                {
                                    isQuoted = false;
                                }
                            }
                            else
                            {
                                sbColumn.Append(c);
                            }
                        }
                        else
                        {
                            if (c == ',')
                            {
                                columns.Add(sbColumn.ToString());
                                sbColumn.Clear();
                            }
                            else if (c == '"')
                            {
                                if (sbColumn.Length > 0)
                                {
                                    sbColumn.Append(c);
                                }
                                else
                                {
                                    isQuoted = true;
                                }
                            }
                            else if (c == '\n')
                            {
                                break;
                            }
                            else
                            {
                                sbColumn.Append(c);
                            }
                        }
                    }
                }
                else
                {
                    string line;
                    char c;
                    bool isQuoted;

                    line = StreamReader.ReadLine();
                    isQuoted = false;
                    for (int j = 0, len = line.Length; j < len; j++)
                    {
                        c = line[j];
                        if (isQuoted)
                        {
                            if (c == '"')
                            {
                                if (line.Length > j + 1 && line[j + 1] == '"')
                                {
                                    sbColumn.Append('"');
                                    j++;
                                }
                                else
                                {
                                    isQuoted = false;
                                }
                            }
                            else
                            {
                                sbColumn.Append(c);
                            }
                        }
                        else
                        {
                            if (c == ',')
                            {
                                columns.Add(sbColumn.ToString());
                                sbColumn.Clear();
                            }
                            else if (c == '"')
                            {
                                if (sbColumn.Length > 0)
                                {
                                    sbColumn.Append(c);
                                }
                                else
                                {
                                    isQuoted = true;
                                }
                            }
                            else
                            {
                                sbColumn.Append(c);
                            }
                        }
                    }
                }

                columns.Add(sbColumn.ToString());

                return columns.ToArray();
            }
        }
    }
}
