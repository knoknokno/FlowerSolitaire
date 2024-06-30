using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Suisen")]
public class Suisen : CardData
{
    async public override Task<bool> playEffect()
    {
        GameManager.Instance.ShowMessage("手札から1枚選んで、そのプレイ時効果を使う。");
        
        var selectedCards = await GameManager.Instance.SelectHandCards(1, 1);
        // キャンセル時処理
        if(selectedCards.Count == 0)
        {
            return false;
        }

        foreach(CardObject cardObject in selectedCards)
        {
            if(!await cardObject.cardData.playEffect())
            {
                return false;
            }
        }

        // 効果処理完了
        return true;
    }
}
