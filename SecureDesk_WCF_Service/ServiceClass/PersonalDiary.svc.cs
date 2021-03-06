﻿using Firebase.Auth;
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

using System.Net;
using SecureDesk_WCF_Service.Models;

namespace SecureDesk_WCF_Service.Services
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "PersonalDiary" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select PersonalDiary.svc or PersonalDiary.svc.cs at the Solution Explorer and start debugging.
    public class PersonalDiary : IPersonalDiary
    {
        //getting the configuration from the azure app Configuration
        private static string apiKey = System.Configuration.ConfigurationManager.AppSettings["FirestoreApiKey"];
        private static string bucket = System.Configuration.ConfigurationManager.AppSettings["FirestoreBucketKey"];
        private static string authEmail = System.Configuration.ConfigurationManager.AppSettings["FirestoreAuthenticationEmailAddress"];
        private static string authPassword = System.Configuration.ConfigurationManager.AppSettings["FirestoreAuthenticationPassword"];
        string link;
        FileStream fs;

        FirestoreDb db;

        public async void addDiary(string link, string date, string title, string email )
        {
            Boolean connectionResult = connectToFirebase();
            string fileName = date.Replace("/", "");
            DocumentReference doc1 = db.Collection("UserDiaries").Document(email).Collection("MyDiary").Document(fileName);
            Dictionary<string, object> data1 = new Dictionary<string, object>()
            {
                { "date" , date },
                { "title" , title },
                { "fileLink" , link },
                

            };
            await doc1.SetAsync(data1);

            
        }

        //Method use to connec to the FireBase Database
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

        public async  Task<DiaryData[]> getAllDiaryData(string email)
        {
            Boolean connectionResult = connectToFirebase();

            Query allDiariesQuery = db.Collection("UserDiaries").Document(email).Collection("MyDiary");
            QuerySnapshot snaps = await allDiariesQuery.GetSnapshotAsync();

            int diaryCount = 0;
            foreach (DocumentSnapshot snap in snaps)
            {
                diaryCount++;
            }
            DiaryData[] diaries = new DiaryData[diaryCount];

            int i = 0;
            foreach (DocumentSnapshot snap in snaps)
            {
                DBDiaryData dBDiaryData = snap.ConvertTo<DBDiaryData>();
                diaries[i] = new DiaryData();
                diaries[i].date = dBDiaryData.date;
                diaries[i].title = dBDiaryData.title;
                diaries[i].fileLink = dBDiaryData.fileLink;
                
                i++;
            }
            return diaries;


        }

        public  void getDiary(string link )
        {
            

            WebClient webClient = new WebClient();
            //webClient.DownloadFile( link , @"C:\Users\kaman\Desktop\Document.pdf");
            byte[] dataBuffer = webClient.DownloadData(link);

            string data = Encoding.ASCII.GetString(dataBuffer);
            Console.WriteLine("done");



        }

        public async Task<int> getDiaryDocumentCount(string email)
        {
            Boolean connectionResult = connectToFirebase();

            int count = 0;

            Query allDiariesQuery = db.Collection("UserDiaries").Document(email).Collection("MyDiary");
            QuerySnapshot allDiariesQuerySnapshot = await allDiariesQuery.GetSnapshotAsync();
            count = allDiariesQuerySnapshot.Documents.Count;
            return count;
        }

        public async void UploadDayThought(string date, string title, string content, string email)
        {
            string fileName = date.Replace("/","");
            //fs = new FileStream(@"C:\Users\kaman\Documents\Epass.pdf", FileMode.Open);
            string text = "Title : " + title + "\n\n" + content; 
            //string test = "test diary";
            byte[] byteArray = Encoding.ASCII.GetBytes(text);
            MemoryStream stream = new MemoryStream(byteArray);

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
                .Child("Diary")
                .Child(fileName+".txt")
                .PutAsync(stream, cancellation.Token);

            try
            {
                link = await upload;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Duhhhhhhhh");
            }
            Console.WriteLine("Hurrraahhhhh");
            

            addDiary(link, date, title, email);

            
        }



    }
}

