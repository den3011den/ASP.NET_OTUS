using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
namespace PromoCodeFactory.DataAccess.Repositories
{
    public class InMemoryRepository<T> : IRepository<T> where T : BaseEntity
    {
        protected IEnumerable<T> Data { get; set; }

        public InMemoryRepository(IEnumerable<T> data)
        {
            Data = data;
        }

        public Task<IEnumerable<T>> GetAllAsync()
        {
            return Task.FromResult(Data);
        }

        public Task<T> GetByIdAsync(Guid id)
        {
            var objAlreadyExist = Data.FirstOrDefault(x => x.Id == id);
            if (objAlreadyExist != null)
                return Task.FromResult(objAlreadyExist);
            else
                return Task.FromResult<T>(null);
        }

        public Task<T> CreateAsync([NotNull] T obj)
        {
            var objAlreadyExist = Data.FirstOrDefault(x => x.Id == obj.Id);
            if (objAlreadyExist != null)
            {
                return Task.FromResult<T>(null);
            }
            Data = Data.Append(obj);
            return Task.FromResult(Data.FirstOrDefault(x => x.Id == obj.Id));
        }

        public Task<T> UpdateAsync([NotNull] T obj)
        {
            var objAlreadyExist = Data.FirstOrDefault(x => x.Id == obj.Id);
            if (objAlreadyExist == null)
            {
                return Task.FromResult<T>(null);

            }
            var propertiesList = objAlreadyExist.GetType().GetProperties();
            foreach (var property in propertiesList)
            {
                property.SetValue(objAlreadyExist, property.GetValue(obj));
            }
            return Task.FromResult(Data.FirstOrDefault(x => x.Id == obj.Id));
        }

        public Task<T> DeleteAsync(Guid id)
        {
            var objAlreadyExist = Data.FirstOrDefault(x => x.Id == id);
            if (objAlreadyExist == null)
            {
                return Task.FromResult<T>(null);
            }
            Data = Data.Where(x => x.Id != id);
            if (objAlreadyExist == null)
            {
                return Task.FromResult<T>(null);
            }
            return Task.FromResult(objAlreadyExist);
        }
    }
}