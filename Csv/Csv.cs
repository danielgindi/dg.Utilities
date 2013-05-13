using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace dg.Utilities.CSV
{
    public class CsvReader : IDisposable
    {
        public CsvReader(Stream InputStream)
        {
            _Stream = InputStream;
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
                if (StreamReader != null) StreamReader.Dispose();
                StreamReader = null;
                if (_Stream != null) _Stream.Dispose();
                _Stream = null;
            }
            // Now clean up Native Resources (Pointers)
        }

        Stream _Stream = null;
        public Stream Stream
        {
            get { return _Stream; }
        }

        StreamReader StreamReader = null;

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
                    StreamReader = new StreamReader(_Stream, Encoding.UTF8, true);
                }

                List<string> columns = new List<string>();
                string line, column;
                char c;
                bool isQuoted;
                if (!StreamReader.EndOfStream)
                {
                    line = StreamReader.ReadLine();
                    column = "";
                    columns.Clear();
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
                                    column += '"';
                                    j++;
                                }
                                else
                                {
                                    isQuoted = false;
                                }
                            }
                            else
                            {
                                column += c;
                            }
                        }
                        else
                        {
                            if (c == ',')
                            {
                                columns.Add(column);
                                column = "";
                            }
                            else if (c == '"')
                            {
                                if (column.Length > 0)
                                {
                                    column += c;
                                }
                                else
                                {
                                    isQuoted = true;
                                }
                            }
                            else
                            {
                                column += c;
                            }
                        }
                    }
                    columns.Add(column);
                    return columns.ToArray();
                }
                return null;
            }
        }
    }
}
