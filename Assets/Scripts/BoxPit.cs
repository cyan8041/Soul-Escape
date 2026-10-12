using UnityEngine;

// 상자를 밀어 넣으면 구덩이를 메워 다리가 되는 곳. 한 번 들어간 상자는 다시 꺼낼 수 없음.
public class BoxPit : MonoBehaviour
{
    public Transform fillPoint; // 상자가 고정될 위치 (윗면이 바닥과 맞도록)

    private bool isFilled = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isFilled) return;
        PushBox box = other.GetComponent<PushBox>();
        if (box == null || !box.enabled) return; // 이미 다른 구덩이에 박힌 상자는 무시

        isFilled = true;
        Rigidbody rb = box.GetComponent<Rigidbody>();
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;
        box.transform.SetPositionAndRotation(fillPoint.position, Quaternion.identity);
        box.enabled = false; // 낙하 복귀 로직 끔
        Debug.Log(name + " 메움 (되돌릴 수 없음)");
    }
}
