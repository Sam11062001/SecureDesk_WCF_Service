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
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IUserAccountService" in both code and config file together.
    [ServiceContract]
    public interface IUserAccountService
    {
        [OperationContract]
        Task<User> getAccountInfo(string email);

        [OperationContract]
        Task<bool> isAuthorized(string email, int accessPin);

        [OperationContract]
        Task<User> updateUserAccount(User updatedUserInfo);
    }
}
