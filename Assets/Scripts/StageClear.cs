using UnityEngine;
using UnityEngine.SceneManagement;

public class StageClear : MonoBehaviour
{
    public string nextSceneName;  // 다음 스테이지 씬 이름 또는 빈칸일 때 게임 클리어 처리

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            string currentScene = SceneManager.GetActiveScene().name;
            Debug.Log(currentScene+" 클리어!");

            if (!string.IsNullOrEmpty(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                Debug.Log("모든 스테이지 클리어!");
            }
        }
    }
}
