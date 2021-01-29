using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;

namespace SecureDesk_WCF_Service.Models
{
    public class userKeys
    {
        private string email_address;
        private string userSecretKey;

        [MessageHeader(Name = "User_Email_Address")]
        public string Email_Address
        {
            get;
            set;
        }

        [MessageHeader(Name = "User_Secret_Key")]
        public string UserSecretKey
        {
            get;
            set;
        }
    }
}