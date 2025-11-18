namespace DrOcupacional.Backend.Domain.Entities;

public class Menu
{
    public int CodMenu { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int Ordem { get; set; }
    public string Icone { get; set; } = string.Empty;

    public Menu(string nome, int ordem, string icone)
    {
        Nome = nome;
        Ordem = ordem;
        Icone = icone;
    }

    public Menu(int codMenu, string nome, int ordem, string icone)
    {
        CodMenu = codMenu;
        Nome = nome;
        Ordem = ordem;
        Icone = icone;
    }

    public void Update(string nome, int ordem, string icone)
    {
        Nome = nome;
        Ordem = ordem;
        Icone = icone;
    }
}