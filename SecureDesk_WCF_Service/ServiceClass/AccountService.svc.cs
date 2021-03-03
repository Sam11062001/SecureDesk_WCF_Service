using Azure.Identity;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Keys.Cryptography;
using Google.Cloud.Firestore;
using SecureDesk_WCF_Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace SecureDesk_WCF_Service.Services
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "AccountService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select AccountService.svc or AccountService.svc.cs at the Solution Explorer and start debugging
    public class AccountService : IAccountService
    {
        //getting the configuration from the azure app Configuration
        private static string apiKey = System.Configuration.ConfigurationManager.AppSettings["FirestoreApiKey"];
        private static string bucket = System.Configuration.ConfigurationManager.AppSettings["FirestoreBucketKey"];
        private static string authEmail = System.Configuration.ConfigurationManager.AppSettings["FirestoreAuthenticationEmailAddress"];
        private static string authPassword = System.Configuration.ConfigurationManager.AppSettings["FirestoreAuthenticationPassword"];

        //Azure Credential to access the User Encryption Keys
        //Getting the configuration from the azure app configuration
        string KeyVaultUrl = System.Configuration.ConfigurationManager.AppSettings["SecureDeskKeyVaultUrl"];
        string clientid = System.Configuration.ConfigurationManager.AppSettings["SecureDeskAccessPoclicyClientId"];
        string tenantId = System.Configuration.ConfigurationManager.AppSettings["SecureDeskAccessPolicyTenantId"];
        string client_secret = System.Configuration.ConfigurationManager.AppSettings["SecureDeskAccessPolicyPassword"];

        //Firestore database object which allows to work with the Firestore database
        FirestoreDb db;
        public async Task<bool> addAccount(UserAccountData userAccountData)
        {
            //variable which represnts the result of the operation;
            bool account_addition_result = false;
            
            try
            {
                //represent whether the connection to the firebase is succesfull or not
                bool connection_Result = connectToFirebase();

                //variable which reprsents whether the account name provided by the user exists in the database or not
                bool accountExists=false;

                //if the connection is established
                if (connection_Result)
                {
                    //Query to check whether the account with same acoount name exists or not
                    Query checkIfAccountExists = db.Collection("UserAccounts").Document(userAccountData.userEmail).Collection("MyAccounts");

                    //if the object is not null means the user has some account in the database
                    if (checkIfAccountExists != null)
                    {
                        //get the snapshot of all the collection for the account name
                        QuerySnapshot snaps = await checkIfAccountExists.GetSnapshotAsync();
                        
                        //Iterate through the collection and check for the same account name
                        foreach (DocumentSnapshot snap in snaps)
                        {
                            if (snap.Exists)
                            {
                                //remove all the white spaces characters from the string
                                string snapId = snap.Id.ToString().Replace(" ", "").ToLower();
                                string userAccountName = userAccountData.Name.ToString().Replace(" ", "").ToLower();

                                //compare the string 
                                if (snapId.Equals(userAccountName))
                                {
                                    accountExists = true;
                                }
                            }
                        }
                        //if the account exists then throw the fault exception to the user
                        if (accountExists)
                        {
                            CustomException customException = new CustomException();
                            customException.errorTitleName = "Account is not added";
                            customException.errorMessageToUser = "Cannot the Add this Account as the same account name already exists in your Database";
                            throw new FaultException<CustomException>(customException);
                        }
                        else
                        {
                             //first of all encrypt the user Data 
                            //Get the User Encryption Key from the firestore database
                            DocumentReference getUserKey = db.Collection("UserEncryptionKeys").Document(userAccountData.userEmail);

                            //get the snapshot for the same
                            DocumentSnapshot snapshot = await getUserKey.GetSnapshotAsync();

                            if (snapshot.Exists)
                            {
                                string keyName = "";
                                Dictionary<string, object> userKeyDict = snapshot.ToDictionary();
                                foreach (KeyValuePair<string, object> pair in userKeyDict)
                                {
                                   if(pair.Key == "userKeyName")
                                    {
                                        keyName = pair.Value.ToString();
                                        break;
                                    }
                                }

                                //Convert to the DBUserEncryption
                                //DBUserEncryptionKeys dBUserEncryptionKeys = snapshot.ConvertTo<DBUserEncryptionKeys>();
                                //string key_Name_For_Encryption = dBUserEncryptionKeys.userKeyName;

                                //Create the client for the azure KeyVault Access
                                var client = new KeyClient(vaultUri: new Uri(KeyVaultUrl), credential: new ClientSecretCredential(tenantId, clientid, client_secret));

                                //Retrive the key from the azure Key Vault
                                KeyVaultKey key = await client.GetKeyAsync(keyName);

                                //Now Creating the Crypto Client for the Encryption of the user Data
                                var cryptoClient = new CryptographyClient(keyId: key.Id, credential: new ClientSecretCredential(tenantId, clientid, client_secret));
                                
                                //Convert the Data into the byte array
                                byte[] newAccountUserName = Encoding.UTF8.GetBytes(userAccountData.UserName);
                                byte[] newAccountPassword = Encoding.UTF8.GetBytes(userAccountData.Password);
                                
                                //Create the EncryptionResult Object to perform the encryption on the user data
                                EncryptResult encryptResult_for_userName = await cryptoClient.EncryptAsync(EncryptionAlgorithm.RsaOaep, newAccountUserName);
                                EncryptResult encryptResult_for_password = await cryptoClient.EncryptAsync(EncryptionAlgorithm.RsaOaep,newAccountPassword);

                                //Retrive the Cipher text from this Encryption Result objects
                                string newAccountUserNameCipherText = Convert.ToBase64String(encryptResult_for_userName.Ciphertext);
                                string newAccountPasswordCipherText = Convert.ToBase64String(encryptResult_for_password.Ciphertext);

                                //add the account in the database
                                DocumentReference documentReference = db.Collection("UserAccounts").Document(userAccountData.userEmail).Collection("MyAccounts").Document(userAccountData.Name);

                                //Create th Dictonary to add the data
                                Dictionary<string, object> keyValuePairs = new Dictionary<string, object>() 
                                {
                                    {
                                        "accountName",userAccountData.Name
                                    },
                                    {
                                        "accountUserName",newAccountUserNameCipherText
                                    },
                                    {
                                        "accountPassword",newAccountPasswordCipherText
                                    }
                                };

                                //Synchronously add the new Account to the firestore database
                                await documentReference.SetAsync(keyValuePairs);

                                //change thee addition result to the true which indicates the succesfull addition of the newAccount
                                account_addition_result = true;

                            }//end of SnapShot.Exists IF Condition

                        }//end of Else block for the accountExists
                    }//End of IF AccountExists Block
                }//End of IF ConnectionResult Block
            }//End of try block
            catch (Exception ex)
            {
                //catch the exception and return to the client
                CustomException customException = new CustomException();
                customException.errorTitleName = "Error Occured While Adding the User Account Information to the Database";
                customException.errorMessageToUser = ex.Message;
                throw new FaultException<CustomException>(customException);
            }//end of catch block


            //return the result of this operation to the client
            return account_addition_result;
        }//End of Method Logic to add the new Account data in the Database


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
        public async Task<bool> deleteAccount(UserAccountData userAccountData)
        {
            bool is_account_deleted = false;

            //connect to the database
            bool connection_result = connectToFirebase();
            try{
           
                //if connection is succesfull
                if (connection_result)
                {
                    bool accountExists = false;

                    //Query to check whether the account with same acoount name exists or not
                    Query checkIfAccountExists = db.Collection("UserAccounts").Document(userAccountData.userEmail).Collection("MyAccounts");

                    //if the object is not null means the user has some account in the database
                    if (checkIfAccountExists != null)
                    {
                        //get the snapshot of all the collection for the account name
                        QuerySnapshot snaps = await checkIfAccountExists.GetSnapshotAsync();

                        //Iterate through the collection and check for the same account name
                        foreach (DocumentSnapshot snap in snaps)
                        {
                            if (snap.Exists)
                            {
                                //remove all the white spaces characters from the string
                                string snapId = snap.Id.ToString().Replace(" ", "").ToLower();
                                string userAccountName = userAccountData.Name.ToString().Replace(" ", "").ToLower();

                                //compare the string 
                                if (snapId.Equals(userAccountName))
                                {
                                    accountExists = true;
                                }
                            }
                        }
                    }
                    if (accountExists)
                    {
                        //delete the account from the firestore
                        DocumentReference documentReference_for_deletion = db.Collection("UserAccounts").Document(userAccountData.userEmail).Collection("MyAccounts").Document(userAccountData.Name);
                        await documentReference_for_deletion.DeleteAsync();

                        //change the result of the account deletion variable
                        is_account_deleted = true;
                    }
                }
           
            }
            catch (Exception ex)
            {
                CustomException customException = new CustomException();
                customException.errorTitleName = "Error Occured while deletion of this Account";
                customException.errorMessageToUser = ex.Message;
                throw new FaultException<CustomException>(customException);
            }

            //return the result of this operation
            return is_account_deleted;
        }

        public void DoWork()
        {
        }

        public async Task<DBAccountData[]> getAllAccounts(string email_Address)
        {
            //connect to the database
            bool connection_result = connectToFirebase();

            //create the array which will be returned to the user as the output of this operation
            DBAccountData[] dBAccountData = null;
            try
            {
                //connection is succesfull
                if (connection_result)
                {
                    //Query for all the account 
                    Query allAccounts = db.Collection("UserAccounts").Document(email_Address).Collection("MyAccounts");

                    //get the sanpshots for the collections
                    QuerySnapshot allAccountsSnapshots = await allAccounts.GetSnapshotAsync();

                    //to variable represensts the number of accounts
                    int number_of_accounts = allAccountsSnapshots.Count;
                    //create the array for the account data
                    dBAccountData = new DBAccountData[number_of_accounts];

                    //incremeant varibale 
                    int i = 0;
                    foreach (DocumentSnapshot snap in allAccountsSnapshots.Documents)
                    {
                        dBAccountData[i] = new DBAccountData();
                        dBAccountData[i] = snap.ConvertTo<DBAccountData>();
                        i++;

                    }

                }

            }
            catch(Exception ex)
            {
                CustomException customException = new CustomException();
                customException.errorTitleName="Error while getting the data from the database";
                customException.errorMessageToUser = ex.Message;
                throw new FaultException<CustomException>(customException);
                    
            }

            return dBAccountData;
        }

        public async Task<DBAccountData> requestDecryption(string email_Address, string accountName)
        {
            DBAccountData dBAccountData = null;

            //connect to the firestoredatabase
            bool connection_Result = connectToFirebase();

            try
            {
                if (connection_Result)
                {
                    DocumentReference documentReference = db.Collection("UserAccounts").Document(email_Address).Collection("MyAccounts").Document(accountName);
                    DocumentSnapshot documentSnapshot = await documentReference.GetSnapshotAsync();

                    if (documentSnapshot.Exists)
                    {
                        //first of all encrypt the user Data 
                        //Get the User Encryption Key from the firestore database
                        DocumentReference getUserKey = db.Collection("UserEncryptionKeys").Document(email_Address);

                        //get the snapshot for the same
                        DocumentSnapshot snapshot = await getUserKey.GetSnapshotAsync();

                        if (snapshot.Exists)
                        {
                            string keyName = "";
                            Dictionary<string, object> userKeyDict = snapshot.ToDictionary();
                            foreach (KeyValuePair<string, object> pair in userKeyDict)
                            {
                                if (pair.Key == "userKeyName")
                                {
                                    keyName = pair.Value.ToString();
                                    break;
                                }
                            }

                            //Convert to the DBUserEncryption
                            //DBUserEncryptionKeys dBUserEncryptionKeys = snapshot.ConvertTo<DBUserEncryptionKeys>();
                            //string key_Name_For_Encryption = dBUserEncryptionKeys.userKeyName;

                            //Create the client for the azure KeyVault Access
                            var client = new KeyClient(vaultUri: new Uri(KeyVaultUrl), credential: new ClientSecretCredential(tenantId, clientid, client_secret));

                            //Retrive the key from the azure Key Vault
                            KeyVaultKey key = await client.GetKeyAsync(keyName);

                            //Now Creating the Crypto Client for the Encryption of the user Data
                            var cryptoClient = new CryptographyClient(keyId: key.Id, credential: new ClientSecretCredential(tenantId, clientid, client_secret));

                             //get the Account Details
                            DocumentReference documentReference_for_getting_data = db.Collection("UserAccounts").Document(email_Address).Collection("MyAccounts").Document(accountName);

                            //get the snapshots for the document
                            DocumentSnapshot documentSnapshot_for_getting_data = await documentReference_for_getting_data.GetSnapshotAsync();

                            if (documentSnapshot_for_getting_data.Exists)
                            {
                                //intiliaze the AccountData object and conver the snapshot to the Account Data object 
                                dBAccountData = new DBAccountData();
                                dBAccountData = documentSnapshot_for_getting_data.ConvertTo<DBAccountData>();

                                //perform the decryption
                                DecryptResult decryptResult_for_userName = await cryptoClient.DecryptAsync(EncryptionAlgorithm.RsaOaep, Convert.FromBase64String(dBAccountData.accountUserName));
                                DecryptResult decryptResult_for_password = await cryptoClient.DecryptAsync(EncryptionAlgorithm.RsaOaep, Convert.FromBase64String(dBAccountData.accountPassword));

                                //convert decrypted result to the string 
                                string decrypted_username_plaintext = Encoding.UTF8.GetString(decryptResult_for_userName.Plaintext);
                                string decrypted_password_plaintext = Encoding.UTF8.GetString(decryptResult_for_password.Plaintext);

                                //chnage the decrypted string to the plaintext 
                                dBAccountData.accountUserName = decrypted_username_plaintext;
                                dBAccountData.accountPassword = decrypted_password_plaintext;



                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                CustomException customException = new CustomException();
                customException.errorTitleName = "Error Occured while performing the decryption of the Username ans Password";
                customException.errorMessageToUser = ex.Message;
                throw new FaultException<CustomException>(customException);

            }
            return dBAccountData;
        }

        public async Task<bool> updateAccount(UserAccountData userAccountData)
        {
            bool is_account_updated = false;

            //connect to the database
            bool connection_result = connectToFirebase();
            try
            {

                //if connection is succesfull
                if (connection_result)
                {
                    bool accountExists = false;

                    //Query to check whether the account with same acoount name exists or not
                    Query checkIfAccountExists = db.Collection("UserAccounts").Document(userAccountData.userEmail).Collection("MyAccounts");

                    //if the object is not null means the user has some account in the database
                    if (checkIfAccountExists != null)
                    {
                        //get the snapshot of all the collection for the account name
                        QuerySnapshot snaps = await checkIfAccountExists.GetSnapshotAsync();

                        //Iterate through the collection and check for the same account name
                        foreach (DocumentSnapshot snap in snaps)
                        {
                            if (snap.Exists)
                            {
                                //remove all the white spaces characters from the string
                                string snapId = snap.Id.ToString().Replace(" ", "").ToLower();
                                string userAccountName = userAccountData.Name.ToString().Replace(" ", "").ToLower();

                                //compare the string 
                                if (snapId.Equals(userAccountName))
                                {
                                    accountExists = true;
                                }
                            }
                        }
                    }
                    if (accountExists)
                    {
                        //update the account data
                        //first of all encrypt the user Data 
                        //Get the User Encryption Key from the firestore database
                        DocumentReference getUserKey = db.Collection("UserEncryptionKeys").Document(userAccountData.userEmail);

                        //get the snapshot for the same
                        DocumentSnapshot snapshot = await getUserKey.GetSnapshotAsync();

                        if (snapshot.Exists)
                        {
                            string keyName = "";
                            Dictionary<string, object> userKeyDict = snapshot.ToDictionary();
                            foreach (KeyValuePair<string, object> pair in userKeyDict)
                            {
                                if (pair.Key == "userKeyName")
                                {
                                    keyName = pair.Value.ToString();
                                    break;
                                }
                            }

                            //Convert to the DBUserEncryption
                            //DBUserEncryptionKeys dBUserEncryptionKeys = snapshot.ConvertTo<DBUserEncryptionKeys>();
                            //string key_Name_For_Encryption = dBUserEncryptionKeys.userKeyName;

                            //Create the client for the azure KeyVault Access
                            var client = new KeyClient(vaultUri: new Uri(KeyVaultUrl), credential: new ClientSecretCredential(tenantId, clientid, client_secret));

                            //Retrive the key from the azure Key Vault
                            KeyVaultKey key = await client.GetKeyAsync(keyName);

                            //Now Creating the Crypto Client for the Encryption of the user Data
                            var cryptoClient = new CryptographyClient(keyId: key.Id, credential: new ClientSecretCredential(tenantId, clientid, client_secret));

                            //Convert the Data into the byte array
                            byte[] newAccountUserName = Encoding.UTF8.GetBytes(userAccountData.UserName);
                            byte[] newAccountPassword = Encoding.UTF8.GetBytes(userAccountData.Password);

                            //Create the EncryptionResult Object to perform the encryption on the user data
                            EncryptResult encryptResult_for_userName = await cryptoClient.EncryptAsync(EncryptionAlgorithm.RsaOaep, newAccountUserName);
                            EncryptResult encryptResult_for_password = await cryptoClient.EncryptAsync(EncryptionAlgorithm.RsaOaep, newAccountPassword);

                            //Retrive the Cipher text from this Encryption Result objects
                            string newAccountUserNameCipherText = Convert.ToBase64String(encryptResult_for_userName.Ciphertext);
                            string newAccountPasswordCipherText = Convert.ToBase64String(encryptResult_for_password.Ciphertext);

                            //add the account in the database
                            DocumentReference documentReference = db.Collection("UserAccounts").Document(userAccountData.userEmail).Collection("MyAccounts").Document(userAccountData.Name);

                            //Create th Dictonary to add the data
                            Dictionary<string, object> keyValuePairs = new Dictionary<string, object>()
                                {
                                    {
                                        "accountName",userAccountData.Name
                                    },
                                    {
                                        "accountUserName",newAccountUserNameCipherText
                                    },
                                    {
                                        "accountPassword",newAccountPasswordCipherText
                                    }
                                };

                            //Synchronously add the new Account to the firestore database
                            await documentReference.UpdateAsync(keyValuePairs);

                            is_account_updated = true;
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                CustomException customException = new CustomException();
                customException.errorTitleName = "Error Occured while deletion of this Account";
                customException.errorMessageToUser = ex.Message;
                throw new FaultException<CustomException>(customException);
            }
            finally
            {
                
            }
            //return the result of this operation
            return is_account_updated;
        }
    }
}
