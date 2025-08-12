using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// キャラクターの全データを管理するクラス
/// </summary>
[System.Serializable]  // これを付けるとInspectorで表示＆セーブ可能になる
public class Character
{
    [Header("基本情報")]
    public string characterID;      // ユニークなID（例："player_001"）
    public string characterName;    // キャラクター名（例："勇者"）
    public Sprite characterImage;   // キャラクターの画像
    
    [Header("ステータス")]
    public int level = 1;           // レベル（初期値1）
    public int currentHP = 100;     // 現在のHP
    public int maxHP = 100;         // 最大HP
    public int currentMP = 100;      // 現在のMP
    public int maxMP = 100;          // 最大MP
    public int currentExp = 0;      // 現在の経験値
    
    [Header("所持品")]
    public int gold = 0;            // 所持金
    
    [Header("装備")]
    public Equipment staff;         // 杖
    public Equipment robe;          // ローブ
    public Equipment ring;          // 指輪
    public Equipment brooch;        // ブローチ

        // --- 計算用ヘルパーメソッド ---

    /// <summary>
    /// 全ての装備品をリストで取得する
    /// </summary>
    private List<Equipment> GetAllEquipments()
    {
        var list = new List<Equipment>();
        if (staff != null) list.Add(staff);
        if (robe != null) list.Add(robe);
        if (ring != null) list.Add(ring);
        if (brooch != null) list.Add(brooch);
        return list;
    }

    /// <summary>
    /// 装備による合計ダメージ軽減率を取得する
    /// </summary>
    public float GetTotalDamageReduction()
    {
        return GetAllEquipments().Sum(eq => eq.damageReduction);
    }
    
    /// <summary>
    /// 装備による合計MP回復ボーナスを取得する
    /// </summary>
    public float GetTotalMpRegenBonus()
    {
        return GetAllEquipments().Sum(eq => eq.mpRegenBonus);
    }

    /// <summary>
    /// 指定された魔法の最終的なダメージを計算する
    /// </summary>
// Characterクラス内でのダメージ計算メソッド（修正イメージ）
public float GetFinalMagicDamage(Magic magic)
{
    float damageMultiplier = 1.0f;
    foreach (var eq in GetAllEquipments())
    {
        // 魔法が持つ原理リストに、装備の強化対象原理が含まれているかチェック
        if (magic.principles.Contains(eq.targetPrinciple))
        {
            damageMultiplier += eq.principleDamageBonus;
        }
    }
    return magic.FinalDamage * damageMultiplier;
}
    /// <summary>
    /// 魔法の最終的な射程を計算する
    /// </summary>
    public float GetFinalMagicRange(Magic magic) // magicクラスに基本射程(baseRange)があると仮定
    {
        float rangeMultiplier = 1.0f;
        rangeMultiplier += GetAllEquipments().Sum(eq => eq.rangeBonus);
        // return magic.baseRange * rangeMultiplier;
        return magic.baseRange * rangeMultiplier; 
    }

    
    [Header("魔法")]
    public List<Magic> equippedMagics = new List<Magic>();  // 装備中の魔法リスト
    
    // [Header("状態異常")]
    // public List<StatusEffect> activeEffects = new List<StatusEffect>();  // 現在の状態異常
    
    // レベルアップ時のステータス上昇値（固定）
    private const int HP_GROWTH = 10;   // レベルアップごとにHP+10
    private const int MP_GROWTH = 5;    // レベルアップごとにMP+5
    
    /// <summary>
    /// レベルアップ処理
    /// </summary>
    public void LevelUp()
    {
        level++;
        maxHP += HP_GROWTH;
        maxMP += MP_GROWTH;
        
        Debug.Log($"{characterName}はレベル{level}になった！");
    }
    
    /// <summary>
    /// ダメージを受ける（軽減率を適用）
    /// </summary>
    public void TakeDamage(int damage)
    {
        float finalDamage = damage * (1.0f - GetTotalDamageReduction());
        if (finalDamage < 0) finalDamage = 0;

        currentHP -= (int)finalDamage;
        if (currentHP < 0) currentHP = 0;

        Debug.Log($"{characterName}は{(int)finalDamage}のダメージを受けた！");
    }
    
    /// <summary>
    /// 時間経過によるMP回復
    /// </summary>
    public void RegenerateMpOverTime(float deltaTime)
    {
        // (例)基本回復量1/秒 + 装備ボーナス
        float regenAmount = (1.0f + GetTotalMpRegenBonus()) * deltaTime;
        currentMP += (int)regenAmount;
        if (currentMP > maxMP) currentMP = maxMP;
    }
    
  /// <summary>
  /// HPを回復する
  /// </summary>
  public void Heal(int amount)
  {
    currentHP += amount;
    if (currentHP > maxHP) currentHP = maxHP;

    Debug.Log($"{characterName}はHPを{amount}回復した！");
  }
    
    /// <summary>
    /// MPを消費する（魔法使用時）
    /// </summary>
    public bool UseMp(int amount)
    {
        if (currentMP >= amount)
        {
            currentMP -= amount;
            return true;  // MP消費成功
        }
        
        Debug.Log("MPが足りない！");
        return false;  // MP不足
    }
    
    /// <summary>
    /// MPを回復する
    /// </summary>
    public void RecoverMp(int amount)
    {
        currentMP += amount;
        if (currentMP > maxMP) currentMP = maxMP;
        
        Debug.Log($"{characterName}はMPを{amount}回復した！");
    }
}

/// <summary>
/// 装備アイテムのデータ
/// </summary>
[System.Serializable]
public class Equipment
{
    public string equipmentID;
    public string equipmentName;
    public EquipmentType type;
    public Sprite equipmentIcon;

    [Header("装備効果")]
    [Tooltip("この値が0より大きい場合、受けるダメージをこの割合だけ軽減します (例: 0.1で10%軽減)")]
    public float damageReduction = 0f;

    [Tooltip("MPの秒間回復量をこの値だけ増加させます")]
    public float mpRegenBonus = 0f;

    [Tooltip("魔法の射程をこの割合だけ増加させます (例: 0.2で20%延長)")]
    public float rangeBonus = 0f;

    [Tooltip("魔法の弾速をこの割合だけ増加させます (例: 0.2で20%増加)")]
    public float projectileSpeedBonus = 0f;

    [Header("特定原理の強化")]
    [Tooltip("この装備が強化する『原理』")]
    public PrincipleType targetPrinciple = PrincipleType.None;

    [Tooltip("対象の『原理』を持つ魔法の威力をこの割合だけ増加させます (例: 0.25で25%強化)")]
    public float principleDamageBonus = 0f;
}

/// <summary>
/// 装備の種類
/// </summary>
public enum EquipmentType
{
    Staff,    // 杖
    Robe,     // ローブ
    Ring,     // 指輪
    Brooch    // ブローチ
}

/// <summary>
/// 魔法のデータ
/// </summary>

[System.Serializable]
public class Magic
{
    // --- 既存のパラメータ ---
    public string magicID;
    public string magicName;
    [Header("魔法の構成要素")]
    public List<PrincipleType> principles = new List<PrincipleType>(); // 使用する原理
    public FormType form;                                             // 魔法の形状
    public List<EffectType> effects = new List<EffectType>();         // 魔法がもたらす効果

    public Sprite magicIcon;

    [Header("基本性能")]
    public float baseDamage = 10f;       // 基本ダメージ量
    public int baseMpCost = 5;           // 基本MP消費量
    public float castTime = 1f;          // 詠唱時間（秒）
    public float cooldown = 2f;          // クールダウン時間（秒）
    public float baseRange = 10f;        // 基本射程
    public float baseSpeed = 15f;        // 基本弾速

    [Header("魔法の練度")]
    public int proficiencyLevel = 0;      // 現在の練度レベル
    public int proficiencyExp = 0;      // 現在の練度経験値
    public int nextProficiencyExp = 100; // 次のレベルに必要な経験値

    // --- 練度ボーナス計算用の定数（ここでバランス調整が可能） ---
    private const float DAMAGE_MODIFIER = 0.05f;    // 練度レベルあたりの威力上昇率
    private const float MP_COST_MODIFIER = 0.03f;   // 練度レベルあたりの消費MP減少率
    private const float NEXT_EXP_SCALER = 1.2f;     // 次のレベルまでに必要な経験値の増加倍率

    // --- 練度ボーナスを反映した最終的な値を取得するプロパティ ---

    /// <summary>
    /// 練度ボーナス込みの最終的なダメージ量
    /// </summary>
    public float FinalDamage
    {
        get
        {
            // Log(proficiencyLevel + 1) を使うことで、レベル0の時はボーナス0、レベルが上がるほど上昇量が緩やかになる
            float bonus = 1.0f + (DAMAGE_MODIFIER * Mathf.Log(proficiencyLevel + 1));
            return baseDamage * bonus;
        }
    }

    /// <summary>
    /// 練度ボーナス込みの最終的な消費MP（最低値は1）
    /// </summary>
    public int FinalMpCost
    {
        get
        {
            float reduction = 1.0f - (MP_COST_MODIFIER * Mathf.Log(proficiencyLevel + 1));
            // 最低でも消費MPが1になるようにMathf.MaxとMathf.CeilToIntを使用
            return Mathf.Max(1, Mathf.CeilToInt(baseMpCost * reduction));
        }
    }

    // --- 練度を上昇させるためのメソッド ---

