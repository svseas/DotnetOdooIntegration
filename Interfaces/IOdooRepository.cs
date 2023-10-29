using System.Collections.Generic;
using OdooIntegration.Models;

namespace OdooIntegration.Interfaces
{
    public interface IOdooRepository<TEntity>
    {
        int Create(TEntity entity);
        TEntity GetById(int id);
        IEnumerable<TEntity> Search(object[] domain);
        void Update(int id, TEntity entity);
        void Delete(int id);
    }
}