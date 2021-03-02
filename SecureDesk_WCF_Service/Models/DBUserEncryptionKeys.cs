using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Google.Cloud.Firestore;

namespace SecureDesk_WCF_Service.Models
{
    //This object represents the User Encryption Keys stored in the database
    [FirestoreData]
    public class DBUserEncryptionKeys
    {

        
        [FirestoreProperty]
        public string userEmailAddress { get; set; }
        

        [FirestoreProperty]
        public string userKeyName { get; set; }
        

        [FirestoreProperty]
        public DateTime keyGenerationDate { get; set; }
       
    }
}