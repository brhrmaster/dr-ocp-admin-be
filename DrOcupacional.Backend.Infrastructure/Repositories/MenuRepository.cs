using System.Data;
using Dapper;
using DrOcupacional.Backend.Domain.Entities;
using DrOcupacional.Backend.Domain.Repositories;
using DrOcupacional.Backend.Infrastructure.Data;

namespace DrOcupacional.Backend.Infrastructure.Repositories;

public class MenuRepository : IMenuRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public MenuRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Menu>> SearchByNameAsync(string? nome)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        string sql;
        if (string.IsNullOrWhiteSpace(nome))
        {
            sql = "SELECT cod_menu AS CodMenu, nome AS Nome, ordem AS Ordem, icon AS Icone FROM tb_menu ORDER BY nome";
        }
        else
        {
            sql = "SELECT cod_menu AS CodMenu, nome AS Nome, ordem AS Ordem, icon AS Icone FROM tb_menu WHERE nome LIKE @Nome ORDER BY nome";
        }

        var parameters = new { Nome = $"%{nome}%" };
        var result = await connection.QueryAsync<Menu>(sql, parameters);
        return result;
    }

    public async Task<(IEnumerable<Menu> Items, int TotalCount)> SearchByNamePagedAsync(string? nome, int page, int pageSize)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // Calcular offset
        var offset = (page - 1) * pageSize;
        
        // Query para contar total de registros
        string countSql;
        object countParameters;
        
        if (string.IsNullOrWhiteSpace(nome))
        {
            countSql = "SELECT COUNT(*) FROM tb_menu";
            countParameters = new { };
        }
        else
        {
            countSql = "SELECT COUNT(*) FROM tb_menu WHERE nome LIKE @Nome";
            countParameters = new { Nome = $"%{nome}%" };
        }
        
        var totalCount = await connection.QuerySingleAsync<int>(countSql, countParameters);
        
        // Query para buscar dados paginados
        string dataSql;
        object dataParameters;
        
        if (string.IsNullOrWhiteSpace(nome))
        {
            dataSql = @"
                SELECT cod_menu AS CodMenu, nome AS Nome, ordem AS Ordem, icon AS Icone 
                FROM tb_menu 
                ORDER BY nome
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";
            dataParameters = new { Offset = offset, PageSize = pageSize };
        }
        else
        {
            dataSql = @"
                SELECT cod_menu AS CodMenu, nome AS Nome, ordem AS Ordem, icon AS Icone 
                FROM tb_menu 
                WHERE nome LIKE @Nome 
                ORDER BY nome
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";
            dataParameters = new { Nome = $"%{nome}%", Offset = offset, PageSize = pageSize };
        }
        
        var items = await connection.QueryAsync<Menu>(dataSql, dataParameters);
        
        return (items, totalCount);
    }

    public async Task<Menu?> GetByIdAsync(int codMenu)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = "SELECT cod_menu AS CodMenu, nome AS Nome, ordem AS Ordem, icon AS Icone FROM tb_menu WHERE cod_menu = @CodMenu";
        var parameters = new { CodMenu = codMenu };
        
        return await connection.QueryFirstOrDefaultAsync<Menu>(sql, parameters);
    }

    public async Task<Menu?> GetByNameAsync(string nome, int? excludeCodMenu = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        string sql = "SELECT cod_menu AS CodMenu, nome AS Nome, ordem AS Ordem, icon AS Icone FROM tb_menu WHERE nome = @Nome";
        var parameters = new DynamicParameters();
        parameters.Add("Nome", nome);
        
        if (excludeCodMenu.HasValue)
        {
            sql += " AND cod_menu <> @ExcludeCodMenu";
            parameters.Add("ExcludeCodMenu", excludeCodMenu.Value);
        }
        
        return await connection.QueryFirstOrDefaultAsync<Menu>(sql, parameters);
    }

    public async Task<int> CreateAsync(Menu menu)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            INSERT INTO tb_menu (nome, ordem, icon) 
            OUTPUT INSERTED.cod_menu
            VALUES (@Nome, @Ordem, @Icone)";
        
        var parameters = new { menu.Nome, menu.Ordem, menu.Icone };
        var codMenu = await connection.QuerySingleAsync<int>(sql, parameters);
        
        return codMenu;
    }

    public async Task UpdateAsync(Menu menu)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            UPDATE tb_menu 
            SET nome = @Nome, ordem = @Ordem, icon = @Icone 
            WHERE cod_menu = @CodMenu";
        
        var parameters = new { menu.CodMenu, menu.Nome, menu.Ordem, menu.Icone };
        await connection.ExecuteAsync(sql, parameters);
    }

    public async Task DeleteAsync(int codMenu)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = "DELETE FROM tb_menu WHERE cod_menu = @CodMenu";
        var parameters = new { CodMenu = codMenu };
        
        await connection.ExecuteAsync(sql, parameters);
    }
}



