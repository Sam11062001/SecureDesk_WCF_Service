﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using Google.Cloud.Firestore;
using SecureDesk_WCF_Service.Models;
using System.Threading.Tasks;

namespace SecureDesk_WCF_Service.Services
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "SharingService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select SharingService.svc or SharingService.svc.cs at the Solution Explorer and start debugging.
   
    public class SharingService : ISharingService
    {
        //getting the configuration from the azure app Configuration

        private static string apiKey = System.Configuration.ConfigurationManager.AppSettings["FirestoreApiKey"];
        private static string bucket = System.Configuration.ConfigurationManager.AppSettings["FirestoreBucketKey"];
        private static string authEmail = System.Configuration.ConfigurationManager.AppSettings["FirestoreAuthenticationEmailAddress"];
        private static string authPassword = System.Configuration.ConfigurationManager.AppSettings["FirestoreAuthenticationPassword"];


        FirestoreDb db;

        public Boolean connectToFirebase()
        {
            try
            {
                //creating the instance of the Firebase_Configuration Class to connect to the Firebase Database 
                string path = AppDomain.CurrentDomain.BaseDirectory + @"deskcloud-155bf-firebase-adminsdk-htpcm-c5324a5466.json";
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);
                db = FirestoreDb.Create("deskcloud-155bf");
                if (db != null)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                //Catch the Throw the Fault Contract Exception
                CustomException customException = new CustomException();
                customException.errorTitleName = ex.Message;
                customException.errorMessageToUser = "Cannot Get Your Account Details,Please try again Later";
                throw new FaultException<CustomException>(customException);
            }
        }

        public void deleteSharedDocument(SharedDocumentData sharedDocumentData, string emailSharedTo)
        {
            Boolean connectionResult = connectToFirebase();
            DocumentReference doc1 = db.Collection("SharedDocuments").Document(emailSharedTo).Collection(sharedDocumentData.sharedBy).Document(sharedDocumentData.fileName);
            doc1.DeleteAsync();
        }

        public async Task<SharedDocumentData[]> getSharedDocument(string email)
        {
            int sharedDocumentCount = 0;
            Boolean connectionResult = connectToFirebase();

            DocumentReference documentReference = db.Collection("SharedDocuments").Document(email);
            IAsyncEnumerable<CollectionReference> collectionRefrences = documentReference.ListCollectionsAsync();
            IAsyncEnumerator<CollectionReference> enumerator = collectionRefrences.GetAsyncEnumerator();
            while ( await enumerator.MoveNextAsync())
            {
                CollectionReference collectionReference = enumerator.Current;
                Query allDocumentsQuery = documentReference.Collection(collectionReference.Id);
                QuerySnapshot snaps = await allDocumentsQuery.GetSnapshotAsync();

               
                foreach (DocumentSnapshot snap in snaps)
                {
                    sharedDocumentCount++;
                }

            }
            

            SharedDocumentData[] sharedDocuments = new SharedDocumentData[sharedDocumentCount];

            IAsyncEnumerator<CollectionReference> enumerator1 = collectionRefrences.GetAsyncEnumerator();
            int i = 0;
            while (await enumerator1.MoveNextAsync())
            {
                CollectionReference collectionReference = enumerator1.Current;
                Query allDocumentsQuery = documentReference.Collection(collectionReference.Id);
                QuerySnapshot snaps = await allDocumentsQuery.GetSnapshotAsync();


                foreach (DocumentSnapshot snap in snaps)
                {
                    DBSharedDocumentData dBSharedDocumentData = snap.ConvertTo<DBSharedDocumentData>();
                    sharedDocuments[i] = new SharedDocumentData();
                    sharedDocuments[i].fileName = dBSharedDocumentData.fileName;
                    sharedDocuments[i].fileLink = dBSharedDocumentData.fileLink;
                    sharedDocuments[i].sharedBy = dBSharedDocumentData.sharedBy;
                    i++;
                }

            }
            return sharedDocuments;

            
        }

        public async void shareDocument(DocumentData documentData, string sharedBy, string sharedTo)
        {
            Boolean connectionResult = connectToFirebase();

            DocumentReference doc1 = db.Collection("SharedDocuments").Document(sharedTo).Collection(sharedBy).Document(documentData.fileName);
            Dictionary<string, object> data1 = new Dictionary<string, object>()
            {
                { "fileName" , documentData.fileName },

                { "fileLink" , documentData.fileLink },

                { "sharedBy" , sharedBy },


            };
            await doc1.SetAsync(data1);


        }

        
    }
}


