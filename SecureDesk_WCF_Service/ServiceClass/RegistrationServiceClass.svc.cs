using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using SecureDesk_WCF_Service.Models;
using OtpNet;
using System.Net.Mail;
using System.Security.Cryptography;
using SecureDesk_WCF_Service.Algorithms;
using Google.Cloud.Firestore;
using System.Threading.Tasks;

namespace SecureDesk_WCF_Service
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    //This class uses the Percall Instance mode so each time the request will come new instance will be managed by the service

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class Service1 : RegistrationService
    {
        
        FirestoreDb db;
        //Method use to connec to the FireBase Database
        public Boolean connectToFirebase()
        {
            //creating the instance of the Firebase_Configuration Class to connect to the Firebase Database 

            
            string path = AppDomain.CurrentDomain.BaseDirectory + @"deskcloud-155bf-firebase-adminsdk-htpcm-c5324a5466.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS",path);

            db = FirestoreDb.Create("deskcloud-155bf");
            if (db != null)
                return true;
            else
                return false;
        }

        
        public async Task<string[]> getQuestions()
        {
            Boolean connectionResult = connectToFirebase();

            
            Query queryRef = db.Collection("Questions");
            QuerySnapshot snaps = await queryRef.GetSnapshotAsync();

            int cntQues = 0;
            foreach (DocumentSnapshot snap in snaps)
            {
                cntQues++;
            }
            string[] questions = new string[cntQues];

            int i = 0;
            foreach( DocumentSnapshot snap in snaps )
            {
                DBQuestions questionObj = snap.ConvertTo<DBQuestions>();
                questions[i] = questionObj.question;
                i++;
            }

            return questions;

            

        }

        



        //This method help to store the data in the Firebase and generates the Otp for the user for the user verification
        public  async  void  registerNewUser(UserRegister user)
        {
            
            Boolean connectionResult = connectToFirebase();
            
            
            string [] hash_password = Password.giveHashPassword(user.Password);

            //Creating the User Instance for the persistance of the user data in the database 
            //user is the object which contains the information retrieved by the user windows application

            DocumentReference doc1 = db.Collection("User").Document(user.Email_Address);
            Dictionary<string, object> data1 = new Dictionary<string, object>()
            {
                { "email" , user.Email_Address },
                { "firstName" , user.First_Name },
                { "lastName" , user.Last_Name },
                { "dateOfBirth" , user.Date_Of_Birth },
                { "password" , hash_password[1] },
                { "questionSelected" , user.Question_Number_Selected },
                { "questionAnswered" , user.Question_Answered },
                { "salt" , hash_password[0] },
                { "verified" , false },

            };
            doc1.SetAsync(data1);

            
            
            //creating secret key of the user
            string key = new string((user.Email_Address + user.Password).ToCharArray().OrderBy(x => Guid.NewGuid()).ToArray());
            //inserting secret key to database

            DocumentReference doc2 = db.Collection("UserKeys").Document(user.Email_Address);
            Dictionary<string, object> data2 = new Dictionary<string, object>()
            {
                { "email" , user.Email_Address },
                { "userKey" , key },

            };
            doc2.SetAsync(data2);

            

            sendOTP(user.Email_Address);
            
           
        }

        public async void sendOTP(string email)
        {

            Boolean connectionResult = connectToFirebase();
            DocumentReference docRef1 = db.Collection("UserKeys").Document(email);
            DocumentSnapshot docSnap1 = await docRef1.GetSnapshotAsync();

            DBUserKeys DBuserKeyObj = docSnap1.ConvertTo<DBUserKeys>();


            
            //var bytes= Base32Encoding.ToBytes(DBuserKeyObj.userKey);
            var bytes = Encoding.ASCII.GetBytes(DBuserKeyObj.userKey);

            //Creating otp 
            var TotpObj = new Totp(bytes, step: 240);
            var otpString = TotpObj.ComputeTotp();
            



            //sending otp using email
            MailMessage mailMessage = new MailMessage("desksecure7@gmail.com", email);
            mailMessage.Subject = "OTP for Secure Desk";
            mailMessage.Body = "Otp code is : " + otpString;

            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
            smtpClient.Credentials = new System.Net.NetworkCredential()
            {
                UserName = "desksecure7@gmail.com",
                Password = "SAURAVYASH1127"
            };
            smtpClient.EnableSsl = true ;
            smtpClient.Send(mailMessage);
        }

        public async Task<OTP_Verified> verifyUser(UserOtpVerification userOtpObj )
        {
            Boolean connectionResult = connectToFirebase();
            DocumentReference docRef1 = db.Collection("UserKeys").Document(userOtpObj.Email_Address);
            DocumentSnapshot docSnap1 = await docRef1.GetSnapshotAsync();

            DBUserKeys DBuserKeyObj = docSnap1.ConvertTo<DBUserKeys>();

            
            
            OTP_Verified verification_status = new OTP_Verified();
            var bytes = Encoding.ASCII.GetBytes(DBuserKeyObj.userKey);
            var totp = new Totp(bytes, step: 240);
            long timeStepMatched;
            bool otpValid = totp.VerifyTotp(userOtpObj.OTP.ToString(), out timeStepMatched, window: null);
            if (otpValid)
            {
                DocumentReference docRef2 = db.Collection("User").Document(DBuserKeyObj.email);
                Dictionary<string, object> data2 = new Dictionary<string, object>()
                {
                    { "verified" , true },

                };
                //DocumentSnapshot docSnap2 = await docRef2.GetSnapshotAsync();
                await docRef2.UpdateAsync(data2);

                
                verification_status.Verification_Result = true;

                return verification_status;
            }
            verification_status.Verification_Result = false;
            return verification_status;
        }
        public async Task<int> getSecurePin(string email)
        {
            Boolean connectionResult = connectToFirebase();
            int SecurePin;
            while (true)
            {
                var cryptoRng = new RNGCryptoServiceProvider();
                byte[] buffer = new byte[sizeof(UInt64)];
                cryptoRng.GetBytes(buffer);
                var num = BitConverter.ToUInt64(buffer, 0);
                var pin = num % 1000000; 
                if (pin > 99999)
                {
                    SecurePin = Convert.ToInt32(pin);
                    break;
                }
            }

            DocumentReference docRef1 = db.Collection("User").Document(email);
            Dictionary<string, object> data1 = new Dictionary<string, object>()
                {
                    { "securePin" , SecurePin },

                };
            //DocumentSnapshot docSnap2 = await docRef2.GetSnapshotAsync();
            await docRef1.UpdateAsync(data1);

            

            return SecurePin;
        }

        public async Task<string> getUserQuestion(string email)
        {
            Boolean connectionResult = connectToFirebase();
            DocumentReference docRef1 = db.Collection("User").Document(email);
            DocumentSnapshot docSnap1 = await docRef1.GetSnapshotAsync();

            User userObj = docSnap1.ConvertTo<User>();

            DocumentReference docRef2 = db.Collection("Questions").Document(userObj.questionSelected.ToString());
            DocumentSnapshot docSnap2 = await docRef2.GetSnapshotAsync();

            DBQuestions question = docSnap2.ConvertTo<DBQuestions>();

            return (question.question);
        }

        public async Task<bool> verifyAnswer(string email, string userAnswer)
        {
            Boolean connectionResult = connectToFirebase();
            DocumentReference docRef1 = db.Collection("User").Document(email);
            DocumentSnapshot docSnap1 = await docRef1.GetSnapshotAsync();

            User userObj = docSnap1.ConvertTo<User>();

            if (userObj.questionAnswered == userAnswer)
                return true;
            else
                return false;
        }

        public async void resetPassword(string email, string newPassword)
        {
            Boolean connectionResult = connectToFirebase();

            string[] hash_password = Password.giveHashPassword(newPassword);

            DocumentReference docRef1 = db.Collection("User").Document(email);
            Dictionary<string, object> data1 = new Dictionary<string, object>()
                {
                    { "password" , hash_password[1] },                
                    { "salt" , hash_password[0] }

                };
            //DocumentSnapshot docSnap2 = await docRef2.GetSnapshotAsync();
            await docRef1.UpdateAsync(data1);

        }
    }
}
