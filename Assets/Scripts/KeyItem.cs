using UnityEngine;

public class KeyItem : MonoBehaviour
{
    public GameObject escapeObject;    // 탈출 오브젝트
    public float floatAmplitude = 0.25f; // 떠오르는 높이 (최대 이동량)
    public float floatFrequency = 1f;    // 떠오르는 속도

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;  // 초기 위치 저장
    }

    void Update()
    {
        // Y축으로 부드럽게 떠오르기 (사인파)
        float newY = startPos.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;

        // 위치 갱신 (X, Z는 고정, Y만 변경)
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();

        // 본체만 획득 가능
        if (player != null && !player.isSoulMode)
        {
            Debug.Log("열쇠 획득!");

            if (escapeObject != null)
            {
                escapeObject.SetActive(true);  // 탈출 오브젝트 활성화
            }

            Destroy(gameObject); // 열쇠는 사라짐
        }
    }
}
