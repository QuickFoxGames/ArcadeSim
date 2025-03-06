using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    public void LoadScene(int sceneId)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneId);
    }
}