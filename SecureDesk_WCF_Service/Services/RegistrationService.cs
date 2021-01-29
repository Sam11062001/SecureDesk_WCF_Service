using SecureDesk_WCF_Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace SecureDesk_WCF_Service
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface RegistrationService
    {
        [OperationContract]
        Task<string[]> getQuestions();

        [OperationContract]
        Boolean connectToFirebase();

        [OperationContract]
        void registerNewUser(UserRegister user);

        [OperationContract]
        void sendOTP(string email);

        [OperationContract]
        Task<OTP_Verified> verifyUser(UserOtpVerification otpObj);

        [OperationContract]
        Task<int> getSecurePin(string email);

        [OperationContract]
        Task<string> getUserQuestion(string email);

        [OperationContract]
        Task<Boolean> verifyAnswer(string email , string userAnswer);

        [OperationContract]
        void resetPassword(string email, string newPassword);
        // TODO: Add your service operations here
    }


    // Use a data contract as illustrated in the sample below to add composite types to service operations.
   
}
