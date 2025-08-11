using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    // タイトルからメインへ
    public void GoToMainScene()
    {
        SceneManager.LoadScene("Main");
    }
    
    // 他のシーン遷移も追加可能
    public void GoToTitleScene()
    {
        SceneManager.LoadScene("Title");
    }
    
}