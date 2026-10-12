using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public string nextSceneName;
    private Dictionary<ColorChanger, bool> floorStates = new Dictionary<ColorChanger, bool>();
    private bool stageCleared = false;

    void Awake()
    {
        // 싱글톤 방식 (선택 사항)
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 초기화: 씬 내 모든 ColorChanger 수집
        ColorChanger[] changers = FindObjectsByType<ColorChanger>(FindObjectsSortMode.None);

        foreach (ColorChanger floor in changers)
        {
            floorStates[floor] = false; // 기본 상태는 비활성화
        }
    }

    // ColorChanger에서 호출됨
    public void NotifyFloorActivated(ColorChanger changer)
    {
        if (floorStates.ContainsKey(changer))
        {
            floorStates[changer] = true;
            CheckStageClearCondition();
        }
    }

    void CheckStageClearCondition()
    {
        foreach (var state in floorStates.Values)
        {
            if (!state)
                return; // 하나라도 false면 클리어 아님
        }

        if (!stageCleared)
        {
            stageCleared = true;
            OnStageClear();
        }
    }

    void OnStageClear()
    {
        Debug.Log("버튼 스테이지 클리어!");
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        // 탈출구 생성, 애니메이션 재생, 다음 씬 로드 등 가능
    }
    public void RegisterButton(ColorChanger changer)
    {
        if (!floorStates.ContainsKey(changer))
            floorStates.Add(changer, false);
    }

}
