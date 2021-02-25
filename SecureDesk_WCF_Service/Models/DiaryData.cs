using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace SecureDesk_WCF_Service.Models
{
    [DataContract]
    public class DiaryData
    {
        [DataMember]
        public string date;

        [DataMember]
        public string title;

        [DataMember]
        public string fileLink;
    }
}