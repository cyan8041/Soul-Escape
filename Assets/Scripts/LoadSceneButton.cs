using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 누르면 sceneName 씬으로 이동하는 버튼. 스테이지 선택 씬의 스테이지 버튼, 메인의 Stage Select 버튼 등에 쓴다.
[RequireComponent(typeof(Button))]
public class LoadSceneButton : MonoBehaviour
{
    public string sceneName;

    void Awake()
    {
        Button button = GetComponent<Button>();
        if (Application.CanStreamedLevelBeLoaded(sceneName))
            button.onClick.AddListener(() => SceneManager.LoadScene(sceneName));
        else
            button.interactable = false; // Build Settings에 없는 씬
    }
}
