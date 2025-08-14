using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// HP/MP/レベルなどのステータスをUIに表示するクラス
/// </summary>
public class StatusDisplay : MonoBehaviour
{
    [Header("HPの表示要素")]
    [SerializeField] private Slider hpBar;                    // HPバー（Slider）
    [SerializeField] private TextMeshProUGUI hpText;         // HP数値テキスト
    [SerializeField] private Image hpFillImage;              // HPバーの塗りつぶし部分
    [SerializeField] private Color hpNormalColor = Color.green;   // HP通常時の色
    [SerializeField] private Color hpDangerColor = Color.red;     // HP危険時の色
    
    [Header("MPの表示要素")]
    [SerializeField] private Slider mpBar;                    // MPバー（Slider）
    [SerializeField] private TextMeshProUGUI mpText;         // MP数値テキスト
    [SerializeField] private Image mpFillImage;              // MPバーの塗りつぶし部分
    [SerializeField] private Color mpColor = new(0, 0.5f, 1);  // MPバーの色
    
    [Header("その他のステータス表示")]
    [SerializeField] private TextMeshProUGUI levelText;      // レベル表示
    [SerializeField] private TextMeshProUGUI goldText;       // 所持金表示
    [SerializeField] private TextMeshProUGUI expText;        // 経験値表示
    [SerializeField] private TextMeshProUGUI nameText;       // キャラクター名表示
    
    [Header("エフェクト設定")]
    [SerializeField] private float updateSpeed = 5f;         // バーのアニメーション速度
    [SerializeField] private bool animateBars = true;        // バーをアニメーションさせるか
    [SerializeField] private float dangerThreshold = 0.3f;   // HP危険域のしきい値（30%以下）
    
    [Header("ダメージ/回復表示")]
    [SerializeField] private GameObject damageTextPrefab;    // ダメージ数値のプレハブ
    [SerializeField] private Transform damageTextParent;     // ダメージ数値の親オブジェクト
    
    // 内部変数
    private Character playerCharacter;                       // プレイヤーキャラクターの参照
    private float targetHpValue = 1f;                       // HPバーの目標値
    private float targetMpValue = 1f;                       // MPバーの目標値
    private int previousHp;                                 // 前フレームのHP（変化検知用）
    private int previousMp;                                 // 前フレームのMP（変化検知用）
    private bool isLowHp = false;                          // HP低下フラグ
    private Coroutine lowHpPulseCoroutine;                  // HP点滅コルーチンの参照
    
    /// <summary>
    /// 初期化処理
    /// </summary>
    void Start()
    {
        // GameManagerからプレイヤーデータを取得
        if (GameManager.Instance != null)
        {
            playerCharacter = GameManager.Instance.playerCharacter;
            previousHp = playerCharacter.currentHP;
            previousMp = playerCharacter.currentMP;
        }
        
        // HPバーの塗りつぶし画像を取得
        if (hpBar != null && hpFillImage == null)
        {
            hpFillImage = hpBar.fillRect.GetComponent<Image>();
        }
        
        // MPバーの塗りつぶし画像を取得
        if (mpBar != null && mpFillImage == null)
        {
            mpFillImage = mpBar.fillRect.GetComponent<Image>();
        }
        
        // 色の初期設定
        if (hpFillImage != null) hpFillImage.color = hpNormalColor;
        if (mpFillImage != null) mpFillImage.color = mpColor;
        
        // 初期表示の更新
        UpdateDisplay();
    }
    
    /// <summary>
    /// 毎フレーム更新
    /// </summary>
    void Update()
    {
        if (playerCharacter == null)
        {
            // プレイヤーデータが未設定の場合、再取得を試みる
            if (GameManager.Instance != null)
            {
                playerCharacter = GameManager.Instance.playerCharacter;
            }
            return;
        }
        
        // ステータス表示を更新
        UpdateDisplay();
        
        // HP/MPの変化を検知
        DetectValueChanges();
        
        // HPが低い時の警告演出
        UpdateLowHpWarning();
    }
    
    /// <summary>
    /// 表示を更新
    /// </summary>
    void UpdateDisplay()
    {
        if (playerCharacter == null) return;
        
        // HP表示の更新
        if (hpBar != null)
        {
            targetHpValue = (float)playerCharacter.currentHP / playerCharacter.maxHP;
            
            if (animateBars)
            {
                // スムーズにアニメーション
                hpBar.value = Mathf.Lerp(hpBar.value, targetHpValue, Time.deltaTime * updateSpeed);
            }
            else
            {
                // 即座に更新
                hpBar.value = targetHpValue;
            }
        }
        
        if (hpText != null)
        {
            hpText.text = $"HP: {playerCharacter.currentHP}/{playerCharacter.maxHP}";
        }
        
        // MP表示の更新
        if (mpBar != null)
        {
            targetMpValue = (float)playerCharacter.currentMP / playerCharacter.maxMP;
            
            if (animateBars)
            {
                mpBar.value = Mathf.Lerp(mpBar.value, targetMpValue, Time.deltaTime * updateSpeed);
            }
            else
            {
                mpBar.value = targetMpValue;
            }
        }
        
        if (mpText != null)
        {
            mpText.text = $"MP: {playerCharacter.currentMP}/{playerCharacter.maxMP}";
        }
        
        // その他のステータス表示
        if (levelText != null)
        {
            levelText.text = $"Lv.{playerCharacter.level}";
        }
        
        if (goldText != null)
        {
            goldText.text = $"Gold: {playerCharacter.gold}";
        }
        
        if (nameText != null)
        {
            nameText.text = playerCharacter.characterName;
        }
        
        // 経験値表示（次のレベルまでの必要経験値も表示）
        if (expText != null)
        {
            int requiredExp = Character.GetRequiredExp(playerCharacter.level);
            expText.text = $"EXP: {playerCharacter.currentExp}/{requiredExp}";
        }
    }
    
