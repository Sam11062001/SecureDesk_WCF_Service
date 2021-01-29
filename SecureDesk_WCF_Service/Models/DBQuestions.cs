using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Google.Cloud.Firestore;

namespace SecureDesk_WCF_Service.Models
{
    [FirestoreData]
    public class DBQuestions
    {
        [FirestoreProperty]
        public int id { get; set; }

        [FirestoreProperty]
        public string question { get; set; }
    }
}