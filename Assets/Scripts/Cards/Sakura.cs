using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Sakura")]
public class Sakura : CardData
{
    async public override Task<bool> playEffect()
    {
        GameManager.Instance.ShowMessage("手札から1枚選んで追放する。");
        
        var cards = await GameManager.Instance.SelectHandCards(1, 1);
        // キャンセル時処理
        if(cards.Count == 0) { return false; }

        foreach(CardObject cardObject in cards)
        {
            GameManager.Instance.BanishHandCard(cardObject);
        }

        // アクション権追加
        GameManager.Instance.AddActionNum(1);

        // 効果処理完了
        return true;
    }
}
