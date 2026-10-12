using UnityEngine;

// 몸이 밀 수 있는 상자. 무게 발판 위에 올려두면 발판이 눌린 상태로 유지됨.
[RequireComponent(typeof(Rigidbody))]
public class PushBox : MonoBehaviour
{
    public float mass = 3f;
    public float damping = 4f; // 밀기를 멈추면 금방 서도록

    private Vector3 startPos;

    void Awake()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.mass = mass;
        rb.linearDamping = damping;
        rb.constraints = RigidbodyConstraints.FreezeRotation; // 굴러가지 않게
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        startPos = transform.position;
    }

    void Update()
    {
        // 낭떠러지로 떨어지면 처음 위치로 복귀
        if (transform.position.y < startPos.y - 10f)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.linearVelocity = Vector3.zero;
            transform.position = startPos;
        }
    }
}
