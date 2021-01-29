using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using System.Runtime.Serialization;
namespace SecureDesk_WCF_Service.Models
{
    [DataContract(Name ="User Credentials")]
    public class AuthUser
    {
        private string email;
        private string password;

        [DataMember]
        public string User_Auth_Email
        {
            get { return email; }
            set { email = value; }
        }

        [DataMember]
        public string User_Auth_Password
        {
            get { return password; }
            set { password = value; }
        }
    }
}