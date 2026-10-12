using UnityEngine;

public class AllStageClear : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("모든 스테이지 클리어!");
            // 여기에 엔딩 연출, 메인 메뉴로 이동 등 추가 가능
        }
    }
}
