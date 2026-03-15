using SupportWay.Data.DTOs;
using SupportWay.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SupportWay.Services.Interfaces
{
    public interface ISupportTypeService
    {
        Task<IEnumerable<SupportTypeDto>> GetAllAsync();
        Task<SupportTypeDto?> GetByIdAsync(Guid id);
        Task<SupportTypeDto> CreateAsync(CreateSupportTypeDto dto);
        Task UpdateAsync(Guid id, CreateSupportTypeDto dto);
        Task DeleteAsync(Guid id);
    }
}