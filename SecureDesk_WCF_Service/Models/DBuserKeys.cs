﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Google.Cloud.Firestore;

namespace SecureDesk_WCF_Service.Models
{
    [FirestoreData]
    public class DBUserKeys
    {
        [FirestoreProperty]
        public string email { get; set; }

        [FirestoreProperty]
        public string userKey { get; set; }
    }
}