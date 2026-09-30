using UnityEngine;
using UnityEngine.SceneManagement; // 씬 리로드에 필요

public class Chanthink_ColorChanger : MonoBehaviour
{
    private bool isActivated = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (!isActivated)
        {
            // 처음 접촉: 색 바꾸고 GameManager에 알림
            isActivated = true;
            GetComponent<Renderer>().material.color = Color.green;
            Chanthink_GameManager.Instance.NotifyFloorActivated(this);
        }
        else
        {
            // 이미 활성화된 버튼인데 또 닿았을 때: 씬 리셋
            Debug.Log("이미 활성화된 버튼 재접촉 → 씬 재시작");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
    void OnEnable()
    {
        Chanthink_GameManager.Instance?.RegisterButton(this);
    }

}
