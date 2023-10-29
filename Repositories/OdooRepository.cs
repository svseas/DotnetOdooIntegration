using System.Collections.Generic;
using OdooIntegration.Interfaces;
using OdooIntegration.Models;

namespace OdooIntegration.Repositories
{
    public class OdooRepository<TEntity> : IOdooRepository<TEntity> where TEntity : class, new()
    {
        private readonly string _modelName;
        private readonly IOdooService _odooService;

        public OdooRepository(string modelName, IOdooService odooService)
        {
            _modelName = modelName;
            _odooService = odooService;
        }

        public int Create(TEntity entity)
        {
            Dictionary<string, object> values = ConvertEntityToDictionary(entity);
            return _odooService.CreateRecord(_modelName, values);
        }

        // Other method implementations, including ConvertEntityToDictionary
    }
}