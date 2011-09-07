using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;

namespace gdocerous.Code
{
    public static class Extensions
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

        public static bool IsDeveloperAccount(this GoogleDocsRepository repos)
        {
            return repos != null && repos.Email == "overeemm@gmail.com";
        }
    }
}