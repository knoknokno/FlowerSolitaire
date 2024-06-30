using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;

// バグ：カードのドラッグ中も他のカードにマウスオーバーすると説明が出る。ドラッグ中のカードのRaycastをオフにする手法のため
// ドラッグ可能なカード（プレイ可能な手札、獲得可能な場札）という概念を作るかどうか。

public class CardObject : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    public CardData cardData;

    public GameObject placeholder;

    public bool isDraggable;

    public void InitCard(CardData cardData)
    {
        // カードデータの初期化、カードフェイス適用
        this.cardData = cardData;
        this.gameObject.GetComponent<Image>().sprite = cardData.cardFace;
        this.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = cardData.cost.ToString();
        this.gameObject.GetComponent<CanvasGroup>().alpha = 1f;
    }

    public void InitCard(CardData cardData, Transform placeholdersTransform)
    {
        // カードデータの初期化、カードフェイス適用
        this.cardData = cardData;
        this.gameObject.GetComponent<Image>().sprite = cardData.cardFace;
        this.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = cardData.cost.ToString();
        this.gameObject.GetComponent<CanvasGroup>().alpha = 1f;

        this.placeholder = GameManager.Instance.CreatePlaceholder(placeholdersTransform);
    }

    public void UpdateCard(CardData cardData)
    {
        this.cardData = cardData;
        this.gameObject.GetComponent<UnityEngine.UI.Image>().sprite = cardData.cardFace;
    }

    // ==========イベントハンドラ==========
    // ドラッグ移動前の親要素（カードのあったエリア）を格納する変数
    private Transform prevParent;

    public void OnBeginDrag(PointerEventData eventData)
    {
        // そもそもドラッグ可能ではないカードの場合、何もしない
        if(!isDraggable) { return; }

        // ドラッグ前の位置を記憶しておく
        prevParent = this.placeholder.transform.parent;
        // ドロップエリアの判定のために、カードの判定を消す
        this.gameObject.GetComponent<CanvasGroup>().blocksRaycasts = false;

        // Tween中止
        this.gameObject.transform.DOKill();

        // 最前面に表示する
        this.gameObject.transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        // そもそもドラッグ可能ではないカードの場合、何もしない
        if(!isDraggable) { return; }

        // ドラッグ中は位置を更新する
        this.gameObject.transform.position = eventData.position;
    }

    // 注記：OnEndDragイベントはOnDropイベントのあとに動作する。
    public void OnEndDrag(PointerEventData eventData)
    {
        // そもそもドラッグ可能ではないカードの場合、何もしない
        if(!isDraggable) { return; }

        // カードの判定を戻す
        this.gameObject.GetComponent<CanvasGroup>().blocksRaycasts = true;
        
        // 他のエリアに移動していなければ、カードをドラッグ前の位置に戻す
        if(this.placeholder.transform.parent == prevParent)
        {
            this.gameObject.transform.DOMove(this.placeholder.transform.position, 0.3f).SetEase(Ease.OutBack);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // カード説明表示
        DescriptionPanel.Instance.ShowPanel(cardData);

        // ドラッグ可能なカードの場合には強調表示する
        if (isDraggable)
        {
            this.gameObject.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.2f).SetId("Scale");
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // カード説明非表示
        DescriptionPanel.Instance.HidePanel();

        DOTween.Kill("Scale");
        this.gameObject.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
    }
}
