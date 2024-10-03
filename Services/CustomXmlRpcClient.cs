using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;
using Microsoft.Extensions.Logging;

namespace OdooIntegration.Services
{
    public class CustomXmlRpcClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CustomXmlRpcClient> _logger;

        public CustomXmlRpcClient(ILogger<CustomXmlRpcClient> logger)
        {
            _httpClient = new HttpClient();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<T> ExecuteMethodAsync<T>(string url, string methodName, params object[] parameters)
        {
            try
            {
                if (string.IsNullOrEmpty(url))
                {
                    throw new ArgumentNullException(nameof(url));
                }

                if (string.IsNullOrEmpty(methodName))
                {
                    throw new ArgumentNullException(nameof(methodName));
                }

                var request = CreateXmlRpcRequest(methodName, parameters);
                var content = new StringContent(request, Encoding.UTF8, "text/xml");

                var response = await _httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                return ParseXmlRpcResponse<T>(responseContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error executing XML-RPC method: {methodName}");
                throw;
            }
        }

        private string CreateXmlRpcRequest(string methodName, object[] parameters)
        {
            try
            {
                var doc = new XDocument(
                    new XElement("methodCall",
                        new XElement("methodName", methodName),
                        new XElement("params",
                            from param in parameters
                            select new XElement("param",
                                new XElement("value",
                                    SerializeValue(param))))));

                return doc.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating XML-RPC request");
                throw;
            }
        }

        private XElement SerializeValue(object value)
        {
            if (value == null)
            {
                return new XElement("nil");
            }

            try
            {
                switch (value)
                {
                    case int i:
                        return new XElement("int", i);
                    case string s:
                        return new XElement("string", s);
                    case bool b:
                        return new XElement("boolean", b ? "1" : "0");
                    case double d:
                        return new XElement("double", d);
                    case DateTime dt:
                        return new XElement("dateTime.iso8601", dt.ToString("yyyyMMddTHH:mm:ss"));
                    case object[] arr:
                        return new XElement("array",
                            new XElement("data",
                                from item in arr
                                select new XElement("value", SerializeValue(item))));
                    case Dictionary<string, object> dict:
                        return new XElement("struct",
                            from entry in dict
                            select new XElement("member",
                                new XElement("name", entry.Key),
                                new XElement("value", SerializeValue(entry.Value))));
                    default:
                        throw new ArgumentException($"Unsupported type: {value.GetType()}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error serializing value of type {value?.GetType()}");
                throw;
            }
        }

        private T ParseXmlRpcResponse<T>(string xmlResponse)
        {
            try
            {
                if (string.IsNullOrEmpty(xmlResponse))
                {
                    throw new ArgumentNullException(nameof(xmlResponse));
                }

                var doc = XDocument.Parse(xmlResponse);
                var valueElement = doc.Descendants("value").FirstOrDefault();
                
                if (valueElement == null)
                {
                    throw new XmlException("Invalid XML-RPC response: missing value element");
                }

                return (T)DeserializeValue(valueElement);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing XML-RPC response");
                throw;
            }
        }

        private object DeserializeValue(XElement valueElement)
        {
            try
            {
                var typeElement = valueElement.Elements().FirstOrDefault();
                if (typeElement == null)
                {
                    return valueElement.Value; // String value
                }

                switch (typeElement.Name.LocalName.ToLowerInvariant())
                {
                    case "int":
                    case "i4":
                        return int.Parse(typeElement.Value);
                    case "boolean":
                        return typeElement.Value == "1";
                    case "double":
                        return double.Parse(typeElement.Value);
                    case "datetime.iso8601":
                        return DateTime.ParseExact(typeElement.Value, "yyyyMMddTHH:mm:ss", null);
                    case "array":
                        return typeElement.Element("data")?.Elements("value")
                            .Select(v => DeserializeValue(v)).ToArray();
                    case "struct":
                        return typeElement.Elements("member")
                            .ToDictionary(
                                m => m.Element("name")?.Value,
                                m => DeserializeValue(m.Element("value")));
                    case "nil":
                        return null;
                    default:
                        return typeElement.Value; // Fallback to string
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deserializing XML-RPC value");
                throw;
            }
        }
    }
}