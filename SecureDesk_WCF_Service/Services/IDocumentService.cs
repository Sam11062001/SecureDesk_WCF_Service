using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using SecureDesk_WCF_Service.Models;
using System.Threading.Tasks;

namespace SecureDesk_WCF_Service.Services
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IDocumentService" in both code and config file together.
    [ServiceContract]
    public interface IDocumentService
    {
        [OperationContract]
        void DoWork();

        [OperationContract]
        Boolean connectToFirebase();

        [OperationContract]
        void uploadDocument(byte[] fileByte, string fileName);

        [OperationContract]
        void addDocument(string link, string fileName);

        [OperationContract]
        Task<DocumentData[]> getAllDocumnetData(string email);
    }
}
