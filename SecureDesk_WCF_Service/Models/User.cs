using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SecureDesk_WCF_Service.Models
{
    //Will be stored to database directly 
    public class User
    {
        public string EmailAddress { get; set; }

        public string firstName { get; set; }

        public string lastName { get; set; }

        public string dateOfBirth { get; set; }

        public string password { get; set; }

        public int questionSelected { get; set; }

        public string questionAnswered { get; set; }

        public Boolean verified { get; set; }

        public int securePin { get; set; }

        public string salt { get; set; }
      


    }
}