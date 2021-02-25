using SecureDesk_WCF_Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace SecureDesk_WCF_Service.Services
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IPersonalDiary" in both code and config file together.
    [ServiceContract]
    public interface IPersonalDiary
    {

        [OperationContract]
        Boolean connectToFirebase();

        [OperationContract]
        void UploadDayThought( string date , string title , string content );

        [OperationContract]
        void addDiary(string link, string date , string title );

        [OperationContract]
        void getDiary(string link );

        [OperationContract]
        Task<int> getDiaryDocumentCount(string email);
        
        [OperationContract]
        Task<DiaryData[]> getAllDiaryData( string email );



    }
}
