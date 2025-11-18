using DrOcupacional.Backend.Application.DTOs;

namespace DrOcupacional.Backend.Application.Interfaces;

public interface IMenuService
{
    Task<IEnumerable<MenuDto>> SearchByNameAsync(string? nome);
    Task<PagedResultDto<MenuDto>> SearchByNamePagedAsync(string? nome, int page, int pageSize);
    Task<MenuDto?> GetByIdAsync(int codMenu);
    Task<MenuDto> CreateAsync(CreateMenuDto createMenuDto);
    Task<MenuDto> UpdateAsync(int codMenu, UpdateMenuDto updateMenuDto);
    Task DeleteAsync(int codMenu);
}



