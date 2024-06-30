using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// 注：Pivotを左下(0,1)に設定すること
public class DescriptionPanel : MonoBehaviour
{
    // DescriptionPanelシングルトンインスタンス

    public static DescriptionPanel Instance;

    // 表示までのディレイ(s)
    [SerializeField] private float delayTime;
    // トランジション速度(s)
    [SerializeField] private float transitionTime;

    // カード名を格納するテキスト
    [SerializeField] private TextMeshProUGUI nameText;
    // ネクター数値を格納するテキスト
    [SerializeField] private TextMeshProUGUI nectarText;
    // 説明文を格納するテキスト
    [SerializeField] private TextMeshProUGUI descriptionText;


    // CanvasGroupコンポーネント。アルファ値操作に利用
    private CanvasGroup canvasGroup;
    // rectTransformコンポーネント。高さおよびカーソル位置取得に利用
    private RectTransform rectTransform;
    // 表示位置を算出するための高さ格納変数
    private float height;

    // トランジション表示コルーチン
    private Coroutine showPanelCoroutine;

    void Start()
    {
        Instance = this;

        canvasGroup = this.GetComponent<CanvasGroup>();
        rectTransform = this.GetComponent<RectTransform>();
        height = rectTransform.sizeDelta.y;

        canvasGroup.alpha = 0f;
    }

    void Update()
    {
        if(Input.mousePosition.y + height + 10f <= Screen.height)
        {
            // カーソルから(10, 10)の位置を基準とした位置に移動
            rectTransform.position = Input.mousePosition + new Vector3(10f, 10f + height, 0);
        }
        else
        {
            // スクリーンからはみ出る場合、はみ出ないように調整して移動（y方向のみ考慮）
            rectTransform.position = new Vector3(Input.mousePosition.x + 10f, Screen.height, 0);
        }
    }

    // パネル表示メソッド。引数：カードデータ(CardData)
    public void ShowPanel(CardData cardData)
    {
        // 表示用のテキストを代入
        nameText.text = cardData.cardName;
        nectarText.text = cardData.cost.ToString();
        descriptionText.text = cardData.description;

        // 最前面に表示
        this.gameObject.transform.SetAsLastSibling();

        // コルーチンが動いている最中なら止める
        if(showPanelCoroutine != null)
        {
            StopCoroutine(showPanelCoroutine);
        }

        // コルーチン起動
        showPanelCoroutine = StartCoroutine(ShowPanelCoroutine());
    }

    // パネル非表示
    public void HidePanel()
    {
        // コルーチンが動いている最中なら止める
        if(showPanelCoroutine != null)
        {
            StopCoroutine(showPanelCoroutine);
        }
        
        // パネル非表示
        canvasGroup.alpha = 0f;
    }

    private IEnumerator ShowPanelCoroutine()
    {
        // メモリ割当を最小化するため先にキャッシュ
        var delay = new WaitForSeconds(transitionTime);
        yield return new WaitForSeconds(delayTime);

        for (canvasGroup.alpha = 0f; canvasGroup.alpha < 1f; canvasGroup.alpha += 0.1f)
        {
            yield return delay;
        }

        canvasGroup.alpha = 1f;

        showPanelCoroutine = null;
    }


}
