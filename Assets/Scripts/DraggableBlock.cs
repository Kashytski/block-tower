using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class DraggableBlock : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform blockRectTransform;
    private Canvas rootCanvas;
    private CanvasGroup canvasGroup;
    private Transform parentBeforeDrag;
    private int siblingIndexBeforeDrag;
    private ScrollRect parentScrollRect;

    private Vector2 dragOffset;
    private Vector2 anchoredPositionBeforeDrag;

    private void Awake()
    {
        blockRectTransform = (RectTransform)transform;
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        rootCanvas = GetComponentInParent<Canvas>();
        parentScrollRect = GetComponentInParent<ScrollRect>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (rootCanvas == null) rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas == null) return;
        if (blockRectTransform == null) blockRectTransform = GetComponent<RectTransform>();
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        if (parentScrollRect == null) parentScrollRect = GetComponentInParent<UnityEngine.UI.ScrollRect>();

        parentBeforeDrag = transform.parent;
        siblingIndexBeforeDrag = transform.GetSiblingIndex();
        anchoredPositionBeforeDrag = blockRectTransform.anchoredPosition;

        bool fromTower = parentBeforeDrag != null && parentBeforeDrag.TryGetComponent<TowerController>(out _);
        if (parentScrollRect != null && !fromTower)
        {
            var clone = Instantiate(gameObject, parentBeforeDrag);
            clone.transform.SetSiblingIndex(siblingIndexBeforeDrag);
        }

        if (parentScrollRect != null) parentScrollRect.enabled = false;
        canvasGroup.blocksRaycasts = false;
        transform.SetParent(rootCanvas.transform, true);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)rootCanvas.transform,
            eventData.position,
            eventData.pressEventCamera,
            out var localPoint
        );
        dragOffset = (Vector2)blockRectTransform.localPosition - localPoint;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (rootCanvas == null) rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas == null) return;
        if (blockRectTransform == null) blockRectTransform = GetComponent<RectTransform>();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)rootCanvas.transform,
            eventData.position,
            eventData.pressEventCamera,
            out var localPoint
        );
        blockRectTransform.localPosition = localPoint + dragOffset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        DropZone targetZone = RaycastDropZone(eventData);

        if (targetZone != null && targetZone.zoneType == DropZone.ZoneType.Hole)
        {
            var ellipse = targetZone.GetComponent<EllipseHitArea>();
            if (ellipse != null && !ellipse.ContainsScreenPoint(eventData.position, eventData.pressEventCamera))
                targetZone = null;
        }

        bool fromTower = parentBeforeDrag.TryGetComponent<TowerController>(out _);

        if (targetZone == null)
        {
            if (fromTower)
            {
                ReturnToPreviousPlace();
            }
            else
            {
                MessageLabel.Instance.ShowByKey("ui.missBlock");
                DOTween.Sequence()
                    .Append(transform.DOScale(transform.localScale * 1.1f, 0.08f).SetEase(Ease.OutBack))
                    .Join(canvasGroup.DOFade(0f, 0.16f))
                    .OnComplete(() => Destroy(gameObject));
            }
        }
        else if (targetZone.zoneType == DropZone.ZoneType.Tower)
        {
            if (fromTower)
            {
                ReturnToPreviousPlace();
            }
            else
            {
                var tower = targetZone.tower;
                if (tower != null) tower.AddBlock(blockRectTransform);
            }
        }
        else
        {
            if (fromTower)
            {
                if (parentBeforeDrag.TryGetComponent(out TowerController tower))
                    tower.RemoveBlock(blockRectTransform);
                MessageLabel.Instance.ShowByKey("ui.removeBlock");
                DOTween.Sequence()
                    .Append(transform.DOScale(transform.localScale * 1.1f, 0.08f).SetEase(Ease.OutBack))
                    .Join(canvasGroup.DOFade(0f, 0.16f))
                    .OnComplete(() => Destroy(gameObject));
            }
            else
            {
                MessageLabel.Instance.ShowByKey("ui.missBlock");
                DOTween.Sequence()
                    .Append(transform.DOScale(transform.localScale * 1.1f, 0.08f).SetEase(Ease.OutBack))
                    .Join(canvasGroup.DOFade(0f, 0.16f))
                    .OnComplete(() => Destroy(gameObject));
            }
        }

        canvasGroup.blocksRaycasts = true;
        if (parentScrollRect) parentScrollRect.enabled = true;
    }

    private void ReturnToPreviousPlace()
    {
        transform.SetParent(parentBeforeDrag, false);
        blockRectTransform.anchoredPosition = anchoredPositionBeforeDrag;
        transform.SetSiblingIndex(siblingIndexBeforeDrag);
    }

    private DropZone RaycastDropZone(PointerEventData eventData)
    {
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        for (int i = 0; i < results.Count; i++)
        {
            var hitObject = results[i].gameObject;
            if (hitObject == gameObject) continue;
            var dropZone = FindDropZone(hitObject.transform);
            if (dropZone != null) return dropZone;
        }
        return null;
    }

    private DropZone FindDropZone(Transform targetTransform)
    {
        while (targetTransform != null)
        {
            if (targetTransform.TryGetComponent(out DropZone dropZone))
                return dropZone;
            targetTransform = targetTransform.parent;
        }
        return null;
    }
}