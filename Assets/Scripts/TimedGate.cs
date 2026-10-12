using UnityEngine;

// 연결된 시간제한 기둥이 "모두 동시에" 켜져 있는 동안만 열리는 문.
// 닫히는 순간 몸이나 물건이 걸쳐 있으면 튕겨나가지 않게 빠져나갈 때까지 기다렸다가 닫힘.
[RequireComponent(typeof(Collider))]
public class TimedGate : MonoBehaviour
{
    public TimedSwitch[] switches;

    private Collider col;
    private Renderer rend;
    private Bounds closedBounds; // 콜라이더가 꺼지면 bounds가 비어서 미리 저장

    public bool IsOpen => !col.enabled;

    void Awake()
    {
        col = GetComponent<Collider>();
        rend = GetComponent<Renderer>();
        closedBounds = col.bounds;
    }

    void FixedUpdate()
    {
        bool open = switches.Length > 0;
        foreach (TimedSwitch s in switches)
            if (s == null || !s.IsOn) { open = false; break; }

        if (open == IsOpen) return;
        if (!open && OverlapsPhysicsObject()) return;

        col.enabled = !open;
        if (rend != null) rend.enabled = !open;
        Debug.Log(name + (open ? " 열림" : " 닫힘"));
    }

    bool OverlapsPhysicsObject()
    {
        Bounds b = closedBounds;
        foreach (Collider c in Physics.OverlapBox(b.center, b.extents * 0.98f, transform.rotation, ~0, QueryTriggerInteraction.Ignore))
        {
            if (c == col) continue;
            Rigidbody r = c.attachedRigidbody;
            if (r != null && !r.isKinematic) return true;
        }
        return false;
    }
}
