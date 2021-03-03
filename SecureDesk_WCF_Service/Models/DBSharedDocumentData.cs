using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SecureDesk_WCF_Service.Models
{
    [FirestoreData]
    public class DBSharedDocumentData
    {
        [FirestoreProperty]
        public string fileName { get; set; }

        [FirestoreProperty]
        public string fileLink { get; set; }

        [FirestoreProperty]
        public string sharedBy { get; set; }
    }
}