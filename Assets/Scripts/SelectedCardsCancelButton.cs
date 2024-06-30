using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SelectedCardsCancelButton : MonoBehaviour
{
    // 選択をキャンセルするメソッド。
    public void CancelSelectedCards()
    {
        GameManager.Instance.isSelectedCardsCanceled = true;
    }
}
