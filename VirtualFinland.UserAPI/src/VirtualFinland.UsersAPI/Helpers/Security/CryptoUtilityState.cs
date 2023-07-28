namespace VirtualFinland.UserAPI.Helpers.Security;

public interface ICryptoUtilityState
{
    void StartQuery(string entityName, string? secretKey);
    string? GetQueryKey(string entityName);
    void ClearQuery(string entityName);
}

public class CryptoUtilityState : ICryptoUtilityState
{
    private Dictionary<string, string?> _secretQueryKeys = new Dictionary<string, string?>();

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