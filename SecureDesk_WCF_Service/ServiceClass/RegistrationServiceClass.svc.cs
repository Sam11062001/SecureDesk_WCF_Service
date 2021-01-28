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
        //private  IFirebaseClient client = null;
        FirestoreDb db;
        //Method use to connec to the FireBase Database
        public Boolean connectToFirebase()
        {
            //creating the instance of the Firebase_Configuration Class to connect to the Firebase Database 

            /* Firebase_Configuration configuration = new Firebase_Configuration();
              client=configuration.Configure();

             //check whether the connection is sucessfull or not
             if (client != null)
             {
                 return true;
             }
             else
             {
                 return false;
             }*/
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

            /*
            //array that will return to the client for populating the security question field in the registration form
            string[] returnSecurityQuestion = new string[5];

            //connect to the database
            Boolean connectionResult=connectToFirebase();

            //Create the instance for the firebase response which will held the response obtain from the firebase database
            FirebaseResponse response = null;

            //creating the instance of the Security_Question Model class to hold the question object return from the method
            Security_Question question;

            //if connection is successfull then populate the string array
            if (connectionResult)
            {
                //for loop to get the all the question from the datbase
                for (int i = 0; i < 5; i++) {

                    //counter variable which should be start from 1 as the first question id in the database is 1
                    string counter = (i + 1).ToString();

                    //get the response from the fire base database
                  response = client.Get("SecureDesk/Questions/" +counter);

                    //convert the response as the Security_Question Model
                    question = response.ResultAs<Security_Question>();
                    
                    //store the question retrived in the array
                    returnSecurityQuestion[i] = question.Question;
                }

                return returnSecurityQuestion;  //return the array of question
            }
            {
                return null;  //else return null that means the connection to the database was not successfull
            }*/

        }

        //This method populate the question table in the firebase SecureDesk database should be used only when any nnew question are required to be added in the database
        /*public string populateQuestionTable()
        {
            //connec to the firebase database
            Boolean returnResult = connectToFirebase();

            //create the instance for the setresponse class so that the same instance can be used throughout the method for storing the output 
            //getting from the firebase database
            SetResponse response = null;

            //Adding Question Numner:1
            Security_Question question1 = new Security_Question()
            {
                ID = 1,
                Question = "What was your Childhood Nickname ? "
            };
            response = client.Set("SecureDesk/Questions/" + question1.ID, question1);
            var result1 = response.ResultAs<Security_Question>();


            //Adding  Question Number2 
            Security_Question question2 = new Security_Question()
            {
                ID = 2,
                Question = "In which hospital did you born ?"
            };
            response = client.Set("SecureDesk/Questions/" + question2.ID, question2);
            var result2 = response.ResultAs<Security_Question>();

            //Adding Question Number 3
            Security_Question question3 = new Security_Question()
            {
                ID = 3,
                Question = "In which city you meet your spouse/significant other ?"
            };
            response = client.Set("SecureDesk/Questions/" + question3.ID, question3);
            var result3 = response.ResultAs<Security_Question>();


            //Adding Question Number 4
            Security_Question question4 = new Security_Question()
            {
                ID = 4,
                Question = "What is your older sibling middle name ?"
            };
            response = client.Set("SecureDesk/Questions/" + question4.ID, question4);
            var result4 = response.ResultAs<Security_Question>();

            //Adding Question 5
            Security_Question question5 = new Security_Question()
            {
                ID = 5,
                Question = "Where were you on yours first school vaccation ?"
            };
            response = client.Set("SecureDesk/Questions/" + question5.ID, question5);
            var result5 = response.ResultAs<Security_Question>();


            //return the success message 
            return "Succesfully added the question";
        }//end of the method implementation
        */



        //This method help to store the data in the Firebase and generates the Otp for the user for the user verification
        public  async  void  registerNewUser(UserRegister user)
        {
            //UserOtpVerification otp = new UserOtpVerification();
            Boolean connectionResult = connectToFirebase();
            
            /*Firebase_Configuration config = new Firebase_Configuration();
            IFirebaseClient client1 = config.Configure();*/
            
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

            /*User newUser = new User()
            {
                EmailAddress = user.Email_Address,
                firstName = user.First_Name,
                lastName = user.Last_Name,
                dateOfBirth = user.Date_Of_Birth,
                questionSelected = user.Question_Number_Selected,
                questionAnswered = user.Question_Answered,
                password = hash_password[1],
                salt=hash_password[0],
                verified = false
            };

            
            //inserting the client data to the database
            PushResponse response = await client1.PushTaskAsync<User>("SecureDesk/Users/", newUser);
            
            
            
            User userResult = response.ResultAs<User>();
            string user_id = response.Result.Name;
            */
            
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

            /*DBuserKeys userKey = new DBuserKeys();
            userKey.Email_Address = newUser.EmailAddress;
            userKey.UserSecretKey = key;

           
            SetResponse response1 = client1.Set("SecureDesk/UserKeys/" + userKey.Email_Address, userKey);
            DBuserKeys userResult1 = response1.ResultAs<DBuserKeys>();*/

            sendOTP(user.Email_Address);
            
           
        }

        public async void sendOTP(string email)
        {

            Boolean connectionResult = connectToFirebase();
            DocumentReference docRef1 = db.Collection("UserKeys").Document(email);
            DocumentSnapshot docSnap1 = await docRef1.GetSnapshotAsync();

            DBUserKeys DBuserKeyObj = docSnap1.ConvertTo<DBUserKeys>();
            

            /*FirebaseResponse response = client.Get("SecureDesk/UserKeys/" + email);
            userKeys user = response.ResultAs<userKeys>();*/

            var bytes= Base32Encoding.ToBytes(DBuserKeyObj.userKey);
            //Creating otp 
            var TotpObj = new Totp(bytes, step: 60);
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
            DocumentReference docRef1 = db.Collection("UserKeys").Document(userOtpObj.Email_Address);
            DocumentSnapshot docSnap1 = await docRef1.GetSnapshotAsync();

            DBUserKeys DBuserKeyObj = docSnap1.ConvertTo<DBUserKeys>();

            /*FirebaseResponse response = client.Get("SecureDesk/UserKeys/" + userOtpObj.Email_Address);
            userKeys user = response.ResultAs<userKeys>();*/
            
            OTP_Verified verification_status = new OTP_Verified();
            var bytes = Base32Encoding.ToBytes(DBuserKeyObj.userKey);
            var totp = new Totp(bytes, step: 60);
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

                /*FirebaseResponse response1 = client.Get("SecureDesk/User/" + userOtpObj.Email_Address);
                User userResult = response1.ResultAs<User>();
                User updatedUser = new User()
                {
                    EmailAddress = userResult.EmailAddress,
                    firstName = userResult.firstName,
                    lastName = userResult.lastName,
                    dateOfBirth = userResult.dateOfBirth,
                    questionSelected = userResult.questionSelected,
                    questionAnswered = userResult.questionAnswered,
                    password = userResult.password,
                    verified = true
                };

                FirebaseResponse response2 = client.Update("SecureDesk/User/" + userResult.EmailAddress, updatedUser);
                User result = response2.ResultAs<User>();*/
                verification_status.Verification_Result = true;

                return verification_status;
            }
            verification_status.Verification_Result = false;
            return verification_status;
        }
        public async Task<int> getSecurePin(string email)
        {
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

            /*FirebaseResponse response1 = client.Get("SecureDesk/User/" + email);
            User userResult = response1.ResultAs<User>();
            User updatedUser = new User()
            {
                EmailAddress = userResult.EmailAddress,
                firstName = userResult.firstName,
                lastName = userResult.lastName,
                dateOfBirth = userResult.dateOfBirth,
                questionSelected = userResult.questionSelected,
                questionAnswered = userResult.questionAnswered,
                password = userResult.password,
                verified = true,
                securePin = SecurePin
            };

            FirebaseResponse response2 = client.Update("SecureDesk/User/" + userResult.EmailAddress, updatedUser);
            User result = response2.ResultAs<User>();*/

            return SecurePin;
        }

    }
}
