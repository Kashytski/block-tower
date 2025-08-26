using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TowerController : MonoBehaviour
{
    [SerializeField] GameConfigObj config;
    RectTransform towerArea;
    readonly List<RectTransform> blocksStack = new();
    public IReadOnlyList<RectTransform> Blocks => blocksStack;

    void Awake()
    {
        towerArea = (RectTransform)transform;
    }

    private void Start()
    {
        TryRestoreOnStart();
    }

    private void TryRestoreOnStart()
    {
        if (ProgressService.TryLoad(out var save))
        {
            Debug.Log($"[Restore] save found, blocks: {save.blocks?.Count ?? 0}");
            RestoreFromSave(save);
        }
        else
        {
            Debug.Log("[Restore] no save found");
        }
    }

    void CleanupStack()
    {
        for (int i = blocksStack.Count - 1; i >= 0; i--)
            if (blocksStack[i] == null) blocksStack.RemoveAt(i);
    }

    public void AddBlock(RectTransform newBlock, bool animateAndSave = true)
    {
        CleanupStack();

        newBlock.SetParent(towerArea, false);
        newBlock.anchorMin = newBlock.anchorMax = new Vector2(0.5f, 0f);
        newBlock.pivot = new Vector2(0.5f, 0f);
        newBlock.localScale = Vector3.one;

        RectTransform last = blocksStack.Count > 0 ? blocksStack[blocksStack.Count - 1] : null;
        float currentTopY = last == null ? 0f : last.anchoredPosition.y + last.rect.height;

        if (last != null && animateAndSave)
        {
            var canvas = GetComponentInParent<Canvas>();
            var cam = canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera ? canvas.worldCamera : null;
            if (RectTransformUtility.WorldToScreenPoint(cam, towerArea.TransformPoint(0f, currentTopY, 0f)).y > Screen.height)
            {
                MessageLabel.Instance.ShowByKey("ui.heightLimit");
                Destroy(newBlock.gameObject);
                return;
            }
        }

        float blockWidth = newBlock.rect.width;
        float x = Mathf.Clamp((last == null ? 0f : last.anchoredPosition.x) + Random.Range(-blockWidth * 0.5f, blockWidth * 0.5f),
                             -(towerArea.rect.width * 0.5f - blockWidth * 0.5f),
                             towerArea.rect.width * 0.5f - blockWidth * 0.5f);

        newBlock.anchoredPosition = new Vector2(x, currentTopY);
        blocksStack.Add(newBlock);

        if (animateAndSave)
        {
            MessageLabel.Instance.ShowByKey("ui.placeBlock");

            float y = newBlock.anchoredPosition.y;
            float jump = Mathf.Min(newBlock.rect.height * 0.4f, 80f);
            DOTween.Sequence()
                .Append(newBlock.DOAnchorPosY(y + jump, 0.15f).SetEase(Ease.OutQuad))
                .Join(newBlock.DOScale(1.05f, 0.15f).SetEase(Ease.OutQuad))
                .Append(newBlock.DOAnchorPosY(y, 0.12f).SetEase(Ease.InQuad))
                .Join(newBlock.DOScale(1f, 0.12f).SetEase(Ease.InQuad))
                .OnComplete(() => ProgressService.SaveTower(this));
        }
        else
        {
            ProgressService.SaveTower(this);
        }
    }

    public void RemoveBlock(RectTransform blockToRemove)
    {
        CleanupStack();
        int index = blocksStack.IndexOf(blockToRemove);
        if (index < 0) return;

        blocksStack.RemoveAt(index);
        if (blocksStack.Count == 0)
        {
            ProgressService.SaveTower(this);
            return;
        }

        float halfBlock = blocksStack[0].rect.width * 0.5f;
        float areaHalf = towerArea.rect.width * 0.5f - halfBlock;
        float baseX = index == 0 ? 0f : blocksStack[index - 1].anchoredPosition.x;
        float delta = index < blocksStack.Count ? blocksStack[index].anchoredPosition.x - baseX : 0f;
        float shift = Mathf.Clamp(delta, -halfBlock, halfBlock) - delta;
        float nextY = index == 0 ? 0f : blocksStack[index - 1].anchoredPosition.y + blocksStack[index - 1].rect.height;

        var seq = DOTween.Sequence();
        for (int i = index; i < blocksStack.Count; i++)
        {
            blocksStack[i].DOKill();
            seq.Join(blocksStack[i].DOAnchorPos(
                new Vector2(
                    Mathf.Clamp(blocksStack[i].anchoredPosition.x + shift, -areaHalf, areaHalf),
                    nextY
                ),
                0.25f
            ).SetEase(Ease.OutCubic));
            nextY += blocksStack[i].rect.height;
        }

        seq.OnComplete(() => ProgressService.SaveTower(this));
    }

    private void RestoreFromSave(TowerSave save)
    {
        if (save == null || save.blocks == null || save.blocks.Count == 0)
            return;

        for (int i = blocksStack.Count - 1; i >= 0; i--)
        {
            if (blocksStack[i] != null)
                Destroy(blocksStack[i].gameObject);
        }
        blocksStack.Clear();

        foreach (var block in save.blocks)
        {
            RectTransform newBlock = CreateBlockForRestore(block.spriteIndex);
            if (newBlock == null) continue;

            AddBlock(newBlock, false);

            var pos = newBlock.anchoredPosition;
            newBlock.anchoredPosition = new Vector2(block.x, pos.y);
            newBlock.anchoredPosition3D = new Vector3(newBlock.anchoredPosition.x, newBlock.anchoredPosition.y, 0f);
        }

        ProgressService.SaveTower(this);
    }


    private RectTransform CreateBlockForRestore(int spriteIndex)
    {
        if (config == null || config.blockPrefab == null)
        {
            Debug.LogError("[Tower] No GameConfigObj or blockPrefab assigned");
            return null;
        }

        GameObject go = Instantiate(config.blockPrefab);
        var rt = go.GetComponent<RectTransform>();

        var view = go.GetComponent<BlockView>();
        if (view != null && config.sprites != null && spriteIndex >= 0 && spriteIndex < config.sprites.Count)
            view.SetSprite(config.sprites[spriteIndex], spriteIndex);

        return rt;
    }
}