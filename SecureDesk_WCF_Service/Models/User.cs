using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SecureDesk_WCF_Service.Models
{
    //Will be stored to database directly 
    public class User
    {
        public string EmailAddress;

        public string firstName;

        public string lastName;

        public string dateOfBirth;

        public string password;

        public int questionSelected;

        public string questionAnswered;

        public Boolean verified; 


      


    }
}