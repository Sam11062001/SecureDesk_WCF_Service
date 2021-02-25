using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Google.Cloud.Firestore;


namespace SecureDesk_WCF_Service.Models
{
    [FirestoreData]
    public class DBDiaryData
    {
        [FirestoreProperty]
        public string date { get; set; }

        [FirestoreProperty]
        public string title { get; set; }

        [FirestoreProperty]
        public string fileLink { get; set; }
    }
}