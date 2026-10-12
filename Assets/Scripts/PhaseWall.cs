using UnityEngine;
using System.Collections.Generic;

// 몸·혼 통과 규칙이 바뀌는 벽. 모든 PhaseWall은 전환 기둥(PhaseSwitch)을 건드릴 때마다 한꺼번에 뒤집힘.
//  - 돌벽 상태: 몸과 물건은 막고, 혼은 통과
//  - 푸른 막 상태: 혼은 막고, 몸과 물건은 통과
// startsAsStone이 true인 벽과 false인 벽은 항상 서로 반대 상태가 됨.
[RequireComponent(typeof(Collider))]
public class PhaseWall : MonoBehaviour
{
    public static bool Flipped { get; private set; }
    private static readonly List<PhaseWall> all = new List<PhaseWall>();

    public bool startsAsStone = true;
    public bool permanentBorder = false; // 켜면 항상 몸도 혼도 막는 경계벽 (전환되지 않음)
    public Material stoneMaterial;    // 몸 막힘 (불투명)
    public Material barrierMaterial;  // 혼 막힘 (반투명)

    private Collider col;
    private Renderer rend;

    public bool IsStone => permanentBorder || startsAsStone != Flipped;
    public bool BlocksSoul => permanentBorder || !IsStone;

    void Awake()
    {
        col = GetComponent<Collider>();
        rend = GetComponent<Renderer>();
    }

    void OnEnable()
    {
        all.Add(this);
        Flipped = false; // 씬을 새로 시작하면 처음 상태로
        Refresh();
    }

    void OnDisable() { all.Remove(this); }

    public static void Toggle()
    {
        Flipped = !Flipped;
        foreach (PhaseWall w in all) w.Refresh();
    }

    void Refresh()
    {
        if (rend != null) rend.sharedMaterial = IsStone ? stoneMaterial : barrierMaterial;
    }

    void FixedUpdate()
    {
        // 돌벽이 되는 순간 몸이나 물건이 벽 안에 걸쳐 있으면 튕겨나가지 않게, 빠져나갈 때까지 통과 상태로 둠
        bool wantSolid = IsStone;
        if (wantSolid && col.isTrigger && OverlapsPhysicsObject()) return;
        col.isTrigger = !wantSolid;
    }

    bool OverlapsPhysicsObject()
    {
        Bounds b = col.bounds;
        foreach (Collider c in Physics.OverlapBox(b.center, b.extents * 0.98f, transform.rotation, ~0, QueryTriggerInteraction.Ignore))
        {
            if (c == col) continue;
            Rigidbody r = c.attachedRigidbody;
            if (r != null && !r.isKinematic) return true;
        }
        return false;
    }
}
