using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace PHONE_STORE.Application.Dtos;

        public record CategoryDto(long Id, long? ParentId, string Name, string Slug, int SortOrder, bool IsActive);
        public record CategoryCreateDto(long? ParentId, string Name, string? Slug, int SortOrder, bool IsActive);
        public record CategoryUpdateDto(long? ParentId, string Name, string? Slug, int SortOrder, bool IsActive);
        public record CategoryOptionDto(long Id, string Label); // cho dropdown Parent
        //public record PagedResult<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);


