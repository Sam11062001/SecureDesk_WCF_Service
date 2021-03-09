using Google.Cloud.Firestore;
using SecureDesk_WCF_Service.Algorithms;
using SecureDesk_WCF_Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using BC = BCrypt.Net.BCrypt;

namespace SecureDesk_WCF_Service.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Multiple)]
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "UserAccountService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select UserAccountService.svc or UserAccountService.svc.cs at the Solution Explorer and start debugging.
    public class UserAccountService : IUserAccountService
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
            catch(Exception ex)
            {
                //Catch the Throw the Fault Contract Exception
                CustomException customException = new CustomException();
                customException.errorTitleName = ex.Message;
                customException.errorMessageToUser = "Cannot Get Your Account Details,Please try again Later";
                throw new FaultException<CustomException>(customException);
            }
        }
        public async Task<User> getAccountInfo(string email)
        {
            //create the instaance of the user object
            User userInfo = null;
            try
            {
                //connection to the database
                bool connection_result = connectToFirebase();
                //if conenction is successfull
                if (connection_result)
                {
                    //Get the Document reference from the Database
                    DocumentReference documentReference_for_Account = db.Collection("User").Document(email);
                    //Get the Actual Snapshot of the user data from the  database
                    DocumentSnapshot documentSnapshot = await documentReference_for_Account.GetSnapshotAsync();
                    //If the snap for the user exists then
                    if (documentSnapshot.Exists)
                    {
                        userInfo = new User();
                        //convert to the user object
                        userInfo = documentSnapshot.ConvertTo<User>();

                    }//end if
                }//end if
                //return the User Object that contains the user info
                return userInfo;
            }//end try
            catch (Exception ex)
            {
                //Catch the Throw the Fault Contract Exception
                CustomException customException = new CustomException();
                customException.errorTitleName = ex.Message;
                customException.errorMessageToUser = "Cannot Get Your Account Details,Please try again Later";
                throw new FaultException<CustomException>(customException);
            }//end of catch Block
        }//end of method
        public async Task<bool> isAuthorized(string email, int accessPin)
        {
            //reperents the authorization state of the user
            bool isAuthorized = false;
            try
            {
                //connection to the database
                bool connection_Result = connectToFirebase();
                //if the connection is succesfull
                if (connection_Result)
                {
                    //Get the Document reference from the database
                    DocumentReference documentReference = db.Collection("User").Document(email);
                    //Get the snap of the user data form the database
                    DocumentSnapshot documentSnapshot = await documentReference.GetSnapshotAsync();
                    //if the snap exists for the user then check for the validation of the access Pin for the authorization of the user
                    if (documentSnapshot.Exists)
                    {
                        //Conver the snap into the User Object
                        User user_Info = documentSnapshot.ConvertTo<User>();
                        //Bcrypt the Obtianed Pin then verify using the BCRYPT NUGET Package
                        isAuthorized=BC.Verify(accessPin.ToString(), user_Info.accessPIN);

                    }//end if
                }//end if
                //return the result of the authorization
                return isAuthorized;
            }//end the try block
            catch (Exception ex)
            {
                //Catch the exception and throw the to the user
                CustomException customException = new CustomException();
                customException.errorTitleName = ex.Message;
                customException.errorMessageToUser = "You Are UnAuthorized to Perform this action,Provide Correct Access Pin";
                throw new FaultException<CustomException>(customException);
            }
        }

        public async Task<User> updateUserAccount(User updatedUserInfo)
        {
            User updatedUser = null;
            try
            {
                bool connection_Result = connectToFirebase();
                if (connection_Result)
                {
                    //Hash the password for the updating the user password and then store to the user Document
                    string[] hash_password = Password.giveHashPassword(updatedUserInfo.password);

                    //update the Verification Status of the User on the Database
                    DocumentReference docRef2 = db.Collection("User").Document(updatedUserInfo.email);
                    Dictionary<string, object> data2 = new Dictionary<string, object>()
                    {
                        { "firstName" , updatedUserInfo.firstName },
                        { "lastName" , updatedUserInfo.lastName },
                        { "dateOfBirth" , updatedUserInfo.dateOfBirth },
                        { "password" , hash_password[1] },
                        { "salt" , hash_password[0] },

                    };
                    //DocumentSnapshot docSnap2 = await docRef2.GetSnapshotAsync();
                    await docRef2.UpdateAsync(data2);

                    //Get Back the Updated the Account

                    //Get the Document reference from the database
                    DocumentReference documentReference = db.Collection("User").Document(updatedUserInfo.email);
                    //Get the snap of the user data form the database
                    DocumentSnapshot documentSnapshot = await documentReference.GetSnapshotAsync();
                    //if the snap exists for the user then check for the validation of the access Pin for the authorization of the user
                    if (documentSnapshot.Exists)
                    {
                        //Conver the snap into the User Object
                        updatedUser = documentSnapshot.ConvertTo<User>();
                        
                    }//end if
                }//end if
                return updatedUser;
            }//end try
            catch(Exception ex){
                //catch the exception and throw to the user
                CustomException customException = new CustomException();
                customException.errorTitleName = ex.Message;
                customException.errorMessageToUser = "Not Able to Update Your Acccount,Please Try Again Later";
                throw new FaultException<CustomException>(customException);
            }
        }
    }
}
