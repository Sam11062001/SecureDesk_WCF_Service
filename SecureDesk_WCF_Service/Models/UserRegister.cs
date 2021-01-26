using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace SecureDesk_WCF_Service.Models
{
    [DataContract(Name = "User Register")]
    //Object Will be obtianed from the registration form
    public class UserRegister
    {
        private string email_address;
        private string first_name;
        private string last_name;
        private string dob;
        private string password;
        private int question_number;
        private string question_answered;

        [DataMember]
        public string Email_Address
        {
            get;
            set;
        }

        [DataMember]
        public string First_Name
        {
            get;
            set;
        }

        [DataMember]
        public string Last_Name
        {
            get;
            set;
        }

        [DataMember]
        public string Date_Of_Birth
        {
            get;
            set;
        }

        [DataMember]
        public int Question_Number_Selected
        {
            get;
            set;
        }

        [DataMember]
        public string Question_Answered
        {
            get;
            set;
        }

        [DataMember]
        public string Password
        {
            get;
            set;
        }

    }
}