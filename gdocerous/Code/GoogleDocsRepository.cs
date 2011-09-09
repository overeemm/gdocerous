using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using Google.GData.Documents;
using Google.GData.Client;
using Google.Documents;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib;
using System.Text;
using System.Runtime.Serialization.Json;
using MvcMiniProfiler;

namespace gdocerous.Code
{
    public class GoogleDocsRepository
    {
        private const string clientid = "100115939100.apps.googleusercontent.com";
        private const string redirect_url = "https://www.gdocerous.com/AuthCallback";
        private const string clientsecret = "XJlCphttO8pYT5Szliuh9wcH";

        private GoogleOAuthSession m_token;
        private string m_email;

        public GoogleDocsRepository(string code)
        {
            m_token = GetSessionToken(code);

            if (string.IsNullOrEmpty(m_email))
            {
                using (MiniProfiler.Current.Step("Get gmail address"))
                {
                    DocumentsRequest request = new DocumentsRequest(new RequestSettings(MvcApplication.APPNAME, m_token.access_token));
                    m_email = request.GetFolders().AtomFeed.Authors[0].Email;
                }
            }
        }

        public GoogleDocs GetGoogleDocs()
        {
            if (m_token.IsExpired())
                m_token = RefreshToken(m_token);

            GoogleDocs docs = new GoogleDocs(m_token, m_email);
            return docs;
        }

        public string Email { get { return m_email; } }

        public string OAuthToken { get { return m_token.access_token; } }

        private GoogleOAuthSession Token { get { return m_token; } }

        public static string GetAuthUrl()
        {
            return string.Format("https://accounts.google.com/o/oauth2/auth?client_id={0}&redirect_uri={1}&scope={2}&response_type=code"
                                , System.Web.HttpUtility.UrlEncode(clientid)
                                , System.Web.HttpUtility.UrlEncode(redirect_url)
                                , System.Web.HttpUtility.UrlEncode("https://docs.google.com/feeds/"));// https://mail.google.com/"));
        }

        private static GoogleOAuthSession GetSessionToken(string code)
        {
            using (MiniProfiler.Current.Step("GoogleDocsRepository.GetSessionToken"))
            {
                return "https://accounts.google.com/o/oauth2/token"
                        .Post(string.Format("code={0}&client_id={1}&client_secret={2}&redirect_uri={3}&grant_type=authorization_code"
                                            , System.Web.HttpUtility.UrlEncode(code)
                                            , System.Web.HttpUtility.UrlEncode(clientid)
                                            , System.Web.HttpUtility.UrlEncode(clientsecret)
                                            , System.Web.HttpUtility.UrlEncode(redirect_url)))
                        .ExtractToken();
            }
        }

        private static GoogleOAuthSession RefreshToken(GoogleOAuthSession code)
        {
            using (MiniProfiler.Current.Step("GoogleDocsRepository.RefreshToken"))
            {
                return "https://accounts.google.com/o/oauth2/token"
                        .Post(string.Format("client_id={1}&client_secret={2}&refresh_token={0}&grant_type=refresh_token"
                                            , System.Web.HttpUtility.UrlEncode(code.refresh_token)
                                            , System.Web.HttpUtility.UrlEncode(clientid)
                                            , System.Web.HttpUtility.UrlEncode(clientsecret)))
                        .ExtractToken();
            }
        }

        public bool AllowedAccount()
        {
            return true;
            //DocumentsRequest request = new DocumentsRequest(new RequestSettings(MvcApplication.APPNAME, m_token.access_token));
            //return request.GetFolders().AtomFeed.Authors[0].Email == "overeemm@gmail.com";
        }
    }
}