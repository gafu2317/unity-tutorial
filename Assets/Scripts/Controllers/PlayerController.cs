using UnityEngine;

/// <summary>
/// プレイヤーキャラクターの移動と操作を管理するクラス
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField] private float moveSpeed = 5f;        // 通常移動速度
    [SerializeField] private float runSpeed = 8f;         // ダッシュ時の速度
    
    [Header("コンポーネント参照")]
    [SerializeField] private Rigidbody2D rb2d;           // 2D物理演算用
    [SerializeField] private Animator animator;           // アニメーション制御（あれば）
    [SerializeField] private SpriteRenderer spriteRenderer; // スプライト表示
    
    [Header("エフェクト")]
    [SerializeField] private ParticleSystem walkEffect;   // 歩行時のエフェクト
    [SerializeField] private ParticleSystem runEffect;    // ダッシュ時のエフェクト
    
    // 内部変数
    private Vector2 moveDirection;                        // 移動方向
    private bool isRunning = false;                       // ダッシュ中かどうか
    private Character characterData;                      // キャラクターデータへの参照
    
    // 入力値のキャッシュ
    private float horizontalInput;
    private float verticalInput;
    
    /// <summary>
    /// 初期化処理
    /// </summary>
    void Start()
    {
        // GameManagerからキャラクターデータを取得
        if (GameManager.Instance != null)
        {
            characterData = GameManager.Instance.playerCharacter;
        }
        
        // コンポーネントの自動取得（未設定の場合）
        AutoSetupComponents();
    }
    
    /// <summary>
    /// コンポーネントの自動設定
    /// </summary>
    void AutoSetupComponents()
    {
        // Rigidbody2Dをチェック
        if (rb2d == null)
            rb2d = GetComponent<Rigidbody2D>();
            
        // Animatorをチェック
        if (animator == null)
            animator = GetComponent<Animator>();
            
        // SpriteRendererをチェック
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
            
        // 2Dトップビューなので重力の影響を受けないようにする
        if (rb2d != null)
            rb2d.gravityScale = 0;
    }
    
    /// <summary>
    /// 毎フレーム更新（入力取得）
    /// </summary>
    void Update()
    {
        // ゲームが一時停止中は操作を受け付けない
        if (GameManager.Instance != null && GameManager.Instance.isGamePaused)
            return;
            
        // 入力を取得
        GetInput();
        
        // アニメーション更新
        UpdateAnimation();
        
        // エフェクト更新
        UpdateEffects();
    }
    
    /// <summary>
    /// 物理演算の更新（移動処理）
    /// </summary>
    void FixedUpdate()
    {
        // ゲームが一時停止中は移動しない
        if (GameManager.Instance != null && GameManager.Instance.isGamePaused)
            return;
            
        // 移動処理
        Move();
    }
    
    /// <summary>
    /// 入力の取得
    /// </summary>
    void GetInput()
    {
        // 移動入力を取得（WASD or 矢印キー）
        horizontalInput = Input.GetAxisRaw("Horizontal");  // A/D or ←/→
        verticalInput = Input.GetAxisRaw("Vertical");      // W/S or ↑/↓
        
        // ダッシュ入力（Shiftキー）
        isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        
        // 移動方向を計算
        moveDirection = new Vector2(horizontalInput, verticalInput).normalized;
    }
    
    /// <summary>
    /// 移動処理
    /// </summary>
    void Move()
    {
        if (moveDirection.magnitude < 0.1f)
            return;  // 入力がほぼない場合は処理しない
        
        // 現在の速度を決定
        float currentSpeed = isRunning ? runSpeed : moveSpeed;
        
        if (rb2d != null)
        {
            // 2D移動
            Vector2 movement = moveDirection * currentSpeed;
            rb2d.linearVelocity = movement;
            
            // スプライトの向きを変更（左右移動の場合）
            if (spriteRenderer != null && horizontalInput != 0)
            {
                spriteRenderer.flipX = horizontalInput < 0;
            }
        }
        else
        {
            // Rigidbodyがない場合は直接Transform移動
            transform.Translate(moveDirection * currentSpeed * Time.fixedDeltaTime, Space.World);
        }
    }
    
    /// <summary>
    /// アニメーションの更新
    /// </summary>
    void UpdateAnimation()
    {
        if (animator == null)
            return;
            
        // 移動中かどうかをアニメーターに伝える
        bool isMoving = moveDirection.magnitude > 0.1f;
        animator.SetBool("IsMoving", isMoving);
        animator.SetBool("IsRunning", isRunning);
        
        // 移動速度をアニメーターに伝える（ブレンドツリー用）
        animator.SetFloat("MoveSpeed", moveDirection.magnitude);
        
        // 方向もアニメーターに伝える
        animator.SetFloat("Horizontal", horizontalInput);
        animator.SetFloat("Vertical", verticalInput);
    }
    
    /// <summary>
    /// エフェクトの更新
    /// </summary>
    void UpdateEffects()
    {
        bool isMoving = moveDirection.magnitude > 0.1f;
        
        // 歩行エフェクト
        if (walkEffect != null)
        {
            if (isMoving && !isRunning && !walkEffect.isPlaying)
                walkEffect.Play();
            else if ((!isMoving || isRunning) && walkEffect.isPlaying)
                walkEffect.Stop();
        }
        
        // ダッシュエフェクト
        if (runEffect != null)
        {
            if (isMoving && isRunning && !runEffect.isPlaying)
                runEffect.Play();
            else if (!isRunning && runEffect.isPlaying)
                runEffect.Stop();
        }
    }
    
    /// <summary>
    /// 移動速度を変更（バフ/デバフ用）
    /// </summary>
    public void ModifySpeed(float multiplier)
    {
        moveSpeed *= multiplier;
        runSpeed *= multiplier;
    }
    
    /// <summary>
    /// 移動速度をリセット
    /// </summary>
    public void ResetSpeed()
    {
        moveSpeed = 5f;
        runSpeed = 8f;
    }
    
    /// <summary>
    /// デバッグ情報の表示
    /// </summary>
    void OnGUI()
    {
        // デバッグモードの場合のみ表示
        #if UNITY_EDITOR
        if (characterData != null)
        {
            GUI.Label(new Rect(10, 10, 200, 20), $"Position: {transform.position}");
            GUI.Label(new Rect(10, 30, 200, 20), $"Speed: {(isRunning ? "Running" : "Walking")}");
            GUI.Label(new Rect(10, 50, 200, 20), $"HP: {characterData.currentHP}/{characterData.maxHP}");
            GUI.Label(new Rect(10, 70, 200, 20), $"MP: {characterData.currentMP}/{characterData.maxMP}");
        }
        #endif
    }
}