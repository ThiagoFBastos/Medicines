using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Medicines.Interfaces
{
    public interface IRepositoryBase<T>
    {
        void Add(T entidade);
        void Update(T entidade);
        void Delete(T entidade);
        IQueryable<T> FindAll();
        IQueryable<T> FindByCondition(Expression<Func<T, bool>> conditionExpression);
    }
}
