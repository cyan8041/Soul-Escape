using UnityEngine;
using System.Collections.Generic;

// 혼의 고리. 혼 상태에서 고리 근처(위에서 내려다봐서 가까이)에 가서 E를 누르면 혼이 고리에 걸리고,
// 쓰러진 몸과 고리 사이에 줄이 생김. W로 줄을 감아 몸을 끌어당기고 S로 풀어줌.
// 다시 E를 누르면 줄을 끊어서 몸이 그대로 떨어지거나 날아감. F를 누르면 줄을 끊고 몸으로 복귀.
public class SoulHook : MonoBehaviour
{
    public static SoulHook Current { get; private set; }
    private static readonly List<SoulHook> all = new List<SoulHook>();

    public float latchRange = 1.5f;
    public float reelSpeed = 4f;
    public float minRope = 1.2f;
    public float maxRope = 20f;
    public float spring = 60f;
    public float damper = 10f;
    public float maxBodySpeed = 10f; // 줄에 끌려가는 몸이 너무 세게 튕기지 않게
    public Color idleColor = new Color(1f, 0.85f, 0.3f);
    public Color highlightColor = Color.white;
    public Color latchedColor = new Color(0.4f, 1f, 1f);

    private Renderer rend;
    private LineRenderer line;
    private SpringJoint joint;
    private Rigidbody body;
    private GameObject soul;
    private int latchedFrame;
    private float soulHeight; // 고리에서 풀려날 때 원래 떠 있던 높이로
    [System.NonSerialized] public float reelOverride; // 자동 테스트용 (평소엔 0)

    void Awake()
    {
        rend = GetComponent<Renderer>();
        line = gameObject.AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.startWidth = line.endWidth = 0.08f;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        if (rend != null) line.sharedMaterial = rend.sharedMaterial;
        line.enabled = false;
    }

    void OnEnable() { all.Add(this); }

    void OnDisable()
    {
        all.Remove(this);
        if (Current == this) Release();
    }

    public static SoulHook FindNear(Vector3 pos)
    {
        foreach (SoulHook h in all)
        {
            Vector2 d = new Vector2(h.transform.position.x - pos.x, h.transform.position.z - pos.z);
            if (d.magnitude <= h.latchRange) return h;
        }
        return null;
    }

    // 혼을 고리에 걸고 몸과 줄로 연결
    public bool Latch(GameObject soulObject)
    {
        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (Current != null || player == null) return false;
        body = player.GetComponent<Rigidbody>();
        if (body.isKinematic) return false; // 몸이 물리 상태(쓰러진 몸)일 때만 끌 수 있음

        Current = this;
        soul = soulObject;
        soulHeight = soul.transform.position.y;
        latchedFrame = Time.frameCount;
        soul.GetComponent<SoulController>().enabled = false;
        soul.transform.position = transform.position;

        joint = body.gameObject.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedBody = null;
        joint.connectedAnchor = transform.position;
        joint.anchor = Vector3.zero;
        joint.spring = spring;
        joint.damper = damper;
        joint.minDistance = 0f;
        joint.maxDistance = Mathf.Min(maxRope, Vector3.Distance(body.position, transform.position));
        joint.enableCollision = true;
        body.WakeUp();
        line.enabled = true;
        return true;
    }

    // 줄을 끊음. 혼은 고리에서 풀려나 다시 움직일 수 있음
    public void Release()
    {
        if (Current != this) return;
        Current = null;
        if (joint != null) Destroy(joint);
        joint = null;
        line.enabled = false;
        if (soul != null)
        {
            Vector3 p = transform.position;
            soul.transform.position = new Vector3(p.x, soulHeight, p.z);
            soul.GetComponent<SoulController>().enabled = true;
        }
    }

    void FixedUpdate()
    {
        if (Current != this || body == null) return;
        if (body.linearVelocity.magnitude > maxBodySpeed)
            body.linearVelocity = body.linearVelocity.normalized * maxBodySpeed;
    }

    void Update()
    {
        bool near = false;
        if (Current == null)
        {
            SoulController s = FindAnyObjectByType<SoulController>();
            near = s != null && FindNear(s.transform.position) == this;
        }
        if (rend != null) rend.material.color = Current == this ? latchedColor : (near ? highlightColor : idleColor);

        if (Current != this) return;
        if (soul == null || !soul.activeInHierarchy || joint == null) { Release(); return; }

        soul.transform.position = transform.position;
        line.SetPosition(0, transform.position);
        line.SetPosition(1, body.position);

        if (Input.GetKeyDown(KeyCode.E) && Time.frameCount != latchedFrame) { Release(); return; }

        float reel = reelOverride != 0f ? reelOverride : Input.GetAxis("Vertical"); // W: 감기, S: 풀기
        if (Mathf.Abs(reel) > 0.01f)
        {
            joint.maxDistance = Mathf.Clamp(joint.maxDistance - reel * reelSpeed * Time.deltaTime, minRope, maxRope);
            body.WakeUp();
        }
    }
}
