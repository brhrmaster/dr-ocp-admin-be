using DrOcupacional.Backend.Domain.Entities;

namespace DrOcupacional.Backend.Domain.Repositories;

public interface IMenuRepository
{
    Task<IEnumerable<Menu>> SearchByNameAsync(string? nome);
    Task<(IEnumerable<Menu> Items, int TotalCount)> SearchByNamePagedAsync(string? nome, int page, int pageSize);
    Task<Menu?> GetByIdAsync(int codMenu);
    Task<Menu?> GetByNameAsync(string nome, int? excludeCodMenu = null);
    Task<int> CreateAsync(Menu menu);
    Task UpdateAsync(Menu menu);
    Task DeleteAsync(int codMenu);
}



