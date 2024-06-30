using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Nemophila")]
public class Nemophila : CardData
{
    async public override Task<bool> playEffect()
    {
        GameManager.Instance.ShowMessage("手札から好きな枚数選んで、捨札に送る。");
        
        var cards = await GameManager.Instance.SelectHandCards(1, 99);
        // キャンセル時処理
        if(cards.Count == 0)
        {
            return false;
        }

        var DrawNum = cards.Count;

        foreach(CardObject cardObject in cards)
        {
            // 手札から除去する
            GameManager.Instance.hand.Remove(cardObject);
            // 捨札にする
            GameManager.Instance.SendCardObjectToDiscard(cardObject);
        }

        for(int i = 0; i < DrawNum; i++)
        {
            await GameManager.Instance.DrawCard();
        }

        // アクション権追加
        GameManager.Instance.AddActionNum(1);

        // 効果処理完了
        return true;
    }
}
