using System;
using System.Collections.Generic;
using System.Linq;
using OdooIntegration.Interfaces;
using System.Reflection;

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

        public TEntity GetById(int id)
        {
            string[] fields = GetEntityFields();
            var record = _odooService.ReadRecord(_modelName, id, fields);
            return ConvertDictionaryToEntity(record);
        }

        public IEnumerable<TEntity> Search(object[] domain)
        {
            string[] fields = GetEntityFields();
            var records = _odooService.SearchRead(_modelName, domain, fields);
            return records.Select(ConvertDictionaryToEntity);
        }

        public void Update(int id, TEntity entity)
        {
            Dictionary<string, object> values = ConvertEntityToDictionary(entity);
            _odooService.UpdateRecord(_modelName, id, values);
        }

        public void Delete(int id)
        {
            _odooService.DeleteRecord(_modelName, id);
        }

        private Dictionary<string, object> ConvertEntityToDictionary(TEntity entity)
        {
            return entity.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(entity));
        }

        private TEntity ConvertDictionaryToEntity(Dictionary<string, object> dict)
        {
            var entity = new TEntity();
            var props = typeof(TEntity).GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (var prop in props)
            {
                if (dict.TryGetValue(prop.Name, out object value))
                {
                    prop.SetValue(entity, Convert.ChangeType(value, prop.PropertyType));
                }
            }

            return entity;
        }

        private string[] GetEntityFields()
        {
            return typeof(TEntity)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(p => p.Name)
                .ToArray();
        }
    }
}