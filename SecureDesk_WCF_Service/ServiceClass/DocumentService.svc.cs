using Firebase.Auth;
using Firebase.Storage;
using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SecureDesk_WCF_Service.Models;

namespace SecureDesk_WCF_Service.Services
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "DocumentService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select DocumentService.svc or DocumentService.svc.cs at the Solution Explorer and start debugging.
    public class DocumentService : IDocumentService
    {
        private static string apiKey = "AIzaSyC5IUVt3l3rShryC0dWZ5xeyw-iqj0Yfto";
        private static string bucket = "deskcloud-155bf.appspot.com";
        private static string authEmail = "kamaniyash811@gmail.com";
        private static string authPassword = "Yash@9274";
        string link;
        FileStream fs;

        FirestoreDb db;

        public async Task<DocumentData[]> getAllDocumnetData(string email)
        {
            Boolean connectionResult = connectToFirebase();

            Query allDocumentsQuery = db.Collection("UserDocuments").Document(email).Collection("MyDocuments");
            QuerySnapshot snaps = await allDocumentsQuery.GetSnapshotAsync();

            int documentCount = 0;
            foreach (DocumentSnapshot snap in snaps)
            {
                documentCount++;
            }
            DocumentData[] documents = new DocumentData[documentCount];

            int i = 0;
            foreach (DocumentSnapshot snap in snaps)
            {
                DBDocumentData dBDocumentData = snap.ConvertTo<DBDocumentData>();
                documents[i] = new DocumentData();
                documents[i].fileName = dBDocumentData.fileName;
                documents[i].fileLink = dBDocumentData.fileLink;

                i++;
            }
            return documents;

        }

        public Boolean connectToFirebase()
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

        

        public async void addDocument(string link, string fileName)
        {
            Boolean connectionResult = connectToFirebase();

            DocumentReference doc1 = db.Collection("UserDocuments").Document("kamaniyash811@gmail.com").Collection("MyDocuments").Document(fileName);
            Dictionary<string, object> data1 = new Dictionary<string, object>()
            {
                { "fileName" , fileName },
                
                { "fileLink" , link },


            };
            await doc1.SetAsync(data1);

        }

        public void DoWork()
        {
        }

        public async void uploadDocument(byte[] fileByte, string fileName)
        {
            Stream stream = new MemoryStream(fileByte);
            var auth = new FirebaseAuthProvider(new FirebaseConfig(apiKey));
            var a = await auth.SignInWithEmailAndPasswordAsync(authEmail, authPassword);
            var cancellation = new CancellationTokenSource();
            var upload = new FirebaseStorage(
                bucket,
                new FirebaseStorageOptions
                {

                    AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
                    ThrowOnCancel = true
                }
                )
                .Child("Document")
                .Child(fileName)
                .PutAsync(stream, cancellation.Token);

            try
            {
                link = await upload;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception : " + ex.Message);
            }
            Console.WriteLine("done");

            addDocument( link , fileName);
        }

        
    }
}
