using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;
using OdooIntegration.Interfaces;
using OdooIntegration.Models;

namespace OdooIntegration.Services
{
    public class OdooService : IOdooService
    {
        private readonly OdooConnectionInfo _connectionInfo;
        private readonly ILogger<OdooService> _logger;
        private readonly CustomXmlRpcClient _xmlRpcClient;
        private int _uid;

        public OdooService(OdooConnectionInfo connectionInfo, ILogger<OdooService> logger, CustomXmlRpcClient xmlRpcClient)
        {
            _connectionInfo = connectionInfo;
            _logger = logger;
            _xmlRpcClient = xmlRpcClient;

            _logger.LogInformation("Initializing OdooService");
        }

        public async Task<bool> Authenticate()
        {
            _logger.LogInformation("Authenticating with Odoo server");
            try
            {
                var url = $"{_connectionInfo.Url}/xmlrpc/2/common";
                _uid = await _xmlRpcClient.ExecuteMethodAsync<int>(url, "authenticate",
                    _connectionInfo.Database, _connectionInfo.Username, _connectionInfo.Password, new object[] { });
                
                if (_uid <= 0)
                {
                    _logger.LogError("Authentication failed. Invalid user ID received");
                    return false;
                }
                _logger.LogInformation($"Authentication successful. User ID: {_uid}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Authentication failed");
                return false;
            }
        }

        public async Task<bool> TestConnection()
        {
            _logger.LogInformation("Testing connection to Odoo server");
            try
            {
                var url = $"{_connectionInfo.Url}/xmlrpc/2/common";
                var result = await _xmlRpcClient.ExecuteMethodAsync<object>(url, "version");
                _logger.LogInformation("Connection test successful");
                return result != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Connection test failed");
                return false;
            }
        }

        public async Task<Dictionary<string, object>> GetUserInfo()
        {
            _logger.LogInformation("Fetching user information");
            var result = await Execute<List<Dictionary<string, object>>>("res.users", "read", new object[] { _uid }, new[] { "name", "login" });
            return result.FirstOrDefault() ?? new Dictionary<string, object>();
        }

        public async Task<int> CreateRecord(string modelName, Dictionary<string, object> values)
        {
            _logger.LogInformation($"Creating new record in model: {modelName}");
            var result = await Execute<int>(modelName, "create", new object[] { values });
            return result;
        }

        public async Task DeleteRecord(string modelName, int id)
        {
            _logger.LogInformation($"Deleting record {id} from model: {modelName}");
            await Execute<bool>(modelName, "unlink", new object[] { new[] { id } });
        }

        public async Task<Dictionary<string, object>> ReadRecord(string modelName, int id, string[] fields)
        {
            _logger.LogInformation($"Reading record {id} from model: {modelName}");
            var result = await Execute<List<Dictionary<string, object>>>(modelName, "read", new object[] { new[] { id }, fields });
            return result.FirstOrDefault() ?? new Dictionary<string, object>();
        }

        public async Task<List<Dictionary<string, object>>> SearchRead(string modelName, object[] domain, string[] fields, int limit = 0)
        {
            _logger.LogInformation($"Performing search_read on model: {modelName}");
            var result = await Execute<object>(modelName, "search_read", 
                new object[] { domain, new Dictionary<string, object> { { "fields", fields }, { "limit", limit } } });

            if (result is List<object> listResult)
            {
                return listResult.Select(item => item as Dictionary<string, object> ?? new Dictionary<string, object>()).ToList();
            }
            else if (result is Dictionary<string, object> dictResult)
            {
                return new List<Dictionary<string, object>> { dictResult };
            }
            else if (result is object[] arrayResult)
            {
                return arrayResult.Select(item => item as Dictionary<string, object> ?? new Dictionary<string, object>()).ToList();
            }
            else if (result == null)
            {
                _logger.LogWarning("Search_read returned null result");
                return new List<Dictionary<string, object>>();
            }
            else
            {
                _logger.LogWarning($"Unexpected result type from search_read: {result.GetType().Name}");
                return new List<Dictionary<string, object>>();
            }
        }

        public async Task UpdateRecord(string modelName, int id, Dictionary<string, object> values)
        {
            _logger.LogInformation($"Updating record {id} in model: {modelName}");
            await Execute<bool>(modelName, "write", new object[] { new[] { id }, values });
        }

        public async Task<T> Execute<T>(string model, string method, params object[] args)
        {
            _logger.LogInformation($"Executing method: {method} on model: {model}");
            var url = $"{_connectionInfo.Url}/xmlrpc/2/object";
            var result = await _xmlRpcClient.ExecuteMethodAsync<object>(url, "execute_kw",
                _connectionInfo.Database, _uid, _connectionInfo.Password, model, method, args);

            if (result is T typedResult)
            {
                return typedResult;
            }
            else if (typeof(T) == typeof(List<Dictionary<string, object>>))
            {
                if (result is List<object> listResult)
                {
                    return (T)(object)listResult.Select(item => item as Dictionary<string, object> ?? new Dictionary<string, object>()).ToList();
                }
                else if (result is Dictionary<string, object> dictResult)
                {
                    return (T)(object)new List<Dictionary<string, object>> { dictResult };
                }
                else if (result is object[] arrayResult)
                {
                    return (T)(object)arrayResult.Select(item => item as Dictionary<string, object> ?? new Dictionary<string, object>()).ToList();
                }
            }

            _logger.LogWarning($"Unexpected result type from {method}: {result?.GetType().Name ?? "null"}");
            throw new InvalidCastException($"Cannot cast {result?.GetType().Name ?? "null"} to {typeof(T).Name}");
        }

        public async Task<object> Execute(string model, string method, params object[] args)
        {
            return await Execute<object>(model, method, args);
        }
    }
}