using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Azure.Identity;
using Azure.Security.KeyVault.Keys;
using SecureDesk_WCF_Service.Models;
namespace SecureDesk_WCF_Service.Algorithms
{
    public class AzureEncryptDecrypt
    {
        //creating the unique Guid from the Guid Class
        Guid guidObject;
        public async Task<string> createEncryptionRSAKey()
        { 
            try
            {
                //Get the new Guid
                guidObject = Guid.NewGuid();

                //Keyname to be stored in the database
                string keyName = guidObject.ToString();

                //Getting the configuration from the azure app configuration
                string KeyVaultUrl = System.Configuration.ConfigurationManager.AppSettings["SecureDeskKeyVaultUrl"];
                string clientid = System.Configuration.ConfigurationManager.AppSettings["SecureDeskAccessPoclicyClientId"];
                string tenantId = System.Configuration.ConfigurationManager.AppSettings["SecureDeskAccessPolicyTenantId"];
                string client_secret = System.Configuration.ConfigurationManager.AppSettings["SecureDeskAccessPolicyPassword"];

                //Create the Client for the KeyVault 
                var client = new KeyClient(vaultUri: new Uri(KeyVaultUrl), credential: new ClientSecretCredential(tenantId, clientid, client_secret));

                // Create a software RSA key with the KeyName variable             
                var rsaCreateKey = new CreateRsaKeyOptions(keyName, hardwareProtected: false);

                //Create and store the key
                KeyVaultKey rsaKey = await client.CreateRsaKeyAsync(rsaCreateKey);

                //return the key and save to the database
                return keyName;
            }
            
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
                
            }


        }
    }
}