using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Tulip")]
public class Tulip : CardData
{
    async public override Task<bool> playEffect()
    {
        // アクション権を追加する
        GameManager.Instance.AddActionNum(2);

        // 効果処理完了
        return true;
    }

    async public override Task<bool> discardEffect()
    {
        // ネクターを2つ得る
        GameManager.Instance.AddNectar(2);

        // 効果処理完了
        return true;
    }
}
