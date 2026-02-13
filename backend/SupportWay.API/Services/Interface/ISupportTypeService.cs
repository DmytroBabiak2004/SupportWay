using SupportWay.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace SupportWay.Services.Interfaces
{
    public interface ISupportTypeService
    {
        Task<IEnumerable<SupportType>> GetAllAsync();
        Task<SupportType> GetByIdAsync(Guid id);
        Task AddAsync(SupportType type);
        Task UpdateAsync(SupportType type);
        Task DeleteAsync(Guid id);
    }
}