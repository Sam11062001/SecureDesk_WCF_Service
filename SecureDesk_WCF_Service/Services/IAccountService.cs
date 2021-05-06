using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using SecureDesk_WCF_Service.Models;
namespace SecureDesk_WCF_Service.Services
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IAccountService" in both code and config file together.
    [ServiceContract]
    public interface IAccountService
    {
        //This operation Contract helps in adding the new account for the user
        [OperationContract]
        [FaultContract(typeof(CustomException))]
        Task<bool> addAccount(UserAccountData userAccountData);

        //This operation Contract Helps in update account data
        [OperationContract]
        [FaultContract(typeof(CustomException))]
        Task<bool> updateAccount(UserAccountData userAccountData);

        //This Operation Contract helps in removing the existing account 
        [OperationContract]
        [FaultContract(typeof(CustomException))]
        Task<bool> deleteAccount(UserAccountData userAccountData);

        //This Operation retuns all the current account from the database
        [OperationContract]
        [FaultContract(typeof(CustomException))]
        Task<UserAccountData[]> getAllAccounts(string email_Address);

        //This operation performs the decryption for the requested account
        [OperationContract]
        [FaultContract(typeof(CustomException))]
        Task<UserAccountData> requestDecryption(string email_Address, string accountName);

        
    }

}
