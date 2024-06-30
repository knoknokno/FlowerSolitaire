using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Unity.VisualScripting;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    // ==========UI要素==========
    // カードレイヤー
    public Transform cardLayerTransform;

    // 手札
    public Transform handPlaceholdersTransform;
    // プレイエリア
    public Transform playAreaPlaceholdersTransform;
    // マーケット
    public Transform marketPlaceholdersTransform;

    // 山札
    [SerializeField] private GameObject deckTopCard;
    // 捨札の一番上のカード
    [SerializeField] public CardObject discardPileTopCard;
    // 捨札バッジ
    [SerializeField] private GameObject discardPileBadge;

    // ネクター表示テキスト
    [SerializeField] private TextMeshProUGUI nectarText;
    // アクション数表示テキスト
    [SerializeField] private TextMeshProUGUI actionNumText;
    // ターン数表示テキスト
    [SerializeField] private TextMeshProUGUI turnNumText;
    // カード説明：マウスオーバーで表示されるパネル
    [SerializeField] private GameObject descriptionPanel;

    // ターンエンドボタン
    [SerializeField] private Button turnEndButton;
    // 種をすべて捨てるボタン
    [SerializeField] private Button discardAllSeedsButton;
    // SelectedCards選択確定ボタン
    [SerializeField] private Button selectedCardsOKButton;
    [SerializeField] private Button selectedCardsCancelButton;

    // ダイアログ
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private float delayTime;
    private Coroutine showTextCoroutine;
    
    // カードパネル
    public GameObject cardPanel;
    public Transform cardPanelContentTransform;

    // クリアパネル
    public GameObject clearInfoPanel;

    // ==========機能要素==========
    // GameManagerシングルトンインスタンス
    public static GameManager Instance;

    // 現在の山札
    [SerializeField] private CardStack<CardData> deck;
    // 手札
    public List<CardObject> hand;
    // プレイエリア
    public List<CardObject> playArea;
    // マーケット
    public List<CardObject> market;
    // 捨札
    public CardStack<CardData> discardPile;

    // ネクター
    public int nectar;
    // アクション権
    public int actionNum;
    // ターン数
    private int turnNum;

    // 花カードのCardDataのAddressとCardData格納変数
    public string seedAddress = "Assets/Scripts/Cards/Seed.asset";
    public CardData seedCardData;
    // すべての妖精カードのCardDataのAddressリストとCardData格納リスト
    [SerializeField] private List<string> fairiesAddress = new List<string>();
    private List<CardData> fairiesCardDataList;

    // 選択されたSelectableCardを持つCardObjectを代入するためのリスト
    public List<CardObject> selectedCards;
    // 選択カードの要求数を格納する変数
    public int minRequireNum { get; private set; }
    public int maxRequireNum { get; private set; }
    // SelectableCardsの選択の確定状況を格納する変数
    public bool isSelectedCardsConfirmed;
    public bool isSelectedCardsCanceled;

    // ==========Prefab==========
    // カードプレハブ
    public CardObject cardPrefab;

    // ==========Startメソッド==========
    async public void Start()
    {
        // フレームレート設定
        Application.targetFrameRate = 60;

        // GameManagerシングルトンインスタンス初期化
        Instance = this;

        // 花カード・妖精カードのAddressリストから読み出してそれぞれ格納
        seedCardData = await Addressables.LoadAssetAsync<CardData>(seedAddress).Task;
        fairiesCardDataList = new List<CardData>();
        await LoadFairiesCardData();

        // ゲーム初期化
        InitializeGame();
    }

    async public Task LoadFairiesCardData()
    {
        foreach(String address in fairiesAddress)
        {
            fairiesCardDataList.Add(await Addressables.LoadAssetAsync<CardData>(address).Task);
        }
    }

    // ==========ゲームフロー管理処理==========
    // ゲーム初期化処理
    async public void InitializeGame()
    {
        // 各種数値初期化
        nectar = 0;
        actionNum = 0;
        turnNum = 0;
        nectarText.text = nectar.ToString();
        turnNumText.text = turnNum.ToString();
        actionNumText.text = actionNum.ToString();

        // クリーンアップ処理
        CleanUp();

        // リスト初期化
        hand = new List<CardObject>();
        market = new List<CardObject>();

        // デッキ初期化。初期デッキ：種10枚
        deck = new CardStack<CardData>();
        var seed = await Addressables.LoadAssetAsync<CardData>(seedAddress).Task;
        for (int i = 0; i < 10; i++)
        {
            deck.Push(seed);
        }

        // 捨札初期化
        discardPile = new CardStack<CardData>();
        discardPileTopCard.InitCard(seed);
        discardPileTopCard.gameObject.SetActive(false);

        // クリアパネル非表示
        clearInfoPanel.SetActive(false);

        // 第1ターン開始
        StartTurn();
    }

    // ターン開始処理
    async public void StartTurn()
    {
        // ターン数加算
        turnNum += 1;
        // ネクター初期化
        nectar = 0;
        // アクション権初期化
        actionNum = 1;

        // 表示更新
        UpdateView();

        // 5枚ドローする
        for (int i = 0; i < 5; i++) { await DrawCard(); }

        // ドラッグ可能なカードの有効化
        EnableDraggableCards();

        // マーケット更新
        RefreshMarket();

        // ターンエンドボタン有効化
        turnEndButton.gameObject.SetActive(true);
    }

    // ターン終了処理
    // カードが使用可能かどうかの判定などを予めしておくべきかも。手札にある花カードの枚数など？
    public void EndTurn()
    {
        // クリーンアップ処理
        CleanUp();

        // ターンエンドボタンを無効化
        turnEndButton.gameObject.SetActive(false);

        // 次のターンをスタートする
        StartTurn();
    }

    public void CleanUp()
    {
        // 手札・プレイエリアをすべて捨札にする
        var target = new List<CardObject>();
        foreach (CardObject cardObject in hand)
        {
            target.Add(cardObject);
        }
        foreach (CardObject cardObject in playArea)
        {
            target.Add(cardObject);
        }
        foreach (CardObject cardObject in target)
        {
            SendCardObjectToDiscard(cardObject);
            hand.Remove(cardObject);
            playArea.Remove(cardObject);
        }
    }

    public bool IsWin()
    {
        HashSet<CardData> handList = new HashSet<CardData>();
        foreach (CardObject cardObject in hand)
        {
            if(cardObject.cardData != seedCardData)
            {
                handList.Add(cardObject.cardData);
            }
        }

        if (handList.Count >= 5) { return true; }
        else { return false; }
    }

    async public void Clear()
    {
        foreach(CardObject cardObject in hand)
        {
            cardObject.transform.DOScale(1.5f, 0.2f).SetLoops(2, LoopType.Yoyo);
            await UniTask.Delay(100);
        }
        clearInfoPanel.SetActive(true);

        ShowMessage("クリア！");
        ShowMessage("クリアターン: " + turnNum.ToString());
        ShowMessage("--------------------");
    }

    // ==========カードを操作する処理==========
    // CardObjectを生成する処理
    public CardObject CreateCard(CardData cardData, Transform placeholdersTransform)
    {
        var cardObject = Instantiate(cardPrefab, cardLayerTransform);
        cardObject.InitCard(cardData, placeholdersTransform);

        if(placeholdersTransform == handPlaceholdersTransform) { hand.Add(cardObject); }
        else if(placeholdersTransform == playAreaPlaceholdersTransform) { playArea.Add(cardObject); }
        else if(placeholdersTransform == marketPlaceholdersTransform) { market.Add(cardObject); }

        return cardObject;
    }

    // ドロー処理
    async public Task DrawCard()
    {
        // 山札がなく、捨札がある場合、リシャッフルを行う
        if (deck.Count == 0)
        {
            if (discardPile.Count > 0)
            {
                Reshuffle();
            }
            else
            {
                // カードを引けないときの処理
                Debug.Log("There are no more cards left.");
                return;
            }
        }

        // カードプレハブを生成し、山札のトップを取り出して初期化する。
        var cardObject = CreateCard(deck.Pop(), handPlaceholdersTransform);
        // デッキの位置に移動
        cardObject.transform.position = deckTopCard.transform.position;

        // アニメーション
        UpdateHandPos();
        UpdateDeckPile();

        // 1枚ずつ引いてる感のためのウェイト
        await UniTask.DelayFrame(10);

        if(IsWin())
        {
            Clear();
        }
    }

    // リシャッフル処理
    public void Reshuffle()
    {
        // 捨札をシャッフルし、山札とする。（山札が残っている場合を考慮していないので、場合によってはそれも実装するべきかも。ex：効果によるリシャッフルなど）
        var newDeck = discardPile.OrderBy(i => Guid.NewGuid());
        deck = new CardStack<CardData>(newDeck);
        discardPile = new CardStack<CardData>();
        UpdateDiscardPile();
    }

    // プレースホルダを生成する処理
    public GameObject CreatePlaceholder(Transform parent)
    {
        // 与えられたTransformにプレースホルダを生成、プレースホルダのCardObjectコンポーネントは利用しないので削除
        var placeholder = Instantiate(cardPrefab, parent);
        Destroy(placeholder.gameObject.GetComponent<CardObject>());
        placeholder.name = "Placeholder";
        return placeholder.gameObject;
    }

    // カードのプレイ処理
    async public Task PlayCard(CardObject cardObject)
    {
        // プレイできないカードの場合、効果を処理しない
        if(!cardObject.cardData.isPlayable)
        {
            ShowMessage("このカードはプレイできない。");
            return;
        }
        // アクション権が足りない場合、効果を処理しない
        if(actionNum <= 0)
        {
            ShowMessage("これ以上プレイできない。");
            return;
        }

        // プレイ時に、手札から一旦プレイエリアに移動し、新しく生成したプレースホルダの位置に移動する
        hand.Remove(cardObject);
        playArea.Add(cardObject);

        var prevPlaceholder = cardObject.placeholder;
        var newPlaceholder = CreatePlaceholder(playAreaPlaceholdersTransform);
        cardObject.placeholder = newPlaceholder;
        await UpdatePlayAreaPos();

        cardObject.isDraggable = false;

        ShowMessage(cardObject.cardData.cardName + "をプレイした。");

        // 効果処理中はカードのドラッグを無効化する
        DisableDraggableCards();

        // ==========プレイ時効果の処理==========
        // 効果処理完了時
        if(await cardObject.cardData.playEffect())
        {
            // アクション権の消費
            actionNum -= 1;

            // 前のプレースホルダを破壊
            Destroy(prevPlaceholder);
            // プレイ判定のために判定を消す。解説パネルも見れなくなるため、上手い方法ではない。
            cardObject.gameObject.GetComponent<CanvasGroup>().blocksRaycasts = false;
        }
        // 効果処理未完了・キャンセル時。処理中の効果の巻き戻し処理がある場合はそれぞれのカードで行う
        else
        {
            // カードを手札に戻す
            playArea.Remove(cardObject);
            hand.Add(cardObject);

            cardObject.placeholder = prevPlaceholder;
            cardObject.transform.DOMove(cardObject.placeholder.transform.position, 0.5f);
            
            // 新しいプレースホルダを破壊
            Destroy(newPlaceholder);

            cardObject.gameObject.GetComponent<CanvasGroup>().blocksRaycasts = true;
            cardObject.isDraggable = true;
        }

        // ゲーム状態の表示を更新し、ドラッグ可能なカードを有効化
        UpdateView();
        EnableDraggableCards();
    }

    // カードを捨札にする処理
    public void SendCardObjectToDiscard(CardObject cardObject)
    {
        discardPile.Push(cardObject.cardData);
        RemoveCardObject(cardObject);
    }

    // 盤上のカードを除去する処理
    public void RemoveCardObject(CardObject cardObject)
    {
        Destroy(cardObject.placeholder.gameObject);
        Destroy(cardObject.gameObject);
    }

    // 手札からカードを捨てる処理
    async public Task DiscardHandCard(CardObject cardObject)
    {
        // 手札のカードではなければなにもしない
        if (!hand.Contains(cardObject)) { return; }

        // 効果処理中はカードのドラッグを無効化する
        DisableDraggableCards();

        // 捨札時効果の処理。効果が中断された場合にはカードを手札に入れたまま
        // 効果処理完了時
        if(await cardObject.cardData.discardEffect())
        {
            // 手札から除去する
            hand.Remove(cardObject);
            // 捨札にする
            SendCardObjectToDiscard(cardObject);
        }
        // 効果処理未完了・キャンセル時。処理中の効果の巻き戻し処理がある場合はそれぞれのカードで行う
        else
        {
        }

        // ゲーム状態の表示を更新し、ドラッグ可能なカードを有効化
        UpdateView();
        EnableDraggableCards();
    }

    // 手札のすべての「花」カードを捨札にする処理
    async public void DiscardAllSeeds()
    {
        // 手札から種カードを取得する
        var target = new List<CardObject>();
        foreach(CardObject cardObject in hand)
        {
            if(cardObject.cardData == seedCardData)
            {
                target.Add(cardObject);
            }
        }

        // 取得した種カードを捨札に送り、花蜜を得る
        foreach(CardObject cardObject in target)
        {
            cardObject.transform.DOMove(discardPileTopCard.gameObject.transform.position, 0.15f);

            // 1枚ずつ捨ててる感のためのウェイト
            await UniTask.DelayFrame(10);

            // 手札から除去する
            hand.Remove(cardObject);
            // 捨札にする
            SendCardObjectToDiscard(cardObject);
            // 花蜜を1つ得る
            nectar += 1;

            UpdateDiscardPile();
        }

        UpdateView();
        EnableDraggableCards();
    }

    // 獲得処理
    public void GainCardObject(CardObject cardObject)
    {
        // 獲得コストの消費
        nectar -= cardObject.cardData.cost;

        // 獲得したカードを捨札に追加する
        discardPile.Push(cardObject.cardData);

        // 獲得メッセージの表示・表示更新
        ShowMessage(cardObject.cardData.cardName + "を獲得した。");
        UpdateView();
        EnableDraggableCards();
    }

    public void GainMarketCard(CardObject cardObject)
    {
        // マーケットのカードでなければなにもしない
        if (!market.Contains(cardObject)) { return; }

        // マーケットからカードを獲得し、除去する
        GainCardObject(cardObject);
        market.Remove(cardObject);
        Destroy(cardObject.placeholder.gameObject);
        Destroy(cardObject.gameObject);
    }

    // 手札から追放する処理
    public void BanishHandCard(CardObject cardObject)
    {
        hand.Remove(cardObject);
        BanishCardObject(cardObject);
    }

    // 追放処理
    public void BanishCardObject(CardObject cardObject)
    {
        Destroy(cardObject.placeholder.gameObject);
        Destroy(cardObject.gameObject);
    }

    // 手札からカードを選ぶ処理
    // 手札に選択状態にするコンポーネントを追加、選択を確定するボタンを表示
    // 選択を確定するボタンが押され、選択が確定されるまでは手札のプレイ、マーケットの獲得ができないようにする
    async public Task<List<CardObject>> SelectHandCards(int minRequireNum, int maxRequireNum)
    {
        // selectedCardsと要求数、確定状態の初期化
        selectedCards = new List<CardObject>();
        this.minRequireNum = minRequireNum;
        this.maxRequireNum = maxRequireNum;
        isSelectedCardsConfirmed = false;
        isSelectedCardsCanceled = false;

        // 手札にSelectableCardの動作を追加する
        foreach (CardObject cardObject in hand)
        {
            cardObject.gameObject.AddComponent<SelectableCard>();
        }
        
        // ターンエンドボタン/種自動捨札ボタン非表示
        turnEndButton.gameObject.SetActive(false);
        discardAllSeedsButton.gameObject.SetActive(false);
        // 確認ボタンの表示
        selectedCardsOKButton.gameObject.SetActive(true);
        selectedCardsCancelButton.gameObject.SetActive(true);

        // 選択が確定されるまで待つ
        await UniTask.WaitUntil(() => isSelectedCardsConfirmed || isSelectedCardsCanceled);

        if (isSelectedCardsCanceled) { selectedCards = new List<CardObject>(); }

        // 手札ののSelectableCardの動作を削除
        foreach (CardObject cardObject in hand)
        {
            Destroy(cardObject.gameObject.GetComponent<SelectableCard>());
        }

        // 確認ボタンの非表示
        selectedCardsOKButton.gameObject.SetActive(false);
        selectedCardsCancelButton.gameObject.SetActive(false);
        // ターンエンドボタン/種自動捨札ボタン表示
        turnEndButton.gameObject.SetActive(true);
        discardAllSeedsButton.gameObject.SetActive(true);

        return selectedCards;
    }

    public void RefreshMarket()
    {
        var target = new List<CardObject>();
        // 今のマーケットを削除する
        foreach(CardObject cardObject in market)
        {
            target.Add(cardObject);
        }
        foreach (CardObject cardObject in target)
        {
            market.Remove(cardObject);
            BanishCardObject(cardObject);
        }

        // カードリスト全体から被りがないように取り出してマーケットに生成する
        var index = new List<int>();
        for(int i = 0; i < fairiesAddress.Count; i++)
        {
            index.Add(i);
        }

        for(int i = 0; i < 3; i++)
        {
            var num = index[UnityEngine.Random.Range(0, index.Count)];
            index.Remove(num);

            var cardObject = CreateCard(fairiesCardDataList[num], marketPlaceholdersTransform);

            cardObject.transform.position = cardObject.placeholder.transform.position;
        }

        UpdateMarketPos();
    }

    // 捨札パネル表示
    public void ShowDiscardPile()
    {
        ShowCardPanel(discardPile);
    }

    // 与えられたカードスタック（山札、捨札）をパネル上に一覧表示するメソッド
    public void ShowCardPanel(CardStack<CardData> cardDataStack)
    {
        // 表示するカードスタックに何も入っていなければ何もしない
        if(cardDataStack.Count == 0) { return; }

        // パネル上のカードの表示を生成する
        foreach(CardData cardData in cardDataStack)
        {
            var cardObject = Instantiate(cardPrefab, cardPanelContentTransform);
            cardObject.InitCard(cardData);
        }

        // パネルを最前面に表示
        cardPanel.transform.SetAsLastSibling();
        cardPanel.gameObject.SetActive(true);
    }

    public void HideCardPanel()
    {
        // パネルを非表示
        cardPanel.gameObject.SetActive(false);

        // カードの表示を削除
        foreach(CardObject cardObject in cardPanelContentTransform.gameObject.GetComponentsInChildren<CardObject>())
        {
            Destroy(cardObject.gameObject);
        }
    }

    // ==========ゲームオブジェクト・表示関連処理==========
    // ゲーム状態表示の更新処理
    public void UpdateView()
    {
        turnNumText.text = turnNum.ToString();
        nectarText.text = nectar.ToString();
        actionNumText.text = actionNum.ToString();
        UpdateDiscardPile();
        UpdateDeckPile();
        UpdateHandPos();
        UpdatePlayAreaPos();
        UpdateMarketPos();
    }

    // 手札のカードの表示を更新する処理。プレースホルダの位置へTween移動する
    async public Task UpdateHandPos()
    {
        var layoutGroup =  handPlaceholdersTransform.gameObject.GetComponent<HorizontalLayoutGroup>();
        layoutGroup.CalculateLayoutInputHorizontal();
        layoutGroup.SetLayoutHorizontal();

        // LayoutGroupの適用のために1フレーム待つ
        await UniTask.DelayFrame(1);

        foreach(CardObject cardObject in hand)
        {
            cardObject.transform.DOKill();
            cardObject.gameObject.transform.DOMove(cardObject.placeholder.transform.position, 0.3f).SetEase(Ease.OutBack);
        }
    }

    // 手札のカードの表示を更新する処理。プレースホルダの位置へTween移動する
    async public Task UpdatePlayAreaPos()
    {
        var layoutGroup =  playAreaPlaceholdersTransform.gameObject.GetComponent<HorizontalLayoutGroup>();
        layoutGroup.CalculateLayoutInputHorizontal();
        layoutGroup.SetLayoutHorizontal();

        // LayoutGroupの適用のために1フレーム待つ
        await UniTask.DelayFrame(1);

        foreach(CardObject cardObject in playArea)
        {
            cardObject.transform.DOKill();
            cardObject.gameObject.transform.DOMove(cardObject.placeholder.transform.position, 0.3f).SetEase(Ease.OutBack);
        }
    }

    // マーケットのカードの表示を更新する処理。プレースホルダの位置へTween移動する
    async public Task UpdateMarketPos()
    {
        var layoutGroup =  handPlaceholdersTransform.gameObject.GetComponent<HorizontalLayoutGroup>();
        layoutGroup.CalculateLayoutInputHorizontal();
        layoutGroup.SetLayoutHorizontal();

        // LayoutGroupの適用のために1フレーム待つ
        await UniTask.DelayFrame(1);

        foreach(CardObject cardObject in market)
        {
            cardObject.transform.DOKill();
            cardObject.gameObject.transform.DOMove(cardObject.placeholder.transform.position, 0.3f).SetEase(Ease.OutBack);
        }
    }

    // 山札の表示を更新する処理
    public void UpdateDeckPile()
    {
        // 山札にカードがある場合、捨札を表示する
        if(deck.Count > 0)
        {
            deckTopCard.gameObject.SetActive(true);
            deckTopCard.GetComponent<Shadow>().effectDistance = new Vector2(deck.Count, -5f);
        }
        // 山札にカードがない場合、捨札を非表示にする
        else
        {
            deckTopCard.gameObject.SetActive(false);
        }
    }

    // 捨札の表示を更新する処理
    public void UpdateDiscardPile()
    {
        // 捨札にカードがある場合、捨札を表示する
        if(discardPile.Count > 0)
        {
            discardPileTopCard.UpdateCard(discardPile.Peek());
            discardPileTopCard.gameObject.SetActive(true);
            // 2枚以上のカードがある場合、バッジで枚数を表示する
            if(discardPile.Count >= 2)
            {
                discardPileBadge.GetComponentInChildren<TextMeshProUGUI>().text = discardPile.Count.ToString();
                discardPileBadge.gameObject.SetActive(true);
            }
        }
        // 捨札にカードがない場合、捨札を非表示にする
        else
        {
            discardPileTopCard.gameObject.SetActive(false);
            discardPileBadge.gameObject.SetActive(false);
        }
    }

    // ドラッグ可能なカードを有効化する処理
    public void EnableDraggableCards()
    {
        EnableHandDrag();
        EnableAvailableMarketDrag();
    }

    // 手札のドラッグを有効化する処理
    public void EnableHandDrag()
    {
        foreach(CardObject cardObject in hand)
        {
            cardObject.isDraggable = true;
        }
    }

    // 入手可能なマーケットカードのドラッグを有効化する処理
    public void EnableAvailableMarketDrag()
    {
        foreach(CardObject cardObject in market)
        {
            if(cardObject.cardData.cost <= nectar)
            {
                cardObject.isDraggable = true;
            }
            else
            {
                cardObject.isDraggable = false;
            }
        }
    }

    // ドラッグ可能なカードを無効化する処理
    public void DisableDraggableCards()
    {
        DisableHand();
        DisableMarket();
    }

    // 手札を無効化する処理
    public void DisableHand()
    {
        foreach(CardObject cardObject in hand)
        {
            cardObject.isDraggable = false;
        }
    }

    // マーケットを無効化する処理
    public void DisableMarket()
    {
        foreach(CardObject cardObject in market)
        {
            cardObject.isDraggable = false;
        }
    }

    // ネクター追加処理。アニメーションなどさせたい場合に備えてメソッド化
    public void AddNectar(int num)
    {
        nectar += num;

        // いまのところはUpdateViewで全体ごと更新し、メッセージに表示
        UpdateView();
        ShowMessage(num.ToString() + " コストを追加。");
    }

    // アクション権追加処理。アニメーションなどさせたい場合に備えてメソッド化
    public void AddActionNum(int num)
    {
        actionNum += num;

        // いまのところはUpdateViewで全体ごと更新し、メッセージに表示
        UpdateView();
        ShowMessage(num.ToString() + " プレイ権を追加。");
    }

    // ==========ダイアログパネル関連処理==========
    // ダイアログパネルにメッセージを表示する処理
    public void ShowMessage(string message)
    {
        // 文字送り中なら文字送りを止める
        if(showTextCoroutine != null)
        {
            StopCoroutine(showTextCoroutine);
        }

        // メッセージ表示コルーチンを呼出
        showTextCoroutine = StartCoroutine(ShowTextCoroutine(message));
    }

    // メッセージ表示コルーチン
    private IEnumerator ShowTextCoroutine(string message)
    {
        // メモリ割当を最小化するため先にキャッシュ
        var delay = new WaitForSeconds(delayTime);

        // 行送りし、表示文字数を制限したあとメッセージ内容を追記
        dialogueText.text += "\r\n\r\n";
        var prevLength = dialogueText.text.Length;
        dialogueText.maxVisibleCharacters = prevLength;
        dialogueText.text += message;
        var textLength = dialogueText.text.Length;

        // 追記したメッセージ内容を文字送り
        for (var i = prevLength + 1; i < textLength; i++)
        {
            dialogueText.maxVisibleCharacters = i;
            yield return delay;
        }

        // すべての文字を表示
        dialogueText.maxVisibleCharacters = textLength;

        showTextCoroutine = null;
    }
}

