using UnityEngine;

// 제자리에서 빙글빙글 도는 막대(자식 오브젝트가 팔). 몸을 쳐서 밀어냄.
// Freezable이 붙어 있으면 혼이 닿는 동안 그 각도에서 멈춤.
[RequireComponent(typeof(Rigidbody))]
public class Rotator : MonoBehaviour
{
    public float degreesPerSecond = 60f;

    private Rigidbody rb;
    private Freezable freezable;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        freezable = GetComponent<Freezable>();
    }

    void FixedUpdate()
    {
        if (freezable != null && freezable.IsFrozen) return;
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, degreesPerSecond * Time.fixedDeltaTime, 0f));
    }
}
