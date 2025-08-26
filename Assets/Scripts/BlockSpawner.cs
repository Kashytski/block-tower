using UnityEngine;

public class BlockSpawner : MonoBehaviour
{
    [SerializeField] GameConfigObj config;

    private void Start()
    {
        if (config == null || config.blockPrefab == null || config.blocksCount <= 0) return;

        int spriteCount = (config.sprites != null) ? config.sprites.Count : 0;

        for (int i = 0; i < config.blocksCount; i++)
        {
            GameObject blockObject = Instantiate(config.blockPrefab, transform);

            if (spriteCount > 0 && blockObject.TryGetComponent(out BlockView blockView))
            {
                int spriteIndex = i % spriteCount;
                blockView.SetSprite(config.sprites[spriteIndex], spriteIndex);
            }
        }
    }
}