using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DiscardPile : MonoBehaviour, IPointerClickHandler, IDropHandler
{

    [SerializeField] private GameObject cardPanel;
    [SerializeField] private Transform contentArea;

    public void OnPointerClick(PointerEventData eventData)
    {
        GameManager.Instance.ShowDiscardPile();
    }

    async public void OnDrop(PointerEventData eventData)
    {
        CardObject cardObject = eventData.pointerDrag.GetComponent<CardObject>();
        if(cardObject != null)
        {
            if (GameManager.Instance.hand.Contains(cardObject)) {  }
            // 手札の捨札処理
            if(GameManager.Instance.hand.Contains(cardObject) && cardObject.isDraggable)
            {
                await GameManager.Instance.DiscardHandCard(cardObject);
            }
            // 場札の獲得処理
            else if(GameManager.Instance.market.Contains(cardObject) && cardObject.isDraggable)
            {
                GameManager.Instance.GainMarketCard(cardObject);
            }
        }
    }
}
