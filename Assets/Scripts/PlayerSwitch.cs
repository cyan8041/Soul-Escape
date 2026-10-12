using UnityEngine;

public class PlayerSwitch : MonoBehaviour
{
    public GameObject playerTargetObject;  // 버튼으로 작동시킬 오브젝트
    private bool isActivated = false;

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();

        // 본체일 경우만 작동
        if (player != null && !player.isSoulMode)
        {
            if (!isActivated)
            {
                isActivated = true;
                Debug.Log("본체 스위치 작동!");
                ActivateTarget();
            }
        }
    }

    public void ActivateTarget()
    {
        if (playerTargetObject != null)
        {
            playerTargetObject.SetActive(true);
        }
    }
}
