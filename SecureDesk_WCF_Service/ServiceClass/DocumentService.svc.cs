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
        //getting the configuration from the azure app Configuration
        private static string apiKey = System.Configuration.ConfigurationManager.AppSettings["FirestoreApiKey"];
        private static string bucket = System.Configuration.ConfigurationManager.AppSettings["FirestoreBucketKey"];
        private static string authEmail = System.Configuration.ConfigurationManager.AppSettings["FirestoreAuthenticationEmailAddress"];
        private static string authPassword = System.Configuration.ConfigurationManager.AppSettings["FirestoreAuthenticationPassword"];
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



        public async void addDocument(string link, string fileName, string email)
        {
            Boolean connectionResult = connectToFirebase();

            DocumentReference doc1 = db.Collection("UserDocuments").Document(email).Collection("MyDocuments").Document(fileName);
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

        public async void uploadDocument(byte[] fileByte, string fileName , string email)
        {
            //Get the stream object to consume the byte array
            Stream stream = new MemoryStream(fileByte);
            //create the auth client to the firebase to access the firebase storage
            var auth = new FirebaseAuthProvider(new FirebaseConfig(apiKey));
            //authentication to the firebase storage
            var a = await auth.SignInWithEmailAndPasswordAsync(authEmail, authPassword);
            //get the cancellation token 
            var cancellation = new CancellationTokenSource();
            //uploading the user document
            var upload = new FirebaseStorage(bucket,
                new FirebaseStorageOptions
                {

                    AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
                    ThrowOnCancel = true
                }
                )
                .Child("Document")
                .Child(fileName)
                .PutAsync(stream, cancellation.Token); //adding thr file asynchronously

            
            try
            {
                link = await upload;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception : " + ex.Message);
            }
            Console.WriteLine("done");


            addDocument( link , fileName, email);

        }

        public void deleteDocument( string email , string fileName )
        {
            Boolean connectionResult = connectToFirebase();

            DocumentReference doc1 = db.Collection("UserDocuments").Document(email).Collection("MyDocuments").Document(fileName);
            doc1.DeleteAsync();
        }
    }
}
