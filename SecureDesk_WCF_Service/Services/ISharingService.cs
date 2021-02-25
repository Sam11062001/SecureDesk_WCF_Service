﻿using SecureDesk_WCF_Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace SecureDesk_WCF_Service.Services
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "ISharingService" in both code and config file together.
    [ServiceContract]
    public interface ISharingService
    {
        [OperationContract]
        Boolean connectToFirebase();

        [OperationContract]
        void shareDocument(DocumentData documentData, string sharedBy, string sharedTo);

        [OperationContract]
        void getSharedDocument( string email );
    }
}
