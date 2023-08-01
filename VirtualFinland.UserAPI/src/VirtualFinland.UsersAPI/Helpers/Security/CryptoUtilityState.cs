namespace VirtualFinland.UserAPI.Helpers.Security;

public class CryptoUtilityState
{
    private readonly Dictionary<string, string?> _secretQueryKeys = new();

    public void StartQuery(string entityName, string? secretKey)
    {
        if (string.IsNullOrEmpty(secretKey))
            throw new ArgumentNullException(nameof(secretKey));
        _secretQueryKeys[entityName] = secretKey;
    }

    public string? GetQueryKey(string entityName)
    {
        return _secretQueryKeys.ContainsKey(entityName) ? _secretQueryKeys[entityName] : null;
    }

    public void ClearQuery(string entityName)
    {
        _secretQueryKeys[entityName] = null;
    }
}