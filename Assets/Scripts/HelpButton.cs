using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HelpButton : MonoBehaviour
{
    public Image helpImage;

    public void ToggleHelpImage()
    {
        if(!helpImage.gameObject.activeSelf)
        {
            ShowHelpImage();
        }
        else
        {
            HideHelpImage();
        }
    }

    public void ShowHelpImage()
    {
        helpImage.gameObject.transform.SetAsLastSibling();
        this.gameObject.transform.SetAsLastSibling();
        helpImage.gameObject.SetActive(true);
    }

    public void HideHelpImage()
    {
        helpImage.gameObject.SetActive(false);
    }
}
