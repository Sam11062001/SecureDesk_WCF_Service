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
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IUser" in both code and config file together.
    [ServiceContract]
    public interface IUser
    {
        [OperationContract]
        [FaultContract(typeof(CustomException))]
        Task<bool> isAuthorized(string email,int accessPin);

        [OperationContract]
        [FaultContract(typeof(CustomException))]
        Task<User> getAccountInfo(string email);
    }
}
