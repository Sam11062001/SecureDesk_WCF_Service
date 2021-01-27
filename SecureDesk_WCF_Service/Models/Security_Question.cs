using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SecureDesk_WCF_Service.Models
{
    public class Security_Question
    {
        private int id;
        private string question;

        public int ID
        {
            get;
            set;
        }
        public string Question
        {
            get;
            set;
        }
    }
}