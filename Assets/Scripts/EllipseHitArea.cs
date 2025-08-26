using UnityEngine;

public class EllipseHitArea : MonoBehaviour
{
    private RectTransform rectTransform;
    private readonly Vector3[] worldCorners = new Vector3[4];

    private void Awake()
    {
        rectTransform = (RectTransform)transform;
    }

    public bool ContainsScreenPoint(Vector2 screenPoint, Camera eventCamera)
    {
        if (rectTransform == null) return false;

        rectTransform.GetWorldCorners(worldCorners);

        Vector2 screenBottomLeft = RectTransformUtility.WorldToScreenPoint(eventCamera, worldCorners[0]);
        Vector2 screenTopRight = RectTransformUtility.WorldToScreenPoint(eventCamera, worldCorners[2]);

        Vector2 screenCenter = (screenBottomLeft + screenTopRight) * 0.5f;
        float halfWidth = Mathf.Abs(screenTopRight.x - screenBottomLeft.x) * 0.5f;
        float halfHeight = Mathf.Abs(screenTopRight.y - screenBottomLeft.y) * 0.5f;
        if (halfWidth <= 0f || halfHeight <= 0f) return false;

        float deltaX = screenPoint.x - screenCenter.x;
        float deltaY = screenPoint.y - screenCenter.y;

        float ellipseValue = deltaX * deltaX / (halfWidth * halfWidth) +
                             deltaY * deltaY / (halfHeight * halfHeight);

        return ellipseValue <= 1.0001f;
    }
}