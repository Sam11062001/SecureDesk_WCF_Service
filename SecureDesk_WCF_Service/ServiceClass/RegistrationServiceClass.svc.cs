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
        //end of the method implementation


     
        //This method help to store the data in the Firebase and generates the Otp for the user for the user verification
        public  UserOtpVerification registerNewUser(UserRegister user)
        {
            UserOtpVerification otp = new UserOtpVerification();

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
            SetResponse response =  client.Set("SecureDesk/User/" + user.Email_Address, newUser);
            var userResult = response.ResultAs<User>();

            


            return otp;
        }


        
    }
}
