using UnityEngine;
using UnityEngine.SceneManagement;

// Esc 키: 스테이지 안에서는 스테이지 선택 씬으로, 스테이지 선택 씬에서는 메인으로 이동한다.
public class StageSelectEscape : MonoBehaviour
{
    const string MainScene = "Main";
    const string StageSelectScene = "StageSelect";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Install()
    {
        GameObject listener = new GameObject("StageSelect Escape Listener");
        listener.hideFlags = HideFlags.HideInHierarchy;
        DontDestroyOnLoad(listener);
        listener.AddComponent<StageSelectEscape>();
    }

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;

        string current = SceneManager.GetActiveScene().name;
        if (current == MainScene) return;
        SceneManager.LoadScene(current == StageSelectScene ? MainScene : StageSelectScene);
    }
}
