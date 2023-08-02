namespace VirtualFinland.UserAPI.Helpers.Security;

public class CryptoUtilityState
{
    private readonly Dictionary<string, string?> _secretQueryKeys = new();
    private readonly HashSet<string> _disabledQueriyKeys = new();
    private readonly List<string> _personDataQueryKeys = new() { "Person", "PersonAdditionalInformation", "Address" };

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
        EnableQueryKey(entityName);
    }

    public void DisableQueryKey(string entityName)
    {
        _disabledQueriyKeys.Add(entityName);
    }

    public void EnableQueryKey(string entityName)
    {
        _disabledQueriyKeys.Remove(entityName);
    }

    public bool IsQueryKeyDisabled(string entityName)
    {
        return _disabledQueriyKeys.Contains(entityName);
    }

    public void StartPersonDataQuery(string? secretKey)
    {
        foreach (var entityName in _personDataQueryKeys)
        {
            StartQuery(entityName, secretKey);
        }
    }

    public void DisablePersonDataQueryKeys()
    {
        foreach (var entityName in _personDataQueryKeys)
        {
            DisableQueryKey(entityName);
        }
    }

    public void EnablePersonDataQueryKeys()
    {
        foreach (var entityName in _personDataQueryKeys)
        {
            EnableQueryKey(entityName);
        }
    }

    public void ClearPersonDataQueryKeys()
    {
        foreach (var entityName in _personDataQueryKeys)
        {
            ClearQuery(entityName);
        }
    }
}