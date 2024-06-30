using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectableCard : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        var cardObject = this.gameObject.GetComponent<CardObject>();
        // クリックしたカードが選択されておらず、要求数を超えない場合に選択して表示を変える
        if(!GameManager.Instance.selectedCards.Contains(cardObject))
        {
            if(GameManager.Instance.selectedCards.Count < GameManager.Instance.maxRequireNum)
            {
                GameManager.Instance.selectedCards.Add(cardObject);
                this.gameObject.GetComponent<Image>().color = new Color32(200, 200, 200, 255);
            }
        }
        // クリックしたカードが選択されていた場合、非選択状態にする
        else
        {
            GameManager.Instance.selectedCards.Remove(cardObject);
            this.gameObject.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
        }
    }

    public void OnDestroy()
    {
        // 破壊時に表示をもとに戻す
        this.gameObject.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
    }
}
