using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace SecureDesk_WCF_Service.Models
{
    [DataContract]
    public class DocumentData
    {
        [DataMember]
        public string fileName;

        [DataMember]
        public string fileLink;
    }
}