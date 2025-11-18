namespace DrOcupacional.Backend.Application.DTOs;

public class CreateMenuDto
{
    public string Nome { get; set; } = string.Empty;
    public int Ordem { get; set; }
    public string Icone { get; set; } = string.Empty;
}