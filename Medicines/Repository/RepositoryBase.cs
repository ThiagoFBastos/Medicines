using Medicines.Context;
using Medicines.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Medicines.Repository
{
    public class RepositoryBase<T>: IRepositoryBase<T> where T : class
    {
        private readonly RepositoryContext _context;
        public RepositoryBase(RepositoryContext context)
        {
            _context = context;
        }

        public void Add(T entity) => _context.Set<T>().Add(entity);
        public void Update(T entity) => _context.Set<T>().Update(entity);
        public void Delete(T entity) => _context.Set<T>().Remove(entity);
        public IQueryable<T> FindAll() => _context.Set<T>().AsNoTracking();
        public IQueryable<T> FindByCondition(Expression<Func<T, bool>> conditionExpression)
            => _context
                .Set<T>()
                .Where(conditionExpression)
                .AsNoTracking();
    }
}
