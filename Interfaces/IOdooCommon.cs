// IOdooCommon.cs
namespace OdooIntegration.Interfaces
{
    public interface IOdooCommon
    {
        int Login(string db, string username, string password);
        // Other methods based on the common service
    }
}

