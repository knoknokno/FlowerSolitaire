using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(menuName = "Cards/Nanohana")]
public class Nanohana : CardData
{
    async public override Task<bool> playEffect()
    {
        GameManager.Instance.ShowMessage("手札が5枚になるまでドローする。");

        while(GameManager.Instance.hand.Count() < 5)
        {
            await GameManager.Instance.DrawCard();
        }

        // 効果処理完了
        return true;
    }
}
