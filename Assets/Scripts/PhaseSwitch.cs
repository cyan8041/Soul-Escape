using UnityEngine;

// 전환 기둥. 몸·혼·빙의한 물건 중 허용된 것이 닿으면 모든 PhaseWall의 통과 규칙이 뒤바뀜.
[RequireComponent(typeof(Collider))]
public class PhaseSwitch : MonoBehaviour
{
    public bool byBody = true;
    public bool bySoul = true;
    public bool byObject = true;     // 빙의할 수 있는 물건(공 등)
    public float cooldown = 0.6f;
    public Color idleColor = new Color(1f, 0.8f, 0.2f);
    public Color flashColor = Color.white;

    private float lastToggle = -10f;
    private Renderer rend;

    void Start()
    {
        GetComponent<Collider>().isTrigger = true;
        rend = GetComponent<Renderer>();
        if (rend != null) rend.material.color = idleColor;
    }

    void Update()
    {
        if (rend != null)
            rend.material.color = Color.Lerp(flashColor, idleColor, (Time.time - lastToggle) / 0.4f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (Time.time - lastToggle < cooldown) return;
        if (!Accepts(other)) return;
        lastToggle = Time.time;
        PhaseWall.Toggle();
        Debug.Log(name + " 전환 → 벽 통과 규칙이 뒤바뀜");
    }

    bool Accepts(Collider other)
    {
        if (byBody && other.GetComponent<PlayerController>() != null) return true;
        if (bySoul && other.CompareTag("Soul")) return true;
        if (byObject && other.GetComponent<Possessable>() != null) return true;
        return false;
    }
}
