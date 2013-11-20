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
        public class AttachmentEx : Attachment
        {
            private string _attachmentFilePath = null;
            private string _attachmentFileUrl = null;

            public AttachmentEx(string fileName)
                : base(fileName)
            {
                AttachmentFilePath = fileName;
            }
            public AttachmentEx(string fileName, string url)
                : base(fileName)
            {
                AttachmentFilePath = fileName;
                AttachmentFileUrl = url;
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
                AttachmentFilePath = fileName;
            }
            public AttachmentEx(string fileName, string url, string mediaType)
                : base(fileName, mediaType)
            {
                AttachmentFilePath = fileName;
                AttachmentFileUrl = url;
            }
            public AttachmentEx(Stream contentStream, string name, string mediaType)
                : base(contentStream, name, mediaType)
            {
            }

            public string AttachmentFilePath
            {
                get { return _attachmentFilePath; }
                set { _attachmentFilePath = value; }
            }

            public string AttachmentFileUrl
            {
                get { return _attachmentFileUrl; }
                set { _attachmentFileUrl = value; }
            }
        }

        public static void AddAddress(string addresses, MailAddressCollection coll)
        {
            string[] toList = addresses.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            string addr;
            foreach (string address in toList)
            {
                addr = address.Trim();
                if (addr != null && addr.Length > 0) coll.Add(new MailAddress(addr));
            }
        }
    }
}
