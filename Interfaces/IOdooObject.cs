// IOdooObject.cs
using System.Collections.Generic;

namespace OdooIntegration.Interfaces
{
    public interface IOdooObject
    {
        object Execute(string dbName, int uid, string password, string model, string method, params object[] args);
        // Other methods based on the object service
    }
}