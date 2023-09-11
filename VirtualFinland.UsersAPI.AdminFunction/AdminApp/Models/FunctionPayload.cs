namespace VirtualFinland.AdminFunction.AdminApp.Models;
public record FunctionPayload
{
    public Actions Action { get; init; }
    public string? Data { get; init; }
}