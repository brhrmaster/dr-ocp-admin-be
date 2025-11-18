using DrOcupacional.Backend.Application.DTOs;
using DrOcupacional.Backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DrOcupacional.Backend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class MenusController : ControllerBase
{
    private readonly IMenuService _menuService;
    private readonly ILogger<MenusController> _logger;

    public MenusController(IMenuService menuService, ILogger<MenusController> logger)
    {
        _menuService = menuService;
        _logger = logger;
    }

    /// <summary>
    /// Busca menus por nome (sem paginação - mantido para compatibilidade)
    /// </summary>
    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<MenuDto>>> Search([FromQuery] string? nome)
    {
        try
        {
            var menus = await _menuService.SearchByNameAsync(nome);
            return Ok(menus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar menus");
            return StatusCode(500, new { message = "Erro ao buscar menus" });
        }
    }

    /// <summary>
    /// Busca menus por nome com paginação
    /// </summary>
    /// <param name="nome">Nome do menu para busca (opcional)</param>
    /// <param name="page">Número da página (padrão: 1)</param>
    /// <param name="pageSize">Itens por página (padrão: 10, máximo: 100)</param>
    [HttpGet]
    public async Task<ActionResult<PagedResultDto<MenuDto>>> SearchPaged(
        [FromQuery] string? nome,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var result = await _menuService.SearchByNamePagedAsync(nome, page, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar menus paginados");
            return StatusCode(500, new { message = "Erro ao buscar menus" });
        }
    }

    /// <summary>
    /// Obtém um menu por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<MenuDto>> GetById(int id)
    {
        try
        {
            var menu = await _menuService.GetByIdAsync(id);
            if (menu == null)
                return NotFound(new { message = "Menu não encontrado" });

            return Ok(menu);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar menu {Id}", id);
            return StatusCode(500, new { message = "Erro ao buscar menu" });
        }
    }

    /// <summary>
    /// Cria um novo menu
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<MenuDto>> Create([FromBody] CreateMenuDto createMenuDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var menu = await _menuService.CreateAsync(createMenuDto);
            return CreatedAtAction(nameof(GetById), new { id = menu.CodMenu }, menu);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar menu");
            return StatusCode(500, new { message = "Erro ao criar menu" });
        }
    }

    /// <summary>
    /// Atualiza um menu existente
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<MenuDto>> Update(int id, [FromBody] UpdateMenuDto updateMenuDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var menu = await _menuService.UpdateAsync(id, updateMenuDto);
            return Ok(menu);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar menu {Id}", id);
            return StatusCode(500, new { message = "Erro ao atualizar menu" });
        }
    }

    /// <summary>
    /// Exclui um menu
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _menuService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir menu {Id}", id);
            return StatusCode(500, new { message = "Erro ao excluir menu" });
        }
    }
}



