using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib;

namespace gdocerous.Code
{
    public class Posterous : IDisposable
    {
        private Stream m_zipfile;
        private string m_html;
        private Dictionary<string, Stream> m_attachments;
        private string m_documenttitle;

        public Posterous(Stream htmlzipfile, string documenttitle)
        {
            m_documenttitle = documenttitle;
            m_zipfile = htmlzipfile;
            m_attachments = new Dictionary<string, Stream>();

            ExtractZipFile();
        }

        public void Send(string tags)
        {
            using (MailMessage msg = new MailMessage())
            {
                foreach (KeyValuePair<string, Stream> attachment in m_attachments)
                {
                    msg.Attachments.Add(new Attachment(attachment.Value, attachment.Key));
                }

                msg.From = new MailAddress("overeemm@gmail.com", "gmail");
                msg.To.Add(new MailAddress("draft@posterous.com"));
                msg.Subject = string.Format("{0} ((tag: {1}))", m_documenttitle, tags);

                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(m_html, new System.Net.Mime.ContentType("text/html"));

                foreach (KeyValuePair<string, Stream> attachment in m_attachments)
                {
                    LinkedResource linkedRes = new LinkedResource(attachment.Value, "");
                    linkedRes.ContentId = attachment.Key;
                    htmlView.LinkedResources.Add(linkedRes);
                }

                msg.AlternateViews.Add(htmlView);
                
                var smtp = new SmtpClient
                       {
                           Host = "smtp.gmail.com",
                           Port = 587,
                           EnableSsl = true,
                           DeliveryMethod = SmtpDeliveryMethod.Network,
                           UseDefaultCredentials = false,
                           Credentials = new System.Net.NetworkCredential("overeemm@gmail.com", "38Z!burg")
                       };

                smtp.Send(msg);
            }
        }

        private void ExtractZipFile()
        {
            using (ZipInputStream zipInputStream = new ZipInputStream(m_zipfile))
            {
                ZipEntry zipEntry = zipInputStream.GetNextEntry();
                while (zipEntry != null)
                {
                    String entryFileName = zipEntry.Name;
                    if (entryFileName.ToLowerInvariant().EndsWith(".html"))
                    {
                        ExtractHtml(zipInputStream);
                    }
                    else
                    {
                        byte[] buffer = new byte[4096];
                        MemoryStream mstream = new MemoryStream();
                        StreamUtils.Copy(zipInputStream, mstream, buffer);
                        m_attachments.Add(entryFileName, mstream);
                    }
                    zipEntry = zipInputStream.GetNextEntry();
                }
            }

            if (string.IsNullOrEmpty(m_html))
                throw new InvalidOperationException("No HTML found to post to your posterous.");
        }

        private void ExtractHtml(ZipInputStream zipInputStream)
        {
            byte[] buffer = new byte[4096];
            using (MemoryStream mstream = new MemoryStream())
            using (StreamReader reader = new StreamReader(mstream))
            {
                StreamUtils.Copy(zipInputStream, mstream, buffer);
                mstream.Position = 0;
                m_html = reader.ReadToEnd();
            }
        }

        public void Dispose()
        {
            if (m_zipfile != null)
            {
                m_zipfile.Dispose();
                m_zipfile = null;
            }
            foreach (Stream str in m_attachments.Values)
                str.Dispose();
            m_attachments.Clear();
        }
    }
}