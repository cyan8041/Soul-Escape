using UnityEngine;
using UnityEngine.SceneManagement;

public class Chanthink_MainManager : MonoBehaviour
{
    public void StartGame() // 게임 시작, Stage1 씬으로 이동
    {
        SceneManager.LoadScene("Stage1");
    }

    public void EndGame() // 게임 종료
    {
        Debug.Log("게임 종료");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
