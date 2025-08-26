using UnityEngine;
using UnityEngine.UI;

public class BlockView : MonoBehaviour
{
    [SerializeField] Image blockImage;

    [SerializeField] int spriteIndex = -1;
    public int SpriteIndex => spriteIndex;

    private void Awake()
    {
        if (blockImage == null)
            blockImage = GetComponentInChildren<Image>(true);
    }

    public void SetSprite(Sprite sprite, int index)
    {
        if (blockImage != null)
            blockImage.sprite = sprite;

        spriteIndex = index;
    }

    public void SetSprite(Sprite sprite)
    {
        if (blockImage != null)
            blockImage.sprite = sprite;
    }
}