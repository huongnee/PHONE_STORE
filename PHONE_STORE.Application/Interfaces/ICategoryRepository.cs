using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PHONE_STORE.Application.Dtos;

namespace PHONE_STORE.Application.Interfaces
{
    public interface ICategoryRepository
    {
        Task<PagedResult<CategoryDto>> SearchAsync(string? q, int page, int pageSize, CancellationToken ct);
        Task<CategoryDto?> GetAsync(long id, CancellationToken ct);
        Task<long> CreateAsync(CategoryCreateDto dto, CancellationToken ct);
        Task<bool> UpdateAsync(long id, CategoryUpdateDto dto, CancellationToken ct);
        Task<bool> DeleteAsync(long id, CancellationToken ct); // cấm xoá nếu còn con
        Task<bool> SlugExistsAsync(string slug, long? exceptId, CancellationToken ct);

        // options cho Parent (loại trừ chính nó và hậu duệ khi sửa)
        Task<List<CategoryOptionDto>> GetOptionsAsync(long? excludeId, CancellationToken ct);
    }

}


