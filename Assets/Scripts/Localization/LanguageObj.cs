using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Language", menuName = "BlockTower/Localization/Language")]
public class LanguageObj : ScriptableObject
{
    public SystemLanguage language = SystemLanguage.English;

    [System.Serializable]
    public class Entry
    {
        public string key;
        [TextArea] public string value;
    }

    public List<Entry> entries = new();
}