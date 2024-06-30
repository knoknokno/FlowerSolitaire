using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Tanpopo")]
public class Tanpopo : CardData
{
    async public override Task<bool> playEffect()
    {
        // アクション権の追加
        GameManager.Instance.AddActionNum(1);
        // 1枚ドローする
        await GameManager.Instance.DrawCard();

        // 効果処理完了
        return true;
    }
}
