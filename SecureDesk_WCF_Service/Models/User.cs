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
        //Property 1
        [FirestoreProperty]
        public string email { get; set; }

        //Property 2
        [FirestoreProperty]
        public string firstName { get; set; }

        //Property 3
        [FirestoreProperty]
        public string lastName { get; set; }

        //Property 4
        [FirestoreProperty]
        public string dateOfBirth { get; set; }

        //Property 5
        [FirestoreProperty]
        public string password { get; set; }

        //Property 6
        [FirestoreProperty]
        public int questionSelected { get; set; }

        [FirestoreProperty]
        public string questionAnswered { get; set; }

        [FirestoreProperty]
        public Boolean verified { get; set; }

        [FirestoreProperty]
        public string salt { get; set; }

        [FirestoreProperty]
        public string accessPIN { get; set; }

        [FirestoreProperty]
        public string dateOfRegistration { get; set; }


      


    }
}