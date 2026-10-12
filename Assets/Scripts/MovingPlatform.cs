using UnityEngine;
using System.Collections.Generic;

// 두 지점 사이를 왕복하는 발판. Freezable이 붙어 있으면 혼이 닿는 동안 그 자리에 멈춤.
// 위에 올라탄 몸·물건을 같이 옮김.
[RequireComponent(typeof(Rigidbody))]
public class MovingPlatform : MonoBehaviour
{
    public Vector3 offset = new Vector3(0, -3, 0); // 시작 위치에서 이만큼 갔다가 돌아옴
    public float period = 4f;                       // 한 번 왕복하는 시간
    public float phase = 0f;                        // 0~1, 발판마다 엇갈리게

    private Rigidbody rb;
    private Freezable freezable;
    private Vector3 start;
    private float t;
    private readonly HashSet<Rigidbody> riders = new HashSet<Rigidbody>();

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        freezable = GetComponent<Freezable>();
        start = transform.position;
        t = phase * period;
    }

    void FixedUpdate()
    {
        if (freezable != null && freezable.IsFrozen) return;

        t += Time.fixedDeltaTime;
        float k = (1f - Mathf.Cos(t / period * Mathf.PI * 2f)) * 0.5f; // 0→1→0 부드럽게
        Vector3 next = start + offset * k;
        Vector3 delta = next - rb.position;
        if (delta.sqrMagnitude < 1e-8f) return;

        FindRiders();
        rb.position = next;
        foreach (Rigidbody r in riders)
        {
            r.WakeUp();
            r.position += delta;
            if (delta.y > 0f && r.linearVelocity.y < 0f)
                r.linearVelocity = new Vector3(r.linearVelocity.x, 0f, r.linearVelocity.z);
        }
    }

    void FindRiders()
    {
        riders.Clear();
        Bounds b = GetComponent<Collider>().bounds;
        Vector3 center = new Vector3(b.center.x, b.max.y + 0.3f, b.center.z);
        Vector3 half = new Vector3(b.extents.x, 0.3f, b.extents.z);
        foreach (Collider c in Physics.OverlapBox(center, half, Quaternion.identity, ~0, QueryTriggerInteraction.Ignore))
        {
            Rigidbody r = c.attachedRigidbody;
            if (r != null && r != rb && !r.isKinematic) riders.Add(r);
        }
    }
}
