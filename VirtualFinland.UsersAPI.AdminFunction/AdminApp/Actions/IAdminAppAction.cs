namespace VirtualFinland.AdminFunction.AdminApp.Actions;

public interface IAdminAppAction
{
    Task Execute(string? data);
}