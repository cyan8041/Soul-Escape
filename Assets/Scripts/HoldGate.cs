using UnityEngine;

// 연결된 혼 스위치와 무게 발판이 "모두 동시에" 눌려 있는 동안만 열리는 문 (손을 떼면 닫힘).
// 닫히는 순간 몸이나 물건이 걸쳐 있으면 빠져나갈 때까지 기다렸다가 닫힘.
[RequireComponent(typeof(Collider))]
public class HoldGate : MonoBehaviour
{
    public HoldSoulSwitch[] soulSwitches;
    public WeightPlate[] plates;

    private Collider col;
    private Renderer rend;
    private Bounds closedBounds;

    public bool IsOpen => !col.enabled;

    void Awake()
    {
        col = GetComponent<Collider>();
        rend = GetComponent<Renderer>();
        closedBounds = col.bounds;
    }

    void FixedUpdate()
    {
        bool open = true;
        foreach (HoldSoulSwitch s in soulSwitches) if (s == null || !s.IsOn) open = false;
        foreach (WeightPlate p in plates) if (p == null || !p.IsPressed) open = false;

        if (open == IsOpen) return;
        if (!open && OverlapsPhysicsObject()) return;

        col.enabled = !open;
        if (rend != null) rend.enabled = !open;
    }

    bool OverlapsPhysicsObject()
    {
        foreach (Collider c in Physics.OverlapBox(closedBounds.center, closedBounds.extents * 0.98f, transform.rotation, ~0, QueryTriggerInteraction.Ignore))
        {
            if (c == col) continue;
            Rigidbody r = c.attachedRigidbody;
            if (r != null && !r.isKinematic) return true;
        }
        return false;
    }
}
