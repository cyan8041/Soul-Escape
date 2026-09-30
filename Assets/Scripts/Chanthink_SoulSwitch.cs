using UnityEngine;

public class Chanthink_SoulSwitch : MonoBehaviour
{
    public GameObject soulTargetObject;  // 제어할 오브젝트
    private bool isActivated = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Soul"))  // 혼 오브젝트에 "Soul" 태그 지정 필요
        {
            if (!isActivated)
            {
                isActivated = true;
                Debug.Log("혼 전용 스위치 작동! 계속 활성화 상태 유지");
                ActivateTarget();
            }
        }
    }

    public void ActivateTarget()
    {
        if (soulTargetObject != null)
        {
            soulTargetObject.SetActive(true);
        }
    }
}

