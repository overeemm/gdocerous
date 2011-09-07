using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace gdocerous.Code
{
    public class GoogleOAuthSession
    {
        public string access_token;
        public string expires_in;
        public string token_type;
        public string refresh_token;

        private DateTime _created;

        public GoogleOAuthSession()
        {
            _created = DateTime.Now;
        }

        // "access_token":"1/fFAGRNJru1FTz70BzhT3Zg",
        // "expires_in":3920,
        // "token_type":"Bearer",
        // "refresh_token":"1/6BMfW9j53gdGImsixUH6kU5RsR4zwI9lUVX-tqf8JXQ"

        public bool IsExpired()
        {
            //return false;
            int expiressecs;
            if (!int.TryParse(expires_in, out expiressecs))
                expiressecs = 3600;

            // MO; als het token expired is, dan is de rechterkant lager dan de linkerkant.
            // dus het verschil is dan positief
            return (DateTime.Now - _created.AddSeconds(expiressecs)).TotalMinutes >= -5;
        }
    }
}