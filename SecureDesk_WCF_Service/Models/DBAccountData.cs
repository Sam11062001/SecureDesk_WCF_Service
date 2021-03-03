using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Google.Cloud.Firestore;
namespace SecureDesk_WCF_Service.Models
{

    [FirestoreData]
    public class DBAccountData
    {
        [FirestoreProperty]
        public string accountName { get; set; }
       
        [FirestoreProperty]
        public string accountUserName { get; set; }

        [FirestoreProperty]
        public string accountPassword { get; set; }

        
    }
}