using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using Google.Cloud.Firestore;
using System.Threading.Tasks;
using SecureDesk_WCF_Service.Models;
using SecureDesk_WCF_Service.Algorithms;
using BC = BCrypt.Net.BCrypt;
namespace SecureDesk_WCF_Service.Services
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "AuthService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select AuthService.svc or AuthService.svc.cs at the Solution Explorer and start debugging.
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class AuthService : IAuthService
    {
        private readonly FirestoreDb database_configuration;
        AuthService()
        {
            Firebase_Configuration config = new Firebase_Configuration();
            database_configuration = config.connectFireStoreCloud();
        }
        

        public async Task<bool> validateLogin(AuthUser authUser)
        {
            bool auth_result = false;
            string pepper = System.Configuration.ConfigurationManager.AppSettings["SecureDeskPasswordPepper"];

            //if the database connection is successfull
            if (database_configuration != null)
            {
                //getting the reference to the collection and then to the specific user
                DocumentReference documentReference = database_configuration.Collection("User").Document(authUser.User_Auth_Email);

                //get the snapshot of the user 
                DocumentSnapshot documentSnapshot = await documentReference.GetSnapshotAsync();

                //convert the obtained snapshot into the User type object .
                User validUser = documentSnapshot.ConvertTo<User>();

                //if the user is found then check the credentials
                if (validUser != null)
                {
                    //generate the hash password using the SHA512 algorithm
                    string authUserHashedpassword = Password.generateSHA512Hash(authUser.User_Auth_Password, validUser.salt, pepper);

                    //pass the current computer password and the valid user passsword
                    auth_result = BC.Verify(authUserHashedpassword, validUser.password);

                    //return the result of the authentication to the client 
                    return auth_result;
                }
                else
                {
                    //that means the user is not found in the database so return the false
                    return auth_result;
                }
            }
            else
            {
                //error in the database connection
                return auth_result;
            }

        }
    }
}
