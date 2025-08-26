using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TowerBlockState
{
    public int spriteIndex;
    public float x;
}

[Serializable]
public class TowerSave
{
    public List<TowerBlockState> blocks = new();
}

public static class ProgressService
{
    private const string SaveKey = "tower_save_v1";

    public static void SaveTower(TowerController tower)
    {
        if (tower == null) return;

        var save = new TowerSave();
        if (tower.Blocks != null)
            save.blocks.Capacity = tower.Blocks.Count;

        foreach (var blockRect in tower.Blocks)
        {
            if (blockRect == null) continue;

            int spriteIndex = blockRect.TryGetComponent(out BlockView blockView)
                ? blockView.SpriteIndex
                : -1;

            save.blocks.Add(new TowerBlockState
            {
                spriteIndex = spriteIndex,
                x = blockRect.anchoredPosition.x
            });
        }

        string json = JsonUtility.ToJson(save);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    public static bool TryLoad(out TowerSave saveData)
    {
        saveData = null;
        if (!PlayerPrefs.HasKey(SaveKey)) return false;

        string json = PlayerPrefs.GetString(SaveKey);
        if (string.IsNullOrEmpty(json)) return false;

        saveData = JsonUtility.FromJson<TowerSave>(json);
        return saveData != null && saveData.blocks != null;
    }

    public static void Clear()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();
    }
}