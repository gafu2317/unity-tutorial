using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;

/// <summary>
/// ゲーム全体を管理するマネージャークラス
/// シングルトンパターンで実装（ゲーム中に1つだけ存在）
/// </summary>
public class GameManager : MonoBehaviour
{
    // シングルトンインスタンス（どこからでもアクセス可能）
    public static GameManager Instance { get; private set; }
    
    [Header("プレイヤーデータ")]
    public Character playerCharacter;  // プレイヤーキャラクターのデータ
    
    [Header("ゲーム設定")]
    public bool isGamePaused = false;  // ゲームが一時停止中かどうか
    
    
    /// <summary>
    /// ゲーム開始時の初期化
    /// </summary>
    void Awake()
    {
        // シングルトンの設定
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // シーン遷移しても破棄されない
            InitializeGame();
        }
        else
        {
            Destroy(gameObject);  // 既に存在する場合は破棄
        }
    }
    
    /// <summary>
    /// ゲームの初期化処理
    /// </summary>
    void InitializeGame()
    {
        // プレイヤーキャラクターが未設定の場合、新規作成
        if (playerCharacter == null)
        {
            playerCharacter = new Character();
            playerCharacter.characterID = "player_001";
            playerCharacter.characterName = "プレイヤー";
            playerCharacter.level = 1;
            playerCharacter.currentHP = 100;
            playerCharacter.maxHP = 100;
            playerCharacter.currentMP = 100;
            playerCharacter.maxMP = 100;
            playerCharacter.gold = 100;  // 初期所持金
            
            Debug.Log("新しいプレイヤーキャラクターを作成しました");
        }
    }
    
    /// <summary>
    /// 毎フレーム更新処理
    /// </summary>
    void Update()
    {
        if (!isGamePaused && playerCharacter != null)
        {
            // MP自動回復処理
            RegenerateMp();
            
            // デバッグ用：Lキーでレベルアップテスト
            if (Keyboard.current.lKey.wasPressedThisFrame)
            {
                AddExperience(Character.GetRequiredExp(playerCharacter.level));
            }
        }
    }
    
    /// <summary>
    /// MPの自動回復処理
    /// </summary>
    void RegenerateMp()
    {
        if (playerCharacter.currentMP < playerCharacter.maxMP)
        {
            // 装備によるボーナスを含めたMP回復
            playerCharacter.RegenerateMpOverTime(Time.deltaTime);
        }
    }
    
    /// <summary>
    /// 経験値を追加する
    /// </summary>
    public void AddExperience(int exp)
    {
        playerCharacter.currentExp += exp;
        Debug.Log($"経験値を{exp}獲得！ (現在: {playerCharacter.currentExp})");
        
        // レベルアップチェック
        CheckLevelUp();
    }
    
    /// <summary>
    /// レベルアップのチェックと処理
    /// </summary>
    void CheckLevelUp()
    {
        int requiredExp = Character.GetRequiredExp(playerCharacter.level);
        
        while (playerCharacter.currentExp >= requiredExp)
        {
            playerCharacter.currentExp -= requiredExp;
            playerCharacter.LevelUp();
            
            // 次のレベルに必要な経験値を再計算
            requiredExp = Character.GetRequiredExp(playerCharacter.level);
            
            // レベルアップ演出（必要に応じて追加）
            OnLevelUp();
        }
    }
    
    /// <summary>
    /// レベルアップ時の演出や処理
    /// </summary>
    void OnLevelUp()
    {
        Debug.Log($"レベルアップ！ Lv.{playerCharacter.level}");
        // ここにレベルアップエフェクトやサウンド再生を追加可能
    }
    
    /// <summary>
    /// ゴールドを追加
    /// </summary>
    public void AddGold(int amount)
    {
        playerCharacter.gold += amount;
        Debug.Log($"{amount}ゴールドを獲得！ (所持金: {playerCharacter.gold})");
    }
    
    /// <summary>
    /// ゴールドを消費（購入など）
    /// </summary>
    public bool SpendGold(int amount)
    {
        if (playerCharacter.gold >= amount)
        {
            playerCharacter.gold -= amount;
            Debug.Log($"{amount}ゴールドを使用 (残り: {playerCharacter.gold})");
            return true;
        }
        
        Debug.Log("ゴールドが不足しています！");
        return false;
    }
    
    /// <summary>
    /// 魔法を使用する
    /// </summary>
    public bool UseMagic(Magic magic)
    {
        if (magic == null) return false;
        
        // MP消費チェック
        if (playerCharacter.UseMp(magic.FinalMpCost))
        {
            // 魔法の練度を上昇
            magic.AddProficiencyExp(playerCharacter.level);
            
            Debug.Log($"{magic.magicName}を使用！ (消費MP: {magic.FinalMpCost})");
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// ゲームを一時停止/再開
    /// </summary>
    public void TogglePause()
    {
        isGamePaused = !isGamePaused;
        Time.timeScale = isGamePaused ? 0f : 1f;  // 時間の進行を制御
        Debug.Log(isGamePaused ? "ゲーム一時停止" : "ゲーム再開");
    }
    
    /// <summary>
    /// シーン遷移
    /// </summary>
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }
    
    /// <summary>
    /// 非同期シーン読み込み
    /// </summary>
    IEnumerator LoadSceneAsync(string sceneName)
    {
        // フェードアウト演出などを追加可能
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        
        while (!asyncLoad.isDone)
        {
            // 読み込み進捗を表示可能
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            Debug.Log($"Loading... {progress * 100}%");
            yield return null;
        }
    }
    
    /// <summary>
    /// ゲームをリセット（新規ゲーム）
    /// </summary>
    public void ResetGame()
    {
        playerCharacter = null;
        InitializeGame();
        LoadScene("TitleScene");  // タイトルシーンに戻る
    }
}