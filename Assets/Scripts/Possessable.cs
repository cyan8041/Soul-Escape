using UnityEngine;
using System.Collections.Generic;

// 혼이 빙의해서 직접 움직일 수 있는 물건. 혼 상태에서 가까이 가서 E키로 빙의, 다시 E키로 빠져나옴.
// Roll: 공처럼 굴러감 (무게가 있어서 발판을 누를 수 있음)
// Rail: 레일 위에서만 앞뒤로 움직이는 수레. 위에 올라탄 물체(혼이 빠진 몸 등)를 같이 실어 나름.
[RequireComponent(typeof(Rigidbody))]
public class Possessable : MonoBehaviour
{
    public enum Mode { Roll, Rail }

    public static Possessable Current { get; private set; }
    private static readonly List<Possessable> all = new List<Possessable>();

    public Mode mode = Mode.Roll;

    [Header("Roll")]
    public float moveForce = 25f;
    public float maxSpeed = 6f;

    [Header("Rail")]
    public Transform railStart;
    public Transform railEnd;
    public float railSpeed = 4f;

    [Header("표시")]
    public Color highlightColor = new Color(0.6f, 0.9f, 1f); // 혼이 빙의 가능한 거리에 있을 때
    public Color possessedColor = new Color(0.3f, 1f, 1f);   // 빙의 중

    private Rigidbody rb;
    private Renderer rend;
    private Color baseColor;
    private Vector3 spawnPos;
    private GameObject soul;
    private int possessedFrame;
    private readonly HashSet<Rigidbody> riders = new HashSet<Rigidbody>();

    public bool IsPossessed => Current == this;

    [System.NonSerialized] public Vector3 inputOverride; // 자동 테스트용 입력 (평소엔 0)

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rend = GetComponent<Renderer>();
        if (rend != null) baseColor = rend.material.color;
        spawnPos = transform.position;

        if (mode == Mode.Rail)
        {
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }

    void OnEnable() { all.Add(this); }

    void OnDisable()
    {
        all.Remove(this);
        if (Current == this) Current = null;
    }

    // 혼 위치에서 range 안에 있는 가장 가까운 빙의 대상 (혼은 높이가 고정이라 수평 거리로 판단)
    public static Possessable FindNear(Vector3 pos, float range)
    {
        Possessable best = null;
        float bestDist = range;
        foreach (Possessable p in all)
        {
            Collider c = p.GetComponent<Collider>();
            Vector3 probe = c != null ? new Vector3(pos.x, c.bounds.center.y, pos.z) : pos;
            Vector3 closest = c != null ? c.ClosestPoint(probe) : p.transform.position;
            float d = Vector2.Distance(new Vector2(pos.x, pos.z), new Vector2(closest.x, closest.z));
            if (d <= bestDist)
            {
                bestDist = d;
                best = p;
            }
        }
        return best;
    }

    public void Possess(GameObject soulObject)
    {
        if (Current != null) return;
        Current = this;
        soul = soulObject;
        soul.SetActive(false);
        possessedFrame = Time.frameCount;
        SetColor(possessedColor);
    }

    // 빙의 해제. 혼은 물건 위로 빠져나옴
    public void Release()
    {
        if (Current != this) return;
        Current = null;
        SetColor(baseColor);
        if (soul != null)
        {
            soul.transform.position = transform.position + Vector3.up * 1.5f;
            soul.SetActive(true);
        }
    }

    void Update()
    {
        // 굴러가는 물건이 낭떠러지로 떨어지면 처음 위치로 복귀 (빙의 중이어도)
        if (mode == Mode.Roll && transform.position.y < spawnPos.y - 10f)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            transform.position = spawnPos;
        }

        if (IsPossessed)
        {
            if (Input.GetKeyDown(KeyCode.E) && Time.frameCount != possessedFrame)
                Release();
            return;
        }

        // 혼이 빙의 가능한 거리에 있으면 색으로 알려줌
        bool near = false;
        SoulController soulNearby = FindAnyObjectByType<SoulController>();
        if (Current == null && soulNearby != null && FindNear(soulNearby.transform.position, soulNearby.possessRange) == this)
            near = true;
        SetColor(near ? highlightColor : baseColor);
    }

    void FixedUpdate()
    {
        Vector3 input = Vector3.zero;
        if (IsPossessed)
            input = inputOverride != Vector3.zero ? inputOverride : new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));

        if (mode == Mode.Roll)
        {
            if (input.sqrMagnitude > 0.01f)
            {
                rb.AddForce(input.normalized * moveForce, ForceMode.Force);
                Vector3 flat = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                if (flat.magnitude > maxSpeed)
                {
                    flat = flat.normalized * maxSpeed;
                    rb.linearVelocity = new Vector3(flat.x, rb.linearVelocity.y, flat.z);
                }
            }
        }
        else if (railStart != null && railEnd != null)
        {
            Vector3 dir = (railEnd.position - railStart.position).normalized;
            // 세로 레일(승강기)은 W/S로 위아래, 가로 레일은 누른 방향 그대로
            float along = Mathf.Abs(dir.y) > 0.7f ? input.z * Mathf.Sign(dir.y) : Vector3.Dot(input, dir);
            if (Mathf.Abs(along) < 0.01f) return;

            // 레일 구간 안으로 제한
            float length = Vector3.Distance(railStart.position, railEnd.position);
            float t = Vector3.Dot(rb.position - railStart.position, dir);
            float nextT = Mathf.Clamp(t + along * railSpeed * Time.fixedDeltaTime, 0f, length);
            Vector3 next = railStart.position + dir * nextT;
            Vector3 delta = next - rb.position;
            if (delta.sqrMagnitude < 0.000001f) return;

            // 위에 올라탄 물체를 움직이기 전에 찾아둠 (멈춰 잠든 물체는 충돌 이벤트가 안 와서 직접 검사)
            FindRiders();
            // MovePosition은 마찰로 위 물체를 한 번 더 끌고 가서 미끄러져 떨어짐. 순간이동으로 옮기고 탑승물은 같은 만큼만 옮김
            rb.position = next;
            foreach (Rigidbody r in riders)
            {
                r.WakeUp();
                r.position += delta;
                if (delta.y > 0f && r.linearVelocity.y < 0f)
                    r.linearVelocity = new Vector3(r.linearVelocity.x, 0f, r.linearVelocity.z); // 올라가는 중엔 떨어지는 속도가 쌓이지 않게
            }
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

    void SetColor(Color c)
    {
        if (rend != null) rend.material.color = c;
    }
}
