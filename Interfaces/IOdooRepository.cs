using System.Collections.Generic;
using OdooIntegration.Models;

namespace OdooIntegration.Interfaces
{
public interface IOdooRepository<TEntity>
{
    Task<int> Create(TEntity entity);
    Task<TEntity> GetById(int id);
    Task<IEnumerable<TEntity>> Search(object[] domain);
    Task Update(int id, TEntity entity);
    Task Delete(int id);
    Task<bool> TestConnection();
    Task<Dictionary<string, object>> GetUserInfo();
}
    
}