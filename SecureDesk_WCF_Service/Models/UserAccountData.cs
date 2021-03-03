using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
namespace SecureDesk_WCF_Service.Models
{
    //this class is specific to the account module of the project
    [DataContract]
    public class UserAccountData
    {
        //Private property for the classes

        /*
         * Parameter<accountName> : account Name represents the website name 
         * Parameter<accountUserName> :accountUserName represents the username on the specific website
         * Paramter<accountPassword> :accountPassword represents the password for the specific website
         *Parameter<SeccureDeskUser> : represent the valid user of the secure desk software
         */

        private string accountName;

        private string accountUserName;

        private string accountPassword;

        private string SecureDeskUser;

        [DataMember]
        public string Name
        {
            get { return accountName; }
            set { this.accountName = value; }
        }

        [DataMember]
        public string UserName
        {
            get { return accountUserName; }
            set { this.accountUserName = value; }

        }

        [DataMember]
        public string Password
        {
            get { return accountPassword; }
            set { this.accountPassword = value; }
        }

        [DataMember]
        public string userEmail
        {
            get { return SecureDeskUser; }
            set { this.SecureDeskUser = value; }
        }
    }
}