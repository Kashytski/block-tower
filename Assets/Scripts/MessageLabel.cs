using System.Collections;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class MessageLabel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI messageText;
    [SerializeField] float visibleDuration = 1.5f;

    private Coroutine hideCoroutine;

    public static MessageLabel Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (messageText == null)
            messageText = GetComponentInChildren<TextMeshProUGUI>(true);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void ShowByKey(string localizationKey)
    {
        string text = LocalizationService.Instance != null
            ? LocalizationService.Instance.Key(localizationKey)
            : localizationKey;

        Show(text);
    }

    public void Show(string text)
    {
        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);

        if (messageText != null)
            messageText.text = text;

        gameObject.SetActive(true);
        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(visibleDuration);
        gameObject.SetActive(false);
        hideCoroutine = null;
    }
}