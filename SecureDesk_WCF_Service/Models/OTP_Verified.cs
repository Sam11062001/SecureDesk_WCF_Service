using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
namespace SecureDesk_WCF_Service.Models
{
    [MessageContract(IsWrapped =true,WrapperName ="User OTP Verified by Service")]
    public class OTP_Verified
    {
        private bool verification_result;
        
        [MessageHeader(Name ="User OTP Verification Result")]
        public bool Verification_Result
        {
            get;
            set;
        }
    }
}