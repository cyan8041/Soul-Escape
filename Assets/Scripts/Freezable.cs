using UnityEngine;

// 혼이 닿으면 freezeDuration초 동안 멈추는 오브젝트. MovingPlatform, Rotator가 IsFrozen을 보고 움직임을 멈춤.
// 혼은 높이가 고정이라 위아래로 움직이는 발판도 위에서 내려다보며 멈출 수 있게 수평 거리로 판단함.
public class Freezable : MonoBehaviour
{
    public float freezeDuration = 6f;
    public float touchRange = 0.8f;
    public Color frozenColor = new Color(0.5f, 0.85f, 1f);

    private float frozenUntil = -1f;
    private Collider[] cols;
    private Renderer[] rends;
    private Color[] baseColors;
    private SoulController soul;

    public bool IsFrozen => Time.time < frozenUntil;
    public float Remaining => Mathf.Max(0f, frozenUntil - Time.time);

    void Start()
    {
        cols = GetComponentsInChildren<Collider>();
        rends = GetComponentsInChildren<Renderer>();
        baseColors = new Color[rends.Length];
        for (int i = 0; i < rends.Length; i++) baseColors[i] = rends[i].material.color;
    }

    void Update()
    {
        if (soul == null) soul = FindAnyObjectByType<SoulController>();
        if (soul != null && soul.gameObject.activeInHierarchy && SoulTouching(soul.transform.position))
            frozenUntil = Time.time + freezeDuration; // 닿아 있는 동안은 계속 처음부터

        // 얼어 있으면 하늘색, 1.5초 남으면 깜빡임
        bool showFrozen = IsFrozen && (Remaining > 1.5f || Mathf.Repeat(Time.time * 6f, 1f) < 0.5f);
        for (int i = 0; i < rends.Length; i++)
            rends[i].material.color = showFrozen ? frozenColor : baseColors[i];
    }

    bool SoulTouching(Vector3 soulPos)
    {
        foreach (Collider c in cols)
        {
            Vector3 probe = new Vector3(soulPos.x, c.bounds.center.y, soulPos.z);
            Vector3 closest = c.ClosestPoint(probe);
            if (Vector2.Distance(new Vector2(closest.x, closest.z), new Vector2(soulPos.x, soulPos.z)) <= touchRange)
                return true;
        }
        return false;
    }
}
