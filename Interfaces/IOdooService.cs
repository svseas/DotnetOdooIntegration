using System.Collections.Generic;

namespace OdooIntegration.Interfaces
{
    public interface IOdooService
    {
        int CreateRecord(string modelName, Dictionary<string, object> values);
        void DeleteRecord(string modelName, int id);
        // Other methods as needed
    }
}