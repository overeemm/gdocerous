using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using System.Net;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Json;

namespace gdocerous.Code
{
    public static class HttpSessionStateExtensions
    {
        public static GoogleDocsRepository GetGoogleDocsRepos(this HttpSessionState session)
        {
            return session["repos"] as GoogleDocsRepository;
        }

        public static GoogleDocsRepository GetGoogleDocsRepos(this HttpSessionStateBase session)
        {
            return session["repos"] as GoogleDocsRepository;
        }

        public static void SetGoogleDocsRepos(this HttpSessionStateBase session, GoogleDocsRepository repos)
        {
            session["repos"] = repos;
        }

        public static bool IsValidSession(this HttpSessionStateBase session)
        {
            return session.GetGoogleDocsRepos() != null;
        }
    }

    public static class GoogleDocsRepositoryExtensions
    {
        public static bool IsDeveloperAccount(this GoogleDocsRepository repos)
        {
            return repos != null && repos.Email == "overeemm@gmail.com";
        }
        
    }

    public static class StringUrlExtensions
    {
        public static string Post(this string url, string contents)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "POST";

            byte[] postcontentsArray = Encoding.UTF8.GetBytes(contents);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postcontentsArray.Length;

            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(postcontentsArray, 0, postcontentsArray.Length);
                requestStream.Close();

                WebResponse response = request.GetResponse();

                using (Stream responseStream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    string responseFromServer = reader.ReadToEnd();
                    reader.Close();
                    responseStream.Close();
                    response.Close();

                    return responseFromServer;
                }
            }
        }

        public static GoogleOAuthSession ExtractToken(this string tokenjson)
        {
            using (MemoryStream memstream = new MemoryStream(Encoding.Unicode.GetBytes(tokenjson)))
            {
                var serializer = new DataContractJsonSerializer(typeof(GoogleOAuthSession));
                GoogleOAuthSession returnobj = serializer.ReadObject(memstream) as GoogleOAuthSession;
                memstream.Close();
                return returnobj;
            }
        }
    }
}