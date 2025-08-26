using System.Collections.Generic;
using UnityEngine;

public class LocalizationService : MonoBehaviour
{
    public static LocalizationService Instance { get; private set; }

    [SerializeField] List<LanguageObj> languages = new();
    [SerializeField] SystemLanguage startLanguage = SystemLanguage.English;

    private readonly Dictionary<string, string> _map = new();
    private SystemLanguage _current;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetLanguage(startLanguage);
    }

    public void SetLanguage(SystemLanguage lang)
    {
        var table = languages.Find(t => t.language == lang);
        if (table == null)
        {
            Debug.LogWarning($"[Localization] Table for {lang} not found.");
            return;
        }

        _map.Clear();
        foreach (var entry in table.entries)
        {
            if (!string.IsNullOrEmpty(entry.key))
            {
                _map[entry.key] = entry.value;
            }
        }

        _current = lang;
    }

    public string Key(string key)
    {
        if (string.IsNullOrEmpty(key)) return string.Empty;
        return _map.TryGetValue(key, out var value) ? value : key;
    }

    public SystemLanguage Current => _current;
}