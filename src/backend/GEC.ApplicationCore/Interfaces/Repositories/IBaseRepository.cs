using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GEC.ApplicationCore.Interfaces.Repositories
{

    public interface IBaseRepository<T> where T : class
    {
        public Task<T?> GetByIdAsync(Guid id);
        void Add(T entity);
        public void Update(T entity);
        void Remove(T entity);

    }
}