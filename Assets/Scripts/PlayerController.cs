using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameObject soulObject;  // 씬에 두고 비활성화 상태인 유체 오브젝트

    private Rigidbody rb;
    public float moveSpeed = 5f;
    public float jumpForce = 7f;

    public bool isSoulMode = false;
    private bool isSoulOut = false;
    private Collider col;

    public LayerMask groundLayer; // 바닥 레이어 지정

    [Header("혼 복귀 연출")]
    public float returnSpeed = 10f;        // 혼이 몸으로 돌아가는 속도 (칸/초)
    public float minReturnTime = 0.6f;     // 가까워도 이 시간만큼은 천천히 돌아감
    public float maxReturnTime = 2.5f;     // 아무리 멀어도 이 시간 안에 도착
    public float returnArcHeight = 1.5f;   // 돌아가는 경로를 위로 살짝 휘게

    private bool isReturning = false;
    private float stunnedUntil = -1f; // 막대 등에 맞아 튕겨나가는 동안은 조작이 속도를 덮어쓰지 않게

    public void Stun(float seconds)
    {
        stunnedUntil = Mathf.Max(stunnedUntil, Time.time + seconds);
    }

    [Header("혼이 빠진 몸")]
    public bool ragdollWhenSoulOut = false; // 켜면 혼이 빠진 몸이 축 늘어진 물리 오브젝트가 됨 (밀리고, 실려 가고, 무게추가 됨)
    private RigidbodyConstraints savedConstraints;
    private float savedYaw;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        if (soulObject != null)
            soulObject.SetActive(false);
        GameObject startPoint = GameObject.Find("StartPoint");
        if (startPoint != null)
        {
            transform.position = startPoint.transform.position;
        }
        else
        {
            Debug.LogWarning("StartPoint 오브젝트를 찾을 수 없습니다.");
        }
    }

    void Update()
    {
        if (!isSoulOut)
        {
            HandleMovement();

            if (Input.GetKeyDown(KeyCode.F))
            {
                StartSoulOut();
            }
        }
        else
        {
            // 유체 상태일 땐 본체 움직임 제한

            if (soulObject != null)
            {
                // 유체 이동은 SoulController에서 처리

                // 거리와 상관없이 F키로 혼이 몸까지 날아가서 복귀
                if (!isReturning && Input.GetKeyDown(KeyCode.F))
                {
                    // 물건에 빙의 중이면 빠져나와서 그 자리에서 몸으로 날아감
                    if (Possessable.Current != null) Possessable.Current.Release();
                    if (SoulHook.Current != null) SoulHook.Current.Release(); // 고리에 걸려 있으면 줄을 끊고 복귀
                    StartCoroutine(FlyBackToBody());
                }
            }
        }
    }

    void HandleMovement()
    {
        if (Time.time < stunnedUntil) return;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 moveDir = new Vector3(horizontal, 0f, vertical).normalized;

        // 이동
        Vector3 velocity = moveDir * moveSpeed;
        Vector3 currentVelocity = rb.linearVelocity;
        rb.linearVelocity = new Vector3(velocity.x, currentVelocity.y, velocity.z);

        // 점프
        if (Input.GetButtonDown("Jump") && CheckGrounded())
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void StartSoulOut()
    {
        if (soulObject != null)
        {
            isSoulMode = true;
            Debug.Log("혼 모드 상태: " + isSoulMode);
            soulObject.transform.position = transform.position + Vector3.up * 1.5f;
            soulObject.SetActive(true);
            isSoulOut = true;

            // 본체 물리 멈춤
            rb.linearVelocity = Vector3.zero;
            if (ragdollWhenSoulOut)
            {
                savedConstraints = rb.constraints;
                savedYaw = transform.eulerAngles.y;
                rb.constraints = RigidbodyConstraints.None;
                Collapse();
            }
            else
            {
                rb.isKinematic = true;
            }
        }
    }

    // 혼이 경로를 따라 천천히 몸으로 날아가 합쳐진 뒤 복귀 (카메라는 혼을 따라 자연스럽게 몸으로 이동)
    System.Collections.IEnumerator FlyBackToBody()
    {
        isReturning = true;

        Transform soul = soulObject.transform;
        SoulController soulMove = soulObject.GetComponent<SoulController>();
        if (soulMove != null) soulMove.enabled = false;

        Vector3 start = soul.position;
        Vector3 end = transform.position; // 몸이 실려 가는 중일 수 있어서 매 프레임 갱신
        Vector3 startScale = soul.localScale;
        float duration = Mathf.Clamp(Vector3.Distance(start, end) / returnSpeed, minReturnTime, maxReturnTime);

        float t = 0f;
        while (t < 1f)
        {
            t = Mathf.Min(1f, t + Time.deltaTime / duration);
            float eased = Mathf.SmoothStep(0f, 1f, t);
            end = transform.position;
            Vector3 pos = Vector3.Lerp(start, end, eased) + Vector3.up * Mathf.Sin(eased * Mathf.PI) * returnArcHeight;
            soul.position = pos;
            soul.localScale = Vector3.Lerp(startScale, startScale * 0.3f, Mathf.InverseLerp(0.7f, 1f, t)); // 몸에 스며들듯 작아짐
            yield return null;
        }

        soul.localScale = startScale;
        if (soulMove != null) soulMove.enabled = true;
        isReturning = false;
        ReturnToBody();
    }

    void ReturnToBody()
    {
        if (soulObject != null)
        {
            isSoulMode = false;
            Debug.Log("혼 모드 상태: " + isSoulMode);
            soulObject.SetActive(false);
            isSoulOut = false;

            rb.isKinematic = false;
            if (ragdollWhenSoulOut)
            {
                StandUp();
                rb.constraints = savedConstraints;
            }
        }
    }

    // ── 쓰러지기 / 일어나기 ──
    // 그냥 회전만 시키면 길쭉한 몸이 벽·상자·수레에 파고들거나, 밀려나면서 바닥 아래로 빠졌음.
    // 그래서 들어갈 빈 자리와 발밑 바닥을 먼저 확인한 뒤 그 자리에 놓음.

    Vector3 BodyHalfExtents()
    {
        BoxCollider box = col as BoxCollider;
        Vector3 size = box != null ? box.size : col.bounds.size;
        return Vector3.Scale(size, transform.lossyScale) * 0.5f;
    }

    // 쓰러질 빈 자리를 앞 → 뒤 → 오른쪽 → 왼쪽 순서로 찾고, 없으면 선 채로 둠
    void Collapse()
    {
        Vector3 half = BodyHalfExtents();
        Quaternion yawRot = Quaternion.Euler(0f, savedYaw, 0f);
        Vector3 pos = rb.position;
        float feetY = pos.y - half.y;
        FindGround(pos, half.y + 0.5f, out float standY, out Collider standingOn);
        // 승강기가 바닥과 같은 높이에 붙어 있으면 둘 다 맞으니, 움직이는 발판 쪽을 기준으로 삼음
        foreach (RaycastHit hit in Physics.RaycastAll(pos, Vector3.down, half.y + 0.5f, ~0, QueryTriggerInteraction.Ignore))
            if (hit.collider.GetComponent<Possessable>() != null && Mathf.Abs(hit.point.y - standY) < 0.05f)
                standingOn = hit.collider;
        Vector3[] dirs = { yawRot * Vector3.forward, yawRot * Vector3.back, yawRot * Vector3.right, yawRot * Vector3.left };

        // 1차: 머리부터 발끝까지 지금 서 있는 바닥 위에 눕는 자리 (승강기·수레 밖으로 삐져나가지 않게)
        // 2차: 몸 가운데만 받쳐지면 되는 자리
        foreach (bool fullSupport in new[] { true, false })
        {
            foreach (float reach in new[] { half.y, 0f, -half.y * 0.5f })
            {
                foreach (Vector3 dir in dirs)
                {
                    Quaternion rot = Quaternion.AngleAxis(90f, Vector3.Cross(Vector3.up, dir)) * yawRot;
                    Vector3 center = new Vector3(pos.x, feetY + half.z + 0.02f, pos.z) + dir * reach;
                    if (!IsSpaceFree(center, half, rot)) continue;
                    if (!SupportedAt(center, feetY, null)) continue;
                    if (fullSupport && !(SupportedAt(center, feetY, standingOn) && SupportedAt(center + dir * half.y * 0.8f, feetY, standingOn) && SupportedAt(center - dir * half.y * 0.8f, feetY, standingOn))) continue;
                    PlaceBody(center, rot);
                    return;
                }
            }
        }
        // 쓰러질 자리가 없으면 선 채로 물건이 됨
    }

    // 쓰러진 자리 근처에서 설 수 있는 빈 자리를 찾아서 세움
    void StandUp()
    {
        Vector3 half = BodyHalfExtents();
        Quaternion rot = Quaternion.Euler(0f, savedYaw, 0f);
        Vector3 basePos = rb.position;
        FindGround(basePos, 3f, out float currentGroundY);

        // 1차: 지금 누워 있는 바닥과 같은 높이에서만 찾음 (옆 상자 위로 올라가 버리지 않게)
        // 2차: 높이가 달라도 설 수 있는 곳
        foreach (bool sameLevelOnly in new[] { true, false })
        {
            foreach (float radius in new[] { 0f, 0.5f, 1f, 1.5f, 2f })
            {
                int steps = radius == 0f ? 1 : 8;
                for (int i = 0; i < steps; i++)
                {
                    float angle = i * Mathf.PI * 2f / steps;
                    Vector3 probe = basePos + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
                    if (!FindGround(probe + Vector3.up * 2f, 4f, out float groundY)) continue;
                    if (sameLevelOnly && Mathf.Abs(groundY - currentGroundY) > 0.3f) continue;
                    Vector3 center = new Vector3(probe.x, groundY + half.y + 0.02f, probe.z);
                    if (IsSpaceFree(center, half, rot))
                    {
                        PlaceBody(center, rot);
                        return;
                    }
                }
            }
        }
        // 빈 자리를 못 찾으면 제자리에서 위로 띄워 세움
        PlaceBody(basePos + Vector3.up * (half.y - half.z + 0.02f), rot);
    }

    void PlaceBody(Vector3 center, Quaternion rot)
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.position = center;
        rb.rotation = rot;
        transform.SetPositionAndRotation(center, rot);
        Physics.SyncTransforms();
    }

    bool IsSpaceFree(Vector3 center, Vector3 half, Quaternion rot)
    {
        foreach (Collider c in Physics.OverlapBox(center, half * 0.95f, rot, ~0, QueryTriggerInteraction.Ignore))
        {
            if (c == col || c.attachedRigidbody == rb) continue;
            if (soulObject != null && c.transform.IsChildOf(soulObject.transform)) continue;
            return false;
        }
        return true;
    }

    // 그 지점 바로 아래에 지금 발 높이와 같은 바닥이 있는지 (sameSurface를 주면 그 물체 위여야 함)
    bool SupportedAt(Vector3 point, float feetY, Collider sameSurface)
    {
        foreach (RaycastHit hit in Physics.RaycastAll(new Vector3(point.x, feetY + 0.5f, point.z), Vector3.down, 1f, ~0, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider == col || hit.collider.attachedRigidbody == rb) continue;
            if (sameSurface != null && hit.collider != sameSurface) continue;
            if (Mathf.Abs(hit.point.y - feetY) < 0.3f) return true;
        }
        return false;
    }

    bool FindGround(Vector3 from, float distance, out float groundY)
    {
        return FindGround(from, distance, out groundY, out _);
    }

    // 자기 자신과 트리거는 무시하고 아래쪽 가장 가까운 바닥 높이
    bool FindGround(Vector3 from, float distance, out float groundY, out Collider surface)
    {
        groundY = 0f;
        surface = null;
        float best = float.MaxValue;
        foreach (RaycastHit hit in Physics.RaycastAll(from, Vector3.down, distance, ~0, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider == col || hit.collider.attachedRigidbody == rb) continue;
            if (soulObject != null && hit.collider.transform.IsChildOf(soulObject.transform)) continue;
            if (hit.distance < best)
            {
                best = hit.distance;
                groundY = hit.point.y;
                surface = hit.collider;
            }
        }
        return best < float.MaxValue;
    }

    // 점프 순간에 발밑을 직접 검사
    // (충돌 Enter/Exit로 기록하면 바닥 두 개에 걸쳐 있거나, 벽에 붙었거나, 혼 모드에서 돌아올 때 값이 꼬였음)
    bool CheckGrounded()
    {
        Bounds b = col.bounds;
        Vector3 feet = new Vector3(b.center.x, b.min.y, b.center.z);
        Vector3 halfSize = new Vector3(b.extents.x * 0.9f, 0.1f, b.extents.z * 0.9f);
        return Physics.CheckBox(feet, halfSize, Quaternion.identity, groundLayer, QueryTriggerInteraction.Ignore);
    }
}
