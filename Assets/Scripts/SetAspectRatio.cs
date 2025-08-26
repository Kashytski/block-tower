using UnityEngine;

public class SetAspectRatio : MonoBehaviour
{
    [SerializeField] RectTransform leftBG;
    [SerializeField] RectTransform rightBG;
    [SerializeField] RectTransform bottomBG;

    private void Start()
    {
        SetAspect();
    }

    public void SetAspect()
    {
        const float aspectCutoffBelow16x9 = 1.7f;
        const float sideScale = 1.2f;
        const float bottomScale = 1.33f;

        float aspect = (float)Screen.width / Screen.height;
        if (aspect < aspectCutoffBelow16x9)
        {
            leftBG.sizeDelta = rightBG.sizeDelta *= sideScale;
            bottomBG.sizeDelta *= bottomScale;
        }
    }
}
