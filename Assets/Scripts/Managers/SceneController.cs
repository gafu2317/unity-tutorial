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
  public void GoToDungeonScene()
  {
    SceneManager.LoadScene("Dungeon");
  }

  public void GoToSettingsScene()
  {
    SceneManager.LoadScene("Settings");
  }

  public void GoToShopScene()
  {
    SceneManager.LoadScene("Shop");
  }

  public void GoToSpellCraftScene()
  {
    SceneManager.LoadScene("SpellCraft");
  }

  public void GoToGameOverScene()
  {
    SceneManager.LoadScene("GameOver");
  }

  public void GoToClearScene()
  {
    SceneManager.LoadScene("Clear");
  }
}