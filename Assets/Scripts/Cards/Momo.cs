using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Momo")]
public class Momo : CardData
{
    async public override Task<bool> playEffect()
    {
        // アクション権を追加する
        GameManager.Instance.AddNectar(4);

        // 効果処理完了
        return true;
    }
}
