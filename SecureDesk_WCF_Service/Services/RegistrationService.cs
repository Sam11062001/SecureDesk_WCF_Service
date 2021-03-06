﻿using SecureDesk_WCF_Service.Models;
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
        [FaultContract(typeof(CustomException))]
        Task<string[]> getQuestions();

        [OperationContract]
        [FaultContract(typeof(CustomException))]
        Boolean connectToFirebase();

        [OperationContract]
        [FaultContract(typeof(CustomException))]
        void registerNewUser(UserRegister user);

        [OperationContract]
        [FaultContract(typeof(CustomException))]
        Task<bool> sendOTP(string email,string userName,bool isForgetPassword);

        [OperationContract]
        [FaultContract(typeof(CustomException))]
        Task<OTP_Verified> verifyUser(UserOtpVerification otpObj);
        
        [OperationContract]
        [FaultContract(typeof(CustomException))]
        Task<string> getUserQuestion(string email);

        [OperationContract]
        [FaultContract(typeof(CustomException))]
        Task<Boolean> verifyAnswer(string email , string userAnswer);

        [OperationContract]
        [FaultContract(typeof(CustomException))]
        void resetPassword(string email, string newPassword);

        [OperationContract]
        [FaultContract(typeof(CustomException))]
        Task<bool> isUnique(string email);

        [OperationContract]
        [FaultContract(typeof(CustomException))]
        Task<OTP_Verified> verifyForgetPassword(UserOtpVerification otpObj);


        // TODO: Add your service operations here
    }


    // Use a data contract as illustrated in the sample below to add composite types to service operations.

}
