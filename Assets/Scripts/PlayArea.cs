using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayArea : MonoBehaviour, IDropHandler
{
    async public void OnDrop(PointerEventData eventData)
    {
        CardObject cardObject = eventData.pointerDrag.GetComponent<CardObject>();
        if(cardObject != null)
        {
            // カードが手札からプレイされたときのみ処理する
            if(cardObject.placeholder.transform.parent == GameManager.Instance.handPlaceholdersTransform)
            {
                await GameManager.Instance.PlayCard(cardObject);
            }
        }
    }
}
