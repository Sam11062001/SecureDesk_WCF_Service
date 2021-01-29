using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Google.Cloud.Firestore;

namespace SecureDesk_WCF_Service.Models
{
    //Will be stored to database directly 
    [FirestoreData]
    public class User
    {
        [FirestoreProperty]
        public string email { get; set; }

        [FirestoreProperty]
        public string firstName { get; set; }

        [FirestoreProperty]
        public string lastName { get; set; }

        [FirestoreProperty]
        public string dateOfBirth { get; set; }

        [FirestoreProperty]
        public string password { get; set; }

        [FirestoreProperty]
        public int questionSelected { get; set; }

        [FirestoreProperty]
        public string questionAnswered { get; set; }

        [FirestoreProperty]
        public Boolean verified { get; set; }

        [FirestoreProperty]
        public int securePin { get; set; }

        [FirestoreProperty]
        public string salt { get; set; }
      


    }
}