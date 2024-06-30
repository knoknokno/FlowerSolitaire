using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Seed")]
public class Seed : CardData
{
    async public override Task<bool> discardEffect()
    {
        // ネクターを1つ得る
        GameManager.Instance.AddNectar(1);

        // 効果処理完了
        return true;
    }
}
