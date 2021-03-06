﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Web;
using System.Net.Security;

namespace SecureDesk_WCF_Service.Models
{
    [MessageContract(IsWrapped = true, WrapperName = "UserOtpVerification")]
    //will be used to send the otp to the user after the registration fields are obtinaed from the registration form
    public class UserOtpVerification
    {

        private string otp;
        private string email_address;
       

        [MessageHeader(Name = "One Time Password for Verification")]
        public string OTP
        {
            get;
            set;
        }

        [MessageHeader(Name = "User_Email_Address")]
        public string Email_Address
        {
            get;
            set;
        } 


        

    }
}