using UnityEngine;

public class SoulController : MonoBehaviour
{
    public float floatSpeed = 5f;
    public float possessRange = 1.5f; // 이 거리 안의 물건에 E키로 빙의

    [Header("지형 따라 떠다니기")]
    public bool followGround = false;  // 켜면 발밑 지형 높이를 따라 오르내림 (높은 곳 물건에도 닿음)
    public float hoverHeight = 2.4f;   // 발밑 바닥에서 떠 있는 높이
    public float heightLerp = 6f;

    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 move = new Vector3(h, 0, v) * floatSpeed * Time.deltaTime;
        MoveBlockedBySoulWalls(move);

        if (followGround) FollowGround();

        if (Input.GetKeyDown(KeyCode.E))
        {
            // 고리가 가까우면 고리에 걸고, 아니면 물건에 빙의
            SoulHook hook = SoulHook.FindNear(transform.position);
            if (hook != null && hook.Latch(gameObject)) return;
            Possessable target = Possessable.FindNear(transform.position, possessRange);
            if (target != null) target.Possess(gameObject);
        }
    }

    // 혼은 대부분 벽을 통과하지만, "혼 막힘" 상태인 PhaseWall 안으로는 못 들어감 (벽을 따라 미끄러짐)
    void MoveBlockedBySoulWalls(Vector3 move)
    {
        Vector3 pos = transform.position;
        if (!BlockedAt(pos + move, pos)) { transform.position = pos + move; return; }
        Vector3 mx = new Vector3(move.x, 0f, 0f);
        Vector3 mz = new Vector3(0f, 0f, move.z);
        if (!BlockedAt(pos + mx, pos)) transform.position = pos + mx;
        else if (!BlockedAt(pos + mz, pos)) transform.position = pos + mz;
    }

    bool BlockedAt(Vector3 next, Vector3 current)
    {
        float r = SoulRadius();
        foreach (Collider c in Physics.OverlapSphere(next, r, ~0, QueryTriggerInteraction.Collide))
        {
            PhaseWall w = c.GetComponent<PhaseWall>();
            if (w == null || !w.BlocksSoul) continue;
            // 이미 벽 안에 있을 때(벽이 막 바뀐 순간)는 빠져나갈 수 있게 함
            if (c.bounds.Contains(current) || Vector3.Distance(c.ClosestPoint(current), current) < r * 0.5f) continue;
            return true;
        }
        return false;
    }

    float SoulRadius()
    {
        SphereCollider s = GetComponent<SphereCollider>();
        return s != null ? s.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.z) : 0.5f;
    }

    void FollowGround()
    {
        // 위에서 아래로 쏴서 혼 발밑의 가장 높은 면을 찾음 (혼 자신·트리거·빙의 물건 위 몸 등은 상관없이 맨 위 면)
        Vector3 from = new Vector3(transform.position.x, transform.position.y + 20f, transform.position.z);
        float best = float.MinValue;
        foreach (RaycastHit hit in Physics.RaycastAll(from, Vector3.down, 40f, ~0, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.transform.IsChildOf(transform)) continue;
            if (hit.collider.GetComponent<PhaseWall>() != null) continue; // 바뀌는 벽 위로 솟지 않게
            if (hit.point.y > best) best = hit.point.y;
        }
        if (best == float.MinValue) return; // 낭떠러지 위에선 높이 유지

        float targetY = best + hoverHeight;
        Vector3 p = transform.position;
        p.y = Mathf.Lerp(p.y, targetY, heightLerp * Time.deltaTime);
        transform.position = p;
    }
}
