using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector2 prevPos;
    public void OnBeginDrag(PointerEventData eventData)
    {
        // ドラッグ前の位置を記憶しておく
        prevPos = transform.position;
        this.GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // ドラッグ中は位置を更新する
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // ドラッグ前の位置に戻す
        transform.position = prevPos;
        this.GetComponent<CanvasGroup>().blocksRaycasts = true;
    }

}
