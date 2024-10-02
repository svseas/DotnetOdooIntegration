using System.Collections.Generic;

namespace OdooIntegration.Interfaces
{
    public interface IOdooService
    {
        int CreateRecord(string modelName, Dictionary<string, object> values);
        void DeleteRecord(string modelName, int id);
        Dictionary<string, object> ReadRecord(string modelName, int id, string[] fields);
        List<Dictionary<string, object>> SearchRead(string modelName, object[] domain, string[] fields, int limit = 0);
        void UpdateRecord(string modelName, int id, Dictionary<string, object> values);
    }
}