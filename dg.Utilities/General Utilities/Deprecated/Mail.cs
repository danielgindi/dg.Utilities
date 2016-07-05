using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Mail;
using System.Net.Mime;
using System.IO;

namespace dg.Utilities
{
    public static class Mail
    {
        [Obsolete]
        public class AttachmentEx : MailHelper.AttachmentEx
        {
            public AttachmentEx(string fileName)
                : base(fileName)
            {
            }

            public AttachmentEx(string fileName, string url)
                : base(fileName, url)
            {
            }

            public AttachmentEx(Stream contentStream, ContentType contentType)
                : base(contentStream, contentType)
            {
            }

            public AttachmentEx(Stream contentStream, string name)
                : base(contentStream, name)
            {
            }

            public AttachmentEx(string fileName, ContentType contentType)
                : base(fileName, contentType)
            {
            }

            public AttachmentEx(string fileName, string url, string mediaType)
                : base(fileName, url, mediaType)
            {
            }

            public AttachmentEx(Stream contentStream, string name, string mediaType)
                : base(contentStream, name, mediaType)
            {
            }
        }

        [Obsolete]
        public static void AddAddress(string addresses, MailAddressCollection coll)
        {
            MailHelper.AddAddress(addresses, coll);
        }
    }
}
