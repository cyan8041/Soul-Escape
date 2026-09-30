using UnityEngine;

public class Chanthink_PlayerSwitch2 : MonoBehaviour // 플레이어가 오브젝트와 닿은 동안만 활성화
{
    public GameObject targetObject; // 활성화/비활성화할 오브젝트
    private int playerTouchCount = 0;

    private void OnTriggerEnter(Collider other)
    {
        Chanthink_PlayerController player = other.GetComponent<Chanthink_PlayerController>();
        if (player != null && !player.isSoulMode) // 본체만 작동
        {
            playerTouchCount++;
            if (playerTouchCount == 1)
            {
                targetObject?.SetActive(true);
                Debug.Log("본체가 스위치 위에 있음 → 타겟 활성화");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Chanthink_PlayerController player = other.GetComponent<Chanthink_PlayerController>();
        if (player != null && !player.isSoulMode)
        {
            playerTouchCount--;
            if (playerTouchCount <= 0)
            {
                targetObject?.SetActive(false);
                Debug.Log("본체가 스위치에서 벗어남 → 타겟 비활성화");
            }
        }
    }
}
