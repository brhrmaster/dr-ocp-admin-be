using DrOcupacional.Backend.Application.DTOs;
using DrOcupacional.Backend.Application.Interfaces;
using DrOcupacional.Backend.Domain.Entities;
using DrOcupacional.Backend.Domain.Repositories;

namespace DrOcupacional.Backend.Application.Services;

public class MenuService : IMenuService
{
    private readonly IMenuRepository _menuRepository;

    public MenuService(IMenuRepository menuRepository)
    {
        _menuRepository = menuRepository;
    }

    public async Task<IEnumerable<MenuDto>> SearchByNameAsync(string? nome)
    {
        var menus = await _menuRepository.SearchByNameAsync(nome);
        return menus.Select(m => new MenuDto
        {
            CodMenu = m.CodMenu,
            Nome = m.Nome,
            Ordem = m.Ordem,
            Icone = m.Icone
        });
    }

    public async Task<PagedResultDto<MenuDto>> SearchByNamePagedAsync(string? nome, int page, int pageSize)
    {
        // Validar parâmetros
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100; // Limitar máximo de itens por página

        var (menus, totalCount) = await _menuRepository.SearchByNamePagedAsync(nome, page, pageSize);
        
        var menuDtos = menus.Select(m => new MenuDto
        {
            CodMenu = m.CodMenu,
            Nome = m.Nome,
            Ordem = m.Ordem,
            Icone = m.Icone
        });

        return new PagedResultDto<MenuDto>
        {
            Items = menuDtos,
            TotalItems = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<MenuDto?> GetByIdAsync(int codMenu)
    {
        var menu = await _menuRepository.GetByIdAsync(codMenu);
        if (menu == null)
            return null;

        return new MenuDto
        {
            CodMenu = menu.CodMenu,
            Nome = menu.Nome,
            Ordem = menu.Ordem,
            Icone = menu.Icone
        };
    }

    public async Task<MenuDto> CreateAsync(CreateMenuDto createMenuDto)
    {
        // Verificar se já existe menu com o mesmo nome
        var existingMenu = await _menuRepository.GetByNameAsync(createMenuDto.Nome);
        if (existingMenu != null)
            throw new InvalidOperationException("Este menu já existe!");

        var menu = new Menu(createMenuDto.Nome, createMenuDto.Ordem, createMenuDto.Icone);
        var codMenu = await _menuRepository.CreateAsync(menu);

        return new MenuDto
        {
            CodMenu = codMenu,
            Nome = menu.Nome,
            Ordem = menu.Ordem,
            Icone = menu.Icone
        };
    }

    public async Task<MenuDto> UpdateAsync(int codMenu, UpdateMenuDto updateMenuDto)
    {
        var menu = await _menuRepository.GetByIdAsync(codMenu);
        if (menu == null)
            throw new KeyNotFoundException("Menu não encontrado.");

        // Verificar se já existe outro menu com o mesmo nome
        var existingMenu = await _menuRepository.GetByNameAsync(updateMenuDto.Nome, codMenu);
        if (existingMenu != null)
            throw new InvalidOperationException("Este menu já existe!");

        menu.Update(updateMenuDto.Nome, updateMenuDto.Ordem, updateMenuDto.Icone);
        await _menuRepository.UpdateAsync(menu);

        return new MenuDto
        {
            CodMenu = menu.CodMenu,
            Nome = menu.Nome,
            Ordem = menu.Ordem,
            Icone = menu.Icone
        };
    }

    public async Task DeleteAsync(int codMenu)
    {
        var menu = await _menuRepository.GetByIdAsync(codMenu);
        if (menu == null)
            throw new KeyNotFoundException("Menu não encontrado.");

        await _menuRepository.DeleteAsync(codMenu);
    }
}



