using System;
using System.Collections.Generic;
using System.Linq;
using OdooIntegration.Interfaces;
using CookComputing.XmlRpc;

namespace OdooIntegration.Services
{
    public class OdooService : IOdooService
    {
        private readonly OdooXmlRpcProxy _proxy;
        private readonly string _dbName;
        private readonly string _username;
        private readonly string _password;
        private readonly int _uid;

        public OdooService(string url, string dbName, string username, string password)
        {
            _proxy = new OdooXmlRpcProxy(url);
            _dbName = dbName;
            _username = username;
            _password = password;
            _uid = Authenticate();
        }

        private int Authenticate()
        {
            object[] args = new object[] { _dbName, _username, _password, new object() };
            return (int)_proxy.Execute("common", "authenticate", args);
        }

        public int CreateRecord(string modelName, Dictionary<string, object> values)
        {
            object[] args = new object[] { _dbName, _uid, _password, modelName, "create", new object[] { values } };
            return (int)_proxy.Execute("object", "execute_kw", args);
        }

        public void DeleteRecord(string modelName, int id)
        {
            object[] args = new object[] { _dbName, _uid, _password, modelName, "unlink", new object[] { new int[] { id } } };
            _proxy.Execute("object", "execute_kw", args);
        }

        public Dictionary<string, object> ReadRecord(string modelName, int id, string[] fields)
        {
            object[] args = new object[] 
            { 
                _dbName, _uid, _password, modelName, "read", 
                new object[] { new int[] { id } },
                new Dictionary<string, object> { { "fields", fields } }
            };
            var result = (object[])_proxy.Execute("object", "execute_kw", args);
            return ((IDictionary<string, object>)result[0]).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public List<Dictionary<string, object>> SearchRead(string modelName, object[] domain, string[] fields, int limit = 0)
        {
            object[] args = new object[] 
            { 
                _dbName, _uid, _password, modelName, "search_read",
                new object[] { domain },
                new Dictionary<string, object> { { "fields", fields }, { "limit", limit } }
            };
            var result = (object[])_proxy.Execute("object", "execute_kw", args);
            return result.Select(r => ((IDictionary<string, object>)r).ToDictionary(kvp => kvp.Key, kvp => kvp.Value)).ToList();
        }

        public void UpdateRecord(string modelName, int id, Dictionary<string, object> values)
        {
            object[] args = new object[] { _dbName, _uid, _password, modelName, "write", new object[] { new int[] { id }, values } };
            _proxy.Execute("object", "execute_kw", args);
        }
    }

    public class OdooXmlRpcProxy : XmlRpcClientProtocol
    {
        public OdooXmlRpcProxy(string url)
        {
            Url = url;
        }

        [XmlRpcMethod("execute_kw")]
        public object Execute(string service, string method, object[] args)
        {
            return Invoke("execute_kw", new object[] { service, method, args });
        }
    }
}