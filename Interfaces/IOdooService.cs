using System.Collections.Generic;
using System.Threading.Tasks;

namespace OdooIntegration.Interfaces
{
    public interface IOdooService
    {
        Task<bool> TestConnection();

        Task<bool> Authenticate();
        Task<Dictionary<string, object>> GetUserInfo();
        Task<int> CreateRecord(string modelName, Dictionary<string, object> values);
        Task DeleteRecord(string modelName, int id);
        Task<Dictionary<string, object>> ReadRecord(string modelName, int id, string[] fields);
        Task<List<Dictionary<string, object>>> SearchRead(string modelName, object[] domain, string[] fields, int limit = 0);
        Task UpdateRecord(string modelName, int id, Dictionary<string, object> values);

        Task<object> Execute(string model, string method, params object[] args);
    }
}