﻿using PromoCodeFactory.Core.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PromoCodeFactory.Core.Abstractions.Repositories
{
    public interface IRepository<T> where T : BaseEntity
    {
        Task<IEnumerable<T>> GetAllAsync();

        Task<T> GetByIdAsync(Guid id);

        Task<T> CreateAsync(T obj);

        Task<T> UpdateAsync(T obj);

        Task<T> DeleteAsync(Guid id);

    }
}