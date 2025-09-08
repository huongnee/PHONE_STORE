using Microsoft.EntityFrameworkCore;
using PHONE_STORE.Application.Dtos;
using PHONE_STORE.Application.Interfaces;
using PHONE_STORE.Infrastructure.Data;
using PHONE_STORE.Infrastructure.Entities;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace PHONE_STORE.Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly PhoneDbContext _db;
        public CategoryRepository(PhoneDbContext db) => _db = db;

        public async Task<PagedResult<CategoryDto>> SearchAsync(string? q, int page, int pageSize, CancellationToken ct)
        {
            var query = _db.Categories.AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
            {
                var s = q.Trim().ToUpperInvariant();
                query = query.Where(x => x.Name.ToUpper().Contains(s) || x.Slug.ToUpper().Contains(s));
            }

            var total = await query.CountAsync(ct);
            var items = await query
                .OrderBy(x => x.ParentId.HasValue ? 1 : 0)
                .ThenBy(x => x.SortOrder)
                .ThenBy(x => x.Name)
                .Select(c => new CategoryDto(c.Id, c.ParentId, c.Name, c.Slug, c.SortOrder, c.IsActive))
                .Skip((page - 1) * pageSize).Take(pageSize)
                .ToListAsync(ct);

            return new(items, total, page, pageSize);
        }

        public async Task<CategoryDto?> GetAsync(long id, CancellationToken ct)
            => await _db.Categories.Where(c => c.Id == id)
                .Select(c => new CategoryDto(c.Id, c.ParentId, c.Name, c.Slug, c.SortOrder, c.IsActive))
                .FirstOrDefaultAsync(ct);

        // ✅ đúng chữ ký interface
        public async Task<long> CreateAsync(CategoryCreateDto dto, CancellationToken ct)
        {
            // Oracle: tránh TRUE/FALSE -> dùng COUNT(*)
            if (dto.ParentId.HasValue)
            {
                var parentCount = await _db.Categories.CountAsync(x => x.Id == dto.ParentId.Value, ct);
                if (parentCount == 0) throw new InvalidOperationException("Parent không tồn tại.");
            }

            var slug = Slugify(string.IsNullOrWhiteSpace(dto.Slug) ? dto.Name : dto.Slug!);
            if (await SlugExistsAsync(slug, null, ct)) throw new InvalidOperationException("Slug đã tồn tại.");

            var c = new Category
            {
                ParentId = dto.ParentId,
                Name = dto.Name.Trim(),
                Slug = slug,
                SortOrder = dto.SortOrder,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };
            _db.Categories.Add(c);
            await _db.SaveChangesAsync(ct);
            return c.Id;
        }

        // ✅ đúng chữ ký interface
        public async Task<bool> UpdateAsync(long id, CategoryUpdateDto dto, CancellationToken ct)
        {
            var c = await _db.Categories.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (c is null) return false;

            if (dto.ParentId.HasValue)
            {
                var parentCount = await _db.Categories.CountAsync(x => x.Id == dto.ParentId.Value, ct);
                if (parentCount == 0) throw new InvalidOperationException("Parent không tồn tại.");
            }

            if (dto.ParentId.HasValue && await IsDescendantOrSelfAsync(id, dto.ParentId.Value, ct))
                throw new InvalidOperationException("Không thể chọn danh mục con (hoặc chính nó) làm cha.");

            var slug = Slugify(string.IsNullOrWhiteSpace(dto.Slug) ? dto.Name : dto.Slug!);
            if (await SlugExistsAsync(slug, id, ct)) throw new InvalidOperationException("Slug đã tồn tại.");

            c.ParentId = dto.ParentId;
            c.Name = dto.Name.Trim();
            c.Slug = slug;
            c.SortOrder = dto.SortOrder;
            c.IsActive = dto.IsActive;

            await _db.SaveChangesAsync(ct);
            return true;
        }

        // ✅ đúng chữ ký interface
        public async Task<bool> DeleteAsync(long id, CancellationToken ct)
        {
            var childCount = await _db.Categories.CountAsync(x => x.ParentId == id, ct);
            if (childCount > 0) throw new InvalidOperationException("Không thể xoá vì còn danh mục con.");

            var c = await _db.Categories.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (c is null) return false;

            _db.Categories.Remove(c);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> SlugExistsAsync(string slug, long? exceptId, CancellationToken ct)
        {
            var q = _db.Categories.AsNoTracking()
                                  .Where(c => c.Slug.ToLower() == slug.ToLower());
            if (exceptId.HasValue) q = q.Where(c => c.Id != exceptId.Value);
            return (await q.CountAsync(ct)) > 0;
        }

        public async Task<List<CategoryOptionDto>> GetOptionsAsync(long? excludeId, CancellationToken ct)
        {
            var all = await _db.Categories
                .AsNoTracking()
                .Select(x => new
                {
                    x.Id,
                    ParentId = (long?)x.ParentId,
                    Name = (string?)x.Name,
                    Sort = (int?)x.SortOrder
                })
                .ToListAsync(ct);

            var map = all.ToDictionary(x => x.Id, x => x);

            var childrenLookup = all
                .GroupBy(x => x.ParentId)
                .Where(g => g.Key.HasValue)
                .ToDictionary(g => g.Key!.Value, g => g.Select(i => i.Id).ToList());

            var excludeSet = new HashSet<long>();
            if (excludeId.HasValue && map.ContainsKey(excludeId.Value))
            {
                excludeSet.Add(excludeId.Value);
                var st = new Stack<long>();
                st.Push(excludeId.Value);
                while (st.Count > 0)
                {
                    var cur = st.Pop();
                    if (childrenLookup.TryGetValue(cur, out var kids))
                        foreach (var k in kids)
                            if (excludeSet.Add(k)) st.Push(k);
                }
            }

            string BuildLabel(long id)
            {
                var parts = new List<string>();
                var seen = new HashSet<long>();
                var cur = id;
                while (true)
                {
                    if (!map.TryGetValue(cur, out var node)) break;
                    if (!seen.Add(cur)) break;

                    parts.Add(node.Name ?? $"#{node.Id}");
                    if (node.ParentId is null) break;
                    cur = node.ParentId.Value;
                }
                parts.Reverse();
                return string.Join(" > ", parts);
            }

            return all
                .Where(x => !excludeSet.Contains(x.Id))
                .OrderBy(x => x.ParentId.HasValue ? 1 : 0)
                .ThenBy(x => x.Sort ?? 0)
                .ThenBy(x => x.Name ?? string.Empty)
                .Select(x => new CategoryOptionDto(x.Id, BuildLabel(x.Id)))
                .ToList();
        }

        private async Task<bool> IsDescendantOrSelfAsync(long selfId, long candidateParentId, CancellationToken ct)
        {
            if (selfId == candidateParentId) return true;

            var map = await _db.Categories
                .Select(x => new { x.Id, x.ParentId })
                .ToDictionaryAsync(x => x.Id, x => x.ParentId, ct);

            var cur = candidateParentId;
            while (map.TryGetValue(cur, out var parent) && parent.HasValue)
            {
                if (parent.Value == selfId) return true;
                cur = parent.Value;
            }
            return false;
        }

        private static string Slugify(string s)
        {
            var slug = s.Trim().ToLowerInvariant();
            slug = Regex.Replace(slug, @"\s+", "-");
            slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");
            slug = Regex.Replace(slug, "-{2,}", "-").Trim('-');
            return slug;
        }
    }
}
