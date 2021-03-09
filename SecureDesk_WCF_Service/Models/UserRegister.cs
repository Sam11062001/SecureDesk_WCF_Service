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
            get { return email_address; }
            set { email_address = value; }
        }
       
        [DataMember]
        public string First_Name
        {
            get { return first_name; }
            set { first_name = value; }
        }

        [DataMember]
        public string Last_Name
        {
            get { return last_name; }
            set { last_name = value; }
        }

        [DataMember]
        public string Date_Of_Birth
        {
            get { return dob; }
            set { dob = value; }
        }

        [DataMember]
        public int Question_Number_Selected
        {
            get { return question_number; }
            set { question_number = value; }
        }

        [DataMember]
        public string Question_Answered
        {
            get { return question_answered; }
            set { question_answered = value; }
        }

        [DataMember]
        public string Password
        {
            get { return password; }
            set { password = value; }
        }

    }
}