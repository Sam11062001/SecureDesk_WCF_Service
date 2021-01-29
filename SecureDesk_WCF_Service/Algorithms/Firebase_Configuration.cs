using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Google.Cloud.Firestore;

namespace SecureDesk_WCF_Service
{
    
    public class Firebase_Configuration
    {
        FirestoreDb db;

        public FirestoreDb connectFireStoreCloud()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + @"deskcloud-155bf-firebase-adminsdk-htpcm-c5324a5466.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);

            db = FirestoreDb.Create("deskcloud-155bf");

            return db;
        }


    }
}