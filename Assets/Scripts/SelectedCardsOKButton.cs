using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SelectedCardsOKButton : MonoBehaviour
{
    public Button button;

    // 処理が重ければUpdateメソッドで処理しないほうがよいかもしれない。
    void Update()
    {
        // 条件を満たしていればボタンを押せるようにする。
        if(GameManager.Instance.minRequireNum <= GameManager.Instance.selectedCards.Count && GameManager.Instance.selectedCards.Count <= GameManager.Instance.maxRequireNum)
        {
            button.interactable = true;
        }
        else
        {
            button.interactable = false;
        }
    }

    // 選択を確定するメソッド。
    public void ConfirmSelectedCards()
    {
        GameManager.Instance.isSelectedCardsConfirmed = true;
    }
}
