using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;

/* To connect to the firesharp databse we are using the Firesharp Nuget package
 * Firebase.Config: Gives all the code required for the configuration to the Firebase
 * Firebase.Interfaces: Provides all the Methods to work with the Firebase database
 * Firebase.Response : Helps in getting the response from the Firebase
 */
namespace SecureDesk_WCF_Service
{
    public class Firebase_Configuration
    {
        //Authentication to the  Firebase Database we need the FireBaseConfig Class
        //BasePath and Auth Secret is required to connect with the Firebase database
        IFirebaseConfig firebase_config = new FirebaseConfig()
        {
            AuthSecret = "tyMsKyGB9wbvm6aBy5z5IaYGpKdwJUlxVq9KqyVK",
            BasePath = "https://desk-ed328-default-rtdb.firebaseio.com/"
        };
        public IFirebaseClient Configure()
        {
            IFirebaseClient firebaseClient = new FireSharp.FirebaseClient(firebase_config);
            return firebaseClient;

        }
    }
}