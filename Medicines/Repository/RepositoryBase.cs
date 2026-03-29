using Medicines.Context;
using Medicines.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Principal;
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
        public void Update(T entity)
        {
            var local = _context.Set<T>().Local
                            .FirstOrDefault(e => ((IEntity)e).Id == ((IEntity)entity).Id);

            if (local != null)
            {
                _context.Entry(local).CurrentValues.SetValues(entity);
            }
            else
            {
                _context.Attach(entity);
                _context.Entry(entity).State = EntityState.Modified;
            }
        }

        public void Delete(T entity)
        {
            var local = _context.Set<T>().Local
                            .FirstOrDefault(e => ((IEntity)e).Id == ((IEntity)entity).Id);

            if (local != null)
            {
                _context.Set<T>().Remove(local);
            }
            else
            {
                _context.Attach(entity);
                _context.Set<T>().Remove(entity);
            }
        }

        public IQueryable<T> FindAll() => _context.Set<T>().AsNoTracking();
        public IQueryable<T> FindByCondition(Expression<Func<T, bool>> conditionExpression)
            => _context
                .Set<T>()
                .AsNoTracking()
                .Where(conditionExpression);
    }
}