    /// <summary>
    /// HP/MPの変化を検知して演出
    /// </summary>
    void DetectValueChanges()
    {
        // HPの変化を検知
        if (playerCharacter.currentHP != previousHp)
        {
            int difference = playerCharacter.currentHP - previousHp;
            
            if (difference < 0)
            {
                // ダメージを受けた
                OnDamageTaken(-difference);
            }
            else
            {
                // 回復した
                OnHealed(difference);
            }
            
            previousHp = playerCharacter.currentHP;
        }
        
        // MPの変化を検知
        if (playerCharacter.currentMP != previousMp)
        {
            int difference = playerCharacter.currentMP - previousMp;
            
            if (difference < 0)
            {
                // MP消費
                OnMpUsed(-difference);
            }
            else
            {
                // MP回復
                OnMpRecovered(difference);
            }
            
            previousMp = playerCharacter.currentMP;
        }
    }
    
    /// <summary>
    /// HP低下時の警告演出
    /// </summary>
    void UpdateLowHpWarning()
    {
        if (hpFillImage == null || playerCharacter == null) return;
        
        float hpRatio = (float)playerCharacter.currentHP / playerCharacter.maxHP;
        
        if (hpRatio <= dangerThreshold)
        {
            if (!isLowHp)
            {
                isLowHp = true;
                lowHpPulseCoroutine = StartCoroutine(LowHpPulse());
            }
            
            // 色を危険色に変更
            hpFillImage.color = Color.Lerp(hpFillImage.color, hpDangerColor, Time.deltaTime * 5f);
        }
        else
        {
            if (isLowHp)
            {
                isLowHp = false;
                if (lowHpPulseCoroutine != null)
                {
                    StopCoroutine(lowHpPulseCoroutine);
                    lowHpPulseCoroutine = null;
                }
            }
            // 色を通常色に戻す
            hpFillImage.color = Color.Lerp(hpFillImage.color, hpNormalColor, Time.deltaTime * 5f);
        }
    }
    
    /// <summary>
    /// HP低下時の点滅演出
    /// </summary>
    IEnumerator LowHpPulse()
    {
        while (isLowHp)
        {
            // 点滅演出
            float pulseValue = Mathf.PingPong(Time.time * 2f, 1f);
            hpFillImage.color = Color.Lerp(hpDangerColor, new Color(1, 0.5f, 0), pulseValue);
            yield return null;
        }
    }
    
    /// <summary>
    /// ダメージを受けた時の演出
    /// </summary>
    void OnDamageTaken(int damage)
    {
        // バーを赤く点滅させる
        if (hpFillImage != null)
        {
            StartCoroutine(FlashBar(hpFillImage, Color.red, 0.2f));
        }
        
        // ダメージ数値を表示（プレハブがある場合）
        ShowFloatingText($"-{damage}", Color.red);
    }
    
    /// <summary>
    /// 回復時の演出
    /// </summary>
    void OnHealed(int amount)
    {
        // バーを緑に点滅させる
        if (hpFillImage != null)
        {
            StartCoroutine(FlashBar(hpFillImage, Color.green, 0.2f));
        }
        
        // 回復数値を表示
        ShowFloatingText($"+{amount}", Color.green);
    }
    
    /// <summary>
    /// MP消費時の演出
    /// </summary>
    void OnMpUsed(int _)
    {
        // 特に演出なし（必要に応じて追加）
    }
    
    /// <summary>
    /// MP回復時の演出
    /// </summary>
    void OnMpRecovered(int amount)
    {
        // 青く光る演出（必要に応じて）
        if (mpFillImage != null && amount > 1)  // 自然回復以外
        {
            StartCoroutine(FlashBar(mpFillImage, Color.cyan, 0.2f));
        }
    }
    
    /// <summary>
    /// バーを一時的に光らせる
    /// </summary>
    IEnumerator FlashBar(Image barImage, Color flashColor, float duration)
    {
        Color originalColor = barImage.color;
        barImage.color = flashColor;
        yield return new WaitForSeconds(duration);
        barImage.color = originalColor;
    }
    
    /// <summary>
    /// フローティングテキストを表示
    /// </summary>
    void ShowFloatingText(string text, Color color)
    {
        // ダメージテキストプレハブがある場合のみ
        if (damageTextPrefab != null && damageTextParent != null)
        {
            GameObject textObj = Instantiate(damageTextPrefab, damageTextParent);
            if (textObj.TryGetComponent<TextMeshProUGUI>(out var tmpText))
            {
                tmpText.text = text;
                tmpText.color = color;
            }
            
            // 2秒後に削除
            Destroy(textObj, 2f);
        }
    }
    
    /// <summary>
    /// ステータス表示の表示/非表示を切り替え
    /// </summary>
    public void ToggleDisplay(bool show)
    {
        gameObject.SetActive(show);
    }
}