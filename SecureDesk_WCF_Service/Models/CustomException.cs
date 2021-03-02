using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using System.Runtime.Serialization;
namespace SecureDesk_WCF_Service.Models
{
    [DataContract]
    public class CustomException
    {
        private string title;
        private string errorMessage;

        [DataMember]
        public string errorTitleName
        {
            get { return title; }
            set { this.title = value; }
        }
        [DataMember]
        public string errorMessageToUser
        {
            get { return errorMessage; }
            set { this.errorMessage = value; }
        }
    }
}