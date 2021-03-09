using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using SecureDesk_WCF_Service.Models;
using OtpNet;
using System.Net.Mail;
using System.Security.Cryptography;
using SecureDesk_WCF_Service.Algorithms;
using Google.Cloud.Firestore;
using System.Threading.Tasks;
using System.Web;
using System.IO;
using System.Reflection;
using System.Web.UI;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.html.simpleparser;
using System.Net;
using Azure.Storage.Blobs;

namespace SecureDesk_WCF_Service
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    //This class uses the Persession Instance mode so each time the request will come new instance will be managed by the service


    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Multiple)]

    //This service is responsible for the registration purpose
    public class Service1 : RegistrationService
    {
        FirestoreDb db;
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

        //This method gets the security question from the Firestore database
        public async Task<string[]> getQuestions()
        {
            string[] questions = null; //declare the array of string to return to the user
            //starting of try catch block
            try
            {
                //Connection to the database
                Boolean connectionResult = connectToFirebase();
                //Connection is successfull
                if (connectionResult)
                {
                    //create  the query to get the questions
                    Query queryRef = db.Collection("Questions");
                    ///get the snapshots for getting the question
                    QuerySnapshot snaps = await queryRef.GetSnapshotAsync();
                    //increment variable
                    int cntQues = 0;
                    foreach (DocumentSnapshot snap in snaps)
                    {
                        cntQues++;//increment the variable
                    }//end of foreach loop
                    questions = new string[cntQues];
                    int i = 0;
                    foreach (DocumentSnapshot snap in snaps)
                    {
                        //get the question from each document snapshot
                        DBQuestions questionObj = snap.ConvertTo<DBQuestions>();
                        questions[i] = questionObj.question;
                        i++;
                    }//end of foreach loop
                }//end of IF connection result
            }//end of try block
            catch (Exception ex)
            {
                CustomException customException = new CustomException();
                customException.errorTitleName = ex.Message;
                customException.errorMessageToUser = "Something Wrong Occured,Please try again Later";
                throw new FaultException<CustomException>(customException);
            }//end of catch block
            //return the question to the client
            return questions;
        }

        private static int PIN;
        private static string user_Email;
        private static string user_Password;

        //This method help to store the data in the Firebase and generates the Otp for the user for the user verification
        public async void registerNewUser(UserRegister user)
        {
            try
            {
                //connection to the Database
                Boolean connectionResult = connectToFirebase();

                //creating secret key of the user
                string key = new string((user.Email_Address + user.Password).ToCharArray().OrderBy(x => Guid.NewGuid()).ToArray());
                //inserting secret key to database

                //Create the Instance for the Document reference
                DocumentReference doc2 = db.Collection("UserKeys").Document(user.Email_Address);
                Dictionary<string, object> data2 = new Dictionary<string, object>()
                 {
                    { "email" , user.Email_Address },
                    { "userKey" , key },

                };
                await doc2.SetAsync(data2);

                //send the OTP to the User
                await sendOTP(user.Email_Address, user.First_Name + " " + user.Last_Name,false);

                user_Email = user.Email_Address;
                user_Password = user.Password;

               
                //Hash the password
                string[] hash_password = Password.giveHashPassword(user.Password);

                //Creating the User Instance for the persistance of the user data in the database 
                //user is the object which contains the information retrieved by the user windows application

                DocumentReference doc1 = db.Collection("User").Document(user.Email_Address);
                //Create the Dictonary to Intialiaze the Object to be stored on the firestore

                //get the secure access Pin for the user
                PIN = GenerateRandomInt(000001, 999999);
                string bcrypt_PIN = Password.Bcrpyt_Password(PIN.ToString());
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
                    {"accessPIN",bcrypt_PIN },
                    {"dateOfRegistration",System.DateTime.Now.ToString() },

                };
                //Set the account asynchronously on firestore database
                await doc1.SetAsync(data1);
                
                

            }
            catch (Exception ex)
            {
                //catch the exception and throw to the client side
                CustomException customException = new CustomException();
                customException.errorTitleName = ex.Message;
                customException.errorMessageToUser = "Sorry We Are Facing Some Error While Registartion Process," +
                    "Please Try Again Later!!";

                throw new FaultException<CustomException>(customException);
            }


        }
        //This method is responsible for the generation of the SecureDesk Access Pin
        public static int GenerateRandomInt(int minVal = 0, int maxVal = 100)
        {
            //using the RandomCryptoGraphy Service provided by the Microsoft Package 
            var rnd = new byte[4];
            using (var rng = new RNGCryptoServiceProvider())
                rng.GetBytes(rnd);
            var i = Math.Abs(BitConverter.ToInt32(rnd, 0));

            //return the secure Access PIN
            return Convert.ToInt32(i % (maxVal - minVal + 1) + minVal);
        }

        //This operation is sending the OTP to the user for the OTP verificationProcess
        static string otpString = null;
        static DateTime otp_generated_time;
        public async Task<bool> sendOTP(string email, string userName, bool isForgetPassword)
        {
            try
            {
                //Connec to the Firestore Database
                Boolean connectionResult = connectToFirebase();
                //Create the instance to the document refernce and get to the path of the user document
                DocumentReference docRef1 = db.Collection("UserKeys").Document(email);
                //Snapshot for the same document
                DocumentSnapshot docSnap1 = await docRef1.GetSnapshotAsync();
                //Get the User Secret Keys
                DBUserKeys DBuserKeyObj = docSnap1.ConvertTo<DBUserKeys>();

                //var bytes= Base32Encoding.ToBytes(DBuserKeyObj.userKey);
                var bytes = Encoding.ASCII.GetBytes(DBuserKeyObj.userKey);
                //Creating otp 
                var TotpObj = new Totp(bytes, step: 120);
                otpString = TotpObj.ComputeTotp();
                otp_generated_time = System.DateTime.Now;
                //sending otp using email
                string emailAddress = System.Configuration.ConfigurationManager.AppSettings["SecureDeskEmailAddress"];
                MailMessage mailMessage = new MailMessage(emailAddress, email);
               

                //Get the connection string from the azure
                string connection_String = System.Configuration.ConfigurationManager.AppSettings["SecureDeskConnectionStringBlob"];

                //get the reference to the Blob Container of the secure Desk
                BlobContainerClient container = new BlobContainerClient(connection_String, "mysecuredeskcontainer");

                if (!isForgetPassword)
                {
                    mailMessage.Subject = "OTP for Secure Desk";
                    //mailMessage.Body = "Otp code is : " + otpString;

                    //get the refernence to the BlobClient to retrieve the file from the azure blob
                    BlobClient blobClient = container.GetBlobClient("SecureDeskEmailFormat.html");
                    //download the file
                    var response = await blobClient.DownloadAsync();
                    string htmlBody = "";

                    //read all the content from the file
                    using (var streamReader = new StreamReader(response.Value.Content))
                    {
                        while (!streamReader.EndOfStream)
                        {
                            htmlBody += await streamReader.ReadLineAsync();

                        }
                    }

                    htmlBody = htmlBody.Replace("#userName#", userName);
                    htmlBody = htmlBody.Replace("#otpNumber#", otpString);
                    mailMessage.IsBodyHtml = true;
                    mailMessage.Body = htmlBody;
                    //Create the smtp Client and send the email to the user
                    SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
                    smtpClient.Credentials = new System.Net.NetworkCredential()
                    {
                        //Get the network credntials from the Azure Configuration
                        UserName = System.Configuration.ConfigurationManager.AppSettings["SecureDeskEmailAddress"],
                        Password = System.Configuration.ConfigurationManager.AppSettings["SecureDeskEmailPassword"]
                    };
                    //Enabling the SSL level of security of the email
                    smtpClient.EnableSsl = true;
                    //send the Email to the user
                    smtpClient.Send(mailMessage);
                }
                else
                {
                    mailMessage.Subject = "Reset Your Password For Secure Desk Account";
                    //mailMessage.Body = "Otp code is : " + otpString;

                    //get the refernence to the BlobClient to retrieve the file from the azure blob
                    BlobClient blobClient = container.GetBlobClient("ForgetPasswordSecureDeskEmail.html");
                    //download the file
                    var response = await blobClient.DownloadAsync();
                    string htmlBody = "";

                    //read all the content from the file
                    using (var streamReader = new StreamReader(response.Value.Content))
                    {
                        while (!streamReader.EndOfStream)
                        {
                            htmlBody += await streamReader.ReadLineAsync();

                        }
                    }

                    htmlBody = htmlBody.Replace("#userName#", userName);
                    htmlBody = htmlBody.Replace("#otpNumber#", otpString);
                    mailMessage.IsBodyHtml = true;
                    mailMessage.Body = htmlBody;
                    //Create the smtp Client and send the email to the user
                    SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
                    smtpClient.Credentials = new System.Net.NetworkCredential()
                    {
                        //Get the network credntials from the Azure Configuration
                        UserName = System.Configuration.ConfigurationManager.AppSettings["SecureDeskEmailAddress"],
                        Password = System.Configuration.ConfigurationManager.AppSettings["SecureDeskEmailPassword"]
                    };
                    //Enabling the SSL level of security of the email
                    smtpClient.EnableSsl = true;
                    //send the Email to the user
                    smtpClient.Send(mailMessage);
                }
                
                return true;

            }
            catch (Exception ex)
            {
                //catch and throw the exception
                throw new Exception(ex.Message);
            }

        }

        //This method sending the password protected pdf to the user to give the secure access Pin 
        [Obsolete]
        private static async Task<bool> SendPDFEmail(int pin, string email, string password)
        {
            //Get the StringWriter 
            try
            {
                using (StringWriter sw = new StringWriter())
                {
                    //To write the HTML BODY for the Email
                    using (HtmlTextWriter hw = new HtmlTextWriter(sw))
                    {
                        //String Builder to build the message  for the user
                        StringBuilder sb = new StringBuilder();
                        //Write the secure access Pin to the PDF
                        sb.Append("Your Secure Desk Access Pin Is :" + pin);
                        StringReader sr = new StringReader(sb.ToString());
                        //Creating the dynamic pdf and writing the content in it using the  memory Stream Class
                        Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 10f, 0f);
                        HTMLWorker htmlparser = new HTMLWorker(pdfDoc);
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            //using ITextSharp Nuget package to protect the pdf with the password
                            iTextSharp.text.pdf.PdfWriter.GetInstance(pdfDoc, memoryStream);
                            pdfDoc.Open();
                            htmlparser.Parse(sr);
                            //Wrinting to the pdf is completed and then close the pdf
                            pdfDoc.Close();
                            //get the byte array to write to the memory stream
                            byte[] bytes = memoryStream.ToArray();
                            memoryStream.Close();
                            //Convert it to the PDF format
                            using (MemoryStream input = new MemoryStream(bytes))
                            {
                                using (MemoryStream output = new MemoryStream())
                                {
                                    PdfReader reader = new PdfReader(input);
                                    //Encrypt the file using the user Password
                                    PdfEncryptor.Encrypt(reader, output, true, password, password, PdfWriter.ALLOW_SCREENREADERS);
                                    bytes = output.ToArray();
                                }
                            }

                            //Create the Mail and sent to the user
                            string emailAddress = System.Configuration.ConfigurationManager.AppSettings["SecureDeskEmailAddress"];
                            MailMessage mm = new MailMessage(emailAddress, email);
                            //Create the Mail Suject and Body
                            mm.Subject = "Secure Desk Access PIN";
                            mm.Body = "Find Your PIN as the attachment to this mail" +
                                "Provide the same password you provided on Secure Desk";

                            mm.Attachments.Add(new Attachment(new MemoryStream(bytes), "SecureAccessPIN.pdf"));
                            mm.IsBodyHtml = true;
                            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
                            smtpClient.Credentials = new System.Net.NetworkCredential()
                            {
                                UserName = System.Configuration.ConfigurationManager.AppSettings["SecureDeskEmailAddress"],
                                Password = System.Configuration.ConfigurationManager.AppSettings["SecureDeskEmailPassword"]
                            };
                            smtpClient.EnableSsl = true;
                            smtpClient.Send(mm);

                            return true;

                            
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //catch and throw the exception to the user
                throw new Exception(ex.Message);
            }
        }

        [Obsolete]
        public async Task<OTP_Verified> verifyUser(UserOtpVerification userOtpObj)
        {
            OTP_Verified verification_status = new OTP_Verified();
            try
            {
                //connect to the Firebase
                Boolean connectionResult = connectToFirebase();
                if (connectionResult)
                {
                    DocumentReference docRef1 = db.Collection("UserKeys").Document(userOtpObj.Email_Address);
                    DocumentSnapshot docSnap1 = await docRef1.GetSnapshotAsync();
                    DBUserKeys DBuserKeyObj = docSnap1.ConvertTo<DBUserKeys>();
                    var bytes = Encoding.ASCII.GetBytes(DBuserKeyObj.userKey);
                    bool otpValid = false;
                    DateTime otp_obtained_time_from_user = System.DateTime.Now;
                    //Comapre the otp obtained from the user
                    if (otpString.Equals(userOtpObj.OTP.ToString()))
                    {
                        TimeSpan ts = otp_obtained_time_from_user - otp_generated_time;
                        if (ts.TotalMinutes <= 2)
                        {
                            otpValid = true;
                        }
                    }
                    //If OTP is verified then
                    if (otpValid)
                    {
                        //update the Verification Status of the User on the Database
                        DocumentReference docRef2 = db.Collection("User").Document(DBuserKeyObj.email);
                        Dictionary<string, object> data2 = new Dictionary<string, object>()
                        {
                            { "verified" , true },

                        };
                        //DocumentSnapshot docSnap2 = await docRef2.GetSnapshotAsync();
                        await docRef2.UpdateAsync(data2);
                        verification_status.Verification_Result = true;

                        //send the email for secure access Pin to the user
                        await SendPDFEmail(PIN, user_Email, user_Password);
                        //Create Encryption key for the user
                        try
                        {
                            //This class create the encyrption key for the user for the further security features
                            AzureEncryptDecrypt azureEncryptDecrypt = new AzureEncryptDecrypt();
                            string encryptionKeyName = await azureEncryptDecrypt.createEncryptionRSAKey();

                            DocumentReference documentReference_for_keys = db.Collection("UserEncryptionKeys").Document(userOtpObj.Email_Address);
                            Dictionary<string, object> userKeyValues = new Dictionary<string, object>()
                            {
                                {
                                    "userEmailAddress",userOtpObj.Email_Address
                                },
                                {
                                    "userKeyName",encryptionKeyName
                                },
                                {
                                    "keyGenerationDate",DateTime.Now.ToString()
                                }
                            };

                            await documentReference_for_keys.SetAsync(userKeyValues);
                        }
                        catch (Exception ex)
                        {
                            //catch the exception and throw the error on the client 
                            CustomException customException = new CustomException();
                            customException.errorTitleName = ex.Message;
                            customException.errorMessageToUser = "Encryption Key Generation Error";
                            throw new FaultException<CustomException>(customException);
                        }
                        return verification_status;
                    }
                }
            }
            catch (Exception ex)
            {
                //catch the exception and throw the error on the client 
                CustomException customException = new CustomException();
                customException.errorTitleName = ex.Message;
                customException.errorMessageToUser = "Cannot Verify,Please try again Later!!";
                throw new FaultException<CustomException>(customException);
            }
            //make false if the code reaches upto this
            verification_status.Verification_Result = false;
            //return the verification status to the user
            return verification_status;
        }


        //This method retruns the user selected question at the time of the registration
        public async Task<string> getUserQuestion(string email)
        {
            string security_question = "";
            try
            {
                Boolean connectionResult = connectToFirebase();
                if (connectionResult)
                {
                    DocumentReference docRef1 = db.Collection("User").Document(email);
                    DocumentSnapshot docSnap1 = await docRef1.GetSnapshotAsync();
                    User userObj = docSnap1.ConvertTo<User>();
                    DocumentReference docRef2 = db.Collection("Questions").Document(userObj.questionSelected.ToString());
                    DocumentSnapshot docSnap2 = await docRef2.GetSnapshotAsync();
                    DBQuestions question = docSnap2.ConvertTo<DBQuestions>();
                    security_question = question.question;
                }
                return security_question;
            }
            catch (Exception ex)
            {
                //catch the exception and throw the error on the client 
                CustomException customException = new CustomException();
                customException.errorTitleName = ex.Message;
                customException.errorMessageToUser = "Cannot Get Your Security Question,Please try again Later!!";
                throw new FaultException<CustomException>(customException);
            }
        }

        public async Task<bool> verifyAnswer(string email, string userAnswer)
        {
            Boolean connectionResult = connectToFirebase();
            try
            {
                DocumentReference docRef1 = db.Collection("User").Document(email);
                DocumentSnapshot docSnap1 = await docRef1.GetSnapshotAsync();
                User userObj = docSnap1.ConvertTo<User>();
                if (userAnswer.Equals(userObj.questionAnswered))
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                CustomException customException = new CustomException();
                customException.errorTitleName = "Cannot Verify Your Answer,Please Try Again later!!";
                customException.errorMessageToUser = ex.Message;
                throw new FaultException<CustomException>(customException);
            }
        }

        public async void resetPassword(string email, string newPassword)
        {
            Boolean connectionResult = connectToFirebase();
            try
            {
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
            catch (Exception ex)
            {
                CustomException customException = new CustomException();
                customException.errorTitleName = "Cannot Reset Your Password,Please Try Again later!!";
                customException.errorMessageToUser = ex.Message;
                throw new FaultException<CustomException>(customException);

            }

        }

        public async Task<bool> isUnique(string email)
        {
            bool isUnique = false;
            try
            {
                
                Boolean connection_result = connectToFirebase();
                if (connection_result)
                {
                    //Query to check whether the account with same acoount name exists or not
                    DocumentReference documentReference = db.Collection("User").Document(email);
                    DocumentSnapshot snapshot = await documentReference.GetSnapshotAsync();
                    if (snapshot.Exists)
                    {
                        isUnique = false;
                    }
                    else
                    {
                        isUnique = true;
                    }

                }
                return isUnique;

            }
            catch(Exception ex)
            {
                CustomException customException = new CustomException();
                customException.errorTitleName = "Cannot Check whether you are unique or not ,Please Try Again later!!";
                customException.errorMessageToUser = ex.Message;
                throw new FaultException<CustomException>(customException);
            }
           
        }

        public async  Task<OTP_Verified> verifyForgetPassword(UserOtpVerification userOtpObj)
        {
            OTP_Verified verification_status = new OTP_Verified();
            verification_status.Verification_Result = false;
            bool otpValid = false;
            try
            {
                //connect to the Firebase
                Boolean connectionResult = connectToFirebase();
                if (connectionResult)
                {
                    DocumentReference docRef1 = db.Collection("UserKeys").Document(userOtpObj.Email_Address);
                    DocumentSnapshot docSnap1 = await docRef1.GetSnapshotAsync();
                    DBUserKeys DBuserKeyObj = docSnap1.ConvertTo<DBUserKeys>();
                    var bytes = Encoding.ASCII.GetBytes(DBuserKeyObj.userKey);
                  
                    DateTime otp_obtained_time_from_user = System.DateTime.Now;
                    //Comapre the otp obtained from the user
                    if (otpString.Equals(userOtpObj.OTP.ToString()))
                    {
                        TimeSpan ts = otp_obtained_time_from_user - otp_generated_time;
                        if (ts.TotalMinutes <= 2)
                        {
                            otpValid = true;
                            verification_status.Verification_Result = true;
                        }
                    }
                }

                return verification_status;
            }
            catch (Exception ex)
            {
                CustomException customException = new CustomException();
                customException.errorTitleName = "Cannot Verify Your OTP ,Please Try Again later!!";
                customException.errorMessageToUser = ex.Message;
                throw new FaultException<CustomException>(customException);
            }
        }
    }
}
