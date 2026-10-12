using UnityEngine;

// 혼 전용 다리 발판
// 평소엔 보이지도 밟히지도 않음. 혼을 꺼내면 반투명하게 보이고,
// 혼이 지나간 발판은 "켜짐" 상태가 되어 혼이 몸으로 돌아온 뒤 litDuration초 동안 몸이 밟을 수 있음.
public class SoulBridgeTile : MonoBehaviour
{
    public float litDuration = 12f;     // 혼 복귀 후 발판이 유지되는 시간
    public float blinkTime = 3f;        // 꺼지기 전 깜빡이는 시간
    public Color ghostColor = new Color(0.6f, 0.8f, 1f, 0.15f); // 혼에게만 보이는 상태
    public Color litColor = new Color(0.4f, 0.9f, 1f, 0.6f);    // 혼이 켜둔 상태

    private PlayerController player;
    private Renderer rend;
    private Collider col;
    private MaterialPropertyBlock block;

    private bool isLit = false;
    private float litTimer = 0f;

    void Start()
    {
        player = FindFirstObjectByType<PlayerController>();
        rend = GetComponent<Renderer>();
        col = GetComponent<Collider>();
        block = new MaterialPropertyBlock();
        Refresh();
    }

    void Update()
    {
        bool soulOut = player != null && player.soulObject != null && player.soulObject.activeSelf;

        if (soulOut)
        {
            // 혼이 발판 위(XZ 기준)를 지나가면 켜짐
            Vector3 p = player.soulObject.transform.position;
            Bounds b = rend.bounds;
            if (p.x >= b.min.x && p.x <= b.max.x && p.z >= b.min.z && p.z <= b.max.z)
                isLit = true;

            if (isLit)
                litTimer = litDuration; // 혼이 나와 있는 동안은 타이머가 줄지 않음
        }
        else if (isLit)
        {
            litTimer -= Time.deltaTime;
            if (litTimer <= 0f)
                isLit = false;
        }

        Refresh();
    }

    void Refresh()
    {
        bool soulOut = player != null && player.soulObject != null && player.soulObject.activeSelf;

        col.enabled = isLit;
        rend.enabled = isLit || soulOut;

        Color c = isLit ? litColor : ghostColor;
        // 꺼지기 직전엔 깜빡임
        if (isLit && !soulOut && litTimer < blinkTime && Mathf.Repeat(Time.time * 4f, 1f) < 0.5f)
            c.a *= 0.3f;

        rend.GetPropertyBlock(block);
        block.SetColor("_BaseColor", c);
        rend.SetPropertyBlock(block);
    }
}
