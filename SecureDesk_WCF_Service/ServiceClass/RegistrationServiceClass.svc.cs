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


namespace SecureDesk_WCF_Service
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    //This class uses the Percall Instance mode so each time the request will come new instance will be managed by the service

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class Service1 : RegistrationService
    {
        private  IFirebaseClient client = null;

        //Method use to connec to the FireBase Database
        public string connectToFirebase()
        {
            //creating the instance of the Firebase_Configuration Class to connect to the Firebase Database 

            Firebase_Configuration configuration = new Firebase_Configuration();
             client=configuration.Configure();

            //check whether the connection is sucessfull or not
            if (client != null)
            {
                return string.Format("Connection is established");
            }
            else
            {
                return string.Format("Connection is not successfull");
            }
        }

        

        



        //This method help to store the data in the Firebase and generates the Otp for the user for the user verification
        public  void registerNewUser(UserRegister user)
        {
            //UserOtpVerification otp = new UserOtpVerification();

            string hash_password="";

            //Creating the User Instance for the persistance of the user data in the database 
            //user is the object which contains the information retrieved by the user windows application

            User newUser = new User()
            {
                EmailAddress = user.Email_Address,
                firstName = user.First_Name,
                lastName = user.Last_Name,
                dateOfBirth = user.Date_Of_Birth,
                questionSelected = user.Question_Number_Selected,
                questionAnswered = user.Question_Answered,
                password = hash_password,
                verified = false
            };

            //inserting the client data to the database
            SetResponse response = client.Set("SecureDesk/User/" + user.Email_Address, newUser);
            User userResult = response.ResultAs<User>();

            //creating secret key of the user
            string key = new string((newUser.EmailAddress + newUser.password).ToCharArray().OrderBy(x => Guid.NewGuid()).ToArray());
            byte[] secretKey = Encoding.ASCII.GetBytes(key.Substring(0,7));

            //inserting secret key to database
            DBuserKeys userKey = new DBuserKeys();
            userKey.Email_Address = newUser.EmailAddress;
            userKey.UserSecretKey = Encoding.ASCII.GetString(secretKey);

           
            SetResponse response1 = client.Set("SecureDesk/UserKeys/" + userKey.Email_Address, userKey);
            DBuserKeys userResult1 = response1.ResultAs<DBuserKeys>();

            sendOTP(newUser.EmailAddress);

            /*Creating otp and tempararily store it into the database
            var TotpObj = new Totp(secretKey, step: 60);
            var otpString = TotpObj.ComputeTotp();
            otp.Email_Address = newUser.EmailAddress;
            otp.OTP = int.Parse(otpString);

            SetResponse response2 = client.Set("SecureDesk/UserOtps/" + userKey.Email_Address, otp);
            UserOtpVerification userResult2 = response2.ResultAs<UserOtpVerification>();

            
             
            //sending otp using email
            MailMessage mailMessage = new MailMessage("desksecure7@gmail.com", "kamaniyash811@gmail.com");
            mailMessage.Subject = "OTP for Secure Desk";
            mailMessage.Body = "Otp code is : " + userResult2.OTP;

            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
            smtpClient.Credentials = new System.Net.NetworkCredential()
            {
                UserName = "desksecure7@gmail.com",
                Password = "SAURAVYASH1127"
            };
            smtpClient.EnableSsl = true;
            smtpClient.Send(mailMessage);
            */
        }

        public void sendOTP(string email)
        {
            FirebaseResponse response = client.Get("SecureDesk/UserKeys/" + email);
            DBuserKeys user = response.ResultAs<DBuserKeys>();

            //Creating otp 
            var TotpObj = new Totp(Encoding.ASCII.GetBytes(user.UserSecretKey), step: 60);
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

        public bool verifyUser(UserOtpVerification userOtpObj )
        {
            FirebaseResponse response = client.Get("SecureDesk/UserKeys/" + userOtpObj.Email_Address);
            DBuserKeys user = response.ResultAs<DBuserKeys>();

            byte[] secretKey = Encoding.ASCII.GetBytes(user.UserSecretKey);
            var totp = new Totp(secretKey, step: 60);
            long timeStepMatched;
            bool otpValid = totp.VerifyTotp(userOtpObj.OTP.ToString(), out timeStepMatched, window: null);
            if (otpValid)
            {
                FirebaseResponse response1 = client.Get("SecureDesk/User/" + userOtpObj.Email_Address);
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
                User result = response2.ResultAs<User>();


                return true;
            }

            return false;
        }
        public int getSecurePin(string email)
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
            FirebaseResponse response1 = client.Get("SecureDesk/User/" + email);
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
            User result = response2.ResultAs<User>();

            return SecurePin;
        }

    }
}
