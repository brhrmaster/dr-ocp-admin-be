namespace DrOcupacional.Backend.Application.DTOs;

public class MenuDto
{
    public int CodMenu { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int Ordem { get; set; }
    public string Icone { get; set; } = string.Empty;
}