using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Xml;

namespace dg.Utilities.Serialization
{
    public static class XmlSerializerHelper
    {
        #region Methods

        #region Public

        /// <summary>
        /// Serializes the object.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static string SerializeObject(object obj, Type type)
        {
            string xml = string.Empty;
            XmlSerializer xs = new XmlSerializer(type);
            StreamReader streamReader = null; // do not use Using for this - Bug with ASP.NET 4.0 causes Disposing of the memory stream while still trying to access it
            try
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    XmlWriterSettings xmlWriterSettings = new XmlWriterSettings { CheckCharacters = false };
                    using (XmlWriter xmlWriter = XmlWriter.Create(memoryStream, xmlWriterSettings))
                    {
                        xs.Serialize(xmlWriter, obj, null);
                        memoryStream.Position = 0;
                        streamReader = new StreamReader(memoryStream);
                        xml = streamReader.ReadToEnd();
                    }
                }
            }
            finally
            {
                if (streamReader != null)
                {
                    streamReader.Close();
                    streamReader.Dispose();
                }
            }
            return xml;
        }

        /// <summary>
        /// Deserializes the object.
        /// </summary>
        /// <param name="xml">The XML.</param>
        /// <param name="typeName">Name of the type.</param>
        /// <returns></returns>
        public static object DeserializeObject(string xml, string typeName, bool isStrict)
        {
            object obj = null;
            XmlSerializer xs = null;
            try
            {
                xs = new XmlSerializer(Type.GetType(typeName));
            }
            catch (System.Exception exFirst)
            {
                if (isStrict) throw exFirst;
                try
                {
                    int iStrip = typeName.LastIndexOf(@", Version=");
                    typeName = typeName.Remove(iStrip);
                    xs = new XmlSerializer(Type.GetType(typeName));
                }
                catch
                {
                    if (isStrict) throw exFirst;
                }
            }
            if (xs != null)
            {
                using (StringReader stringReader = new StringReader(xml))
                {
                    var xmlReaderSettings = new XmlReaderSettings { CheckCharacters = false };
                    using (XmlReader xmlReader = XmlReader.Create(stringReader, xmlReaderSettings))
                    {
                        obj = xs.Deserialize(xmlReader);
                        stringReader.Close();
                    }
                }
            }
            return obj;
        }

        #endregion

        #endregion
    }
}