    /// <summary>
    /// この魔法の練度経験値を加算する。プレイヤーレベルが高いほど獲得量が増加する。
    /// </summary>
    /// <param name="playerLevel">現在のプレイヤーレベル</param>
    public void AddProficiencyExp(int playerLevel)
    {
        // 基本獲得経験値10 * (1 + プレイヤーレベル * 0.1) で計算
        int gainExp = (int)(10 * (1.0f + playerLevel * 0.1f));
        proficiencyExp += gainExp;
        
        // 練度レベルアップ処理
        while (proficiencyExp >= nextProficiencyExp)
        {
            proficiencyLevel++;
            proficiencyExp -= nextProficiencyExp;
            // 次のレベルに必要な経験値を増加させる
            nextProficiencyExp = (int)(nextProficiencyExp * NEXT_EXP_SCALER);

            Debug.Log($"{magicName}の練度が{proficiencyLevel}に上がった！");
        }
    }
}

/// <summary>
/// 魔法の根源となる「原理」のタイプ。
/// 設計書に合わせて従来の属性(Element)から変更。
/// </summary>
public enum PrincipleType
{
  None,                   // 無原理

  /// <summary>
  /// 分子運動を制御する。結果として発火や氷結が起こる。
  /// </summary>
  ThermalControl,         // 熱量操作

  /// <summary>
  /// オブジェクトの運動ベクトルを制御する。結果として突風や衝撃が起こる。
  /// </summary>
  KineticControl,         // 運動操作

  /// <summary>
  /// 物質の原子結合を制御する。結果として壁の生成や硬質化が起こる。
  /// </summary>
  StructuralControl,      // 構造操作

  /// <summary>
  /// 電磁場を制御する。結果として電撃やレーザーが起こる。
  /// </summary>
  ElectromagneticControl, // 電磁操作
}

/// <summary>
/// 魔法エネルギーを顕現させる「形状」のタイプ。
/// </summary>
public enum FormType
{
  None,

  /// <summary>
  /// 安定したエネルギー塊として投射する。
  /// </summary>
  Sphere,     // 球

  /// <summary>
  /// エネルギーを収束させ、貫通力を高める。
  /// </summary>
  Spear,      // 槍

  /// <summary>
  /// 持続的な干渉として場に固定する。
  /// </summary>
  Wall,       // 壁

  /// <summary>
  /// エネルギーを不安定化させ、範囲拡散させる。
  /// </summary>
  Explosion,  // 爆発

  /// <summary>
  /// 対象から対象へと自動的に伝播する。
  /// </summary>
  Chain,      // 連鎖
}


/// <summary>
/// 魔法が対象に与える「効果」のタイプ。
/// </summary>
public enum EffectType
{
    None,

    /// <summary>
    /// 対象のHPに直接的な損害を与える。
    /// </summary>
    Damage,         // ダメージ

    /// <summary>
    /// 対象のHPを回復させる。
    /// </summary>
    Heal,           // 回復

    /// <summary>
    /// 対象を強制的に移動させる（ノックバック、引き寄せなど）。
    /// </summary>
    Movement,       // 移動

    /// <summary>
    /// 対象に有益な状態変化（バフ）を付与する。
    /// </summary>
    ApplyBuff,      // バフ付与

    /// <summary>
    /// 対象に有害な状態変化（デバフ）を付与する。
    /// </summary>
    ApplyDebuff,    // デバフ付与

    /// <summary>
    /// 新たなオブジェクトをその場に生成する（壁、武器など）。
    /// </summary>
    Generate,       // 生成
}

// /// <summary>
// /// 状態異常のデータ
// /// </summary>
// [System.Serializable]
// public class StatusEffect
// {
//     public StatusType type;              // 状態異常の種類
//     public float remainingTime;          // 残り時間（-1で永続）
//     public float tickInterval = 1f;      // 効果発動間隔（毒ダメージなど）
//     public int tickDamage = 5;           // 定期ダメージ量
//     public bool isNaturalRecovery;       // 自然回復するか
    
//     /// <summary>
//     /// 時間経過処理
//     /// </summary>
//     public void UpdateTime(float deltaTime)
//     {
//         if (remainingTime > 0)
//         {
//             remainingTime -= deltaTime;
//         }
//     }
    
//     /// <summary>
//     /// 状態異常が終了したか
//     /// </summary>
//     public bool IsExpired()
//     {
//         return remainingTime == 0;  // 0になったら終了（-1は永続）
//     }
// }

// /// <summary>
// /// 状態異常の種類
// /// </summary>
// public enum StatusType
// {
//     None,       // なし
//     Poison,     // 毒
//     Paralysis,  // 麻痺
//     Sleep,      // 睡眠
//     Burn,       // 火傷
//     Freeze,     // 凍結
//     Confusion   // 混乱
// }