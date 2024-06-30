using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Wasurenagusa")]
public class Wasurenagusa : CardData
{
    async public override Task<bool> playEffect()
    {

        GameManager.Instance.AddActionNum(1);

        var discardPile = GameManager.Instance.discardPile;
        // 捨札がなければ効果を処理しない
        if(discardPile.Count == 0)
        {
            GameManager.Instance.ShowMessage("捨札から何も手札に加えられなかった。");
            return true;
        }

        // 捨札のランダムなカードを手札に加える
        var cardData = discardPile.DrawAtRandom();
        var cardObject = GameManager.Instance.CreateCard(cardData, GameManager.Instance.handPlaceholdersTransform);
        cardObject.transform.position = GameManager.Instance.discardPileTopCard.transform.position;
        GameManager.Instance.ShowMessage("捨札から" + cardData.cardName + "を手札に加えた。");

        // 効果処理完了
        return true;
    }
}
