using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "BlockTower/Game Config")]
public class GameConfigObj : ScriptableObject
{
    [Min(1)] public int blocksCount = 20;
    public GameObject blockPrefab;

    [Header("Bottom palette (sprites)")]
    public List<Sprite> sprites = new List<Sprite>();
}