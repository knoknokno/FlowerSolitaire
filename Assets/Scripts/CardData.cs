using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CardData : ScriptableObject
{
    public string cardName;
    public int cost;
    public Sprite cardFace;
    [TextArea(6, 6)]
    public string description;

    public bool isPlayable;

    async public virtual Task<bool> playEffect()
    {
        GameManager.Instance.ShowMessage("このカードはプレイできない。");
        return false;
    }

    async public virtual Task<bool> discardEffect()
    {
        Debug.Log("This card has no discard effect.");
        return false;
    }

}
