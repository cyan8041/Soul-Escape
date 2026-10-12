using UnityEngine;

// 시간제한 기둥. 닿으면 duration초 동안 켜지고, 남은 시간만큼 기둥 높이가 줄어듦.
// 혼이 기둥을 누른 뒤 F로 몸까지 날아가는 시간도 이 안에 포함되므로, 몸을 어디에 세워둘지가 퍼즐이 됨.
[RequireComponent(typeof(Collider))]
public class TimedSwitch : MonoBehaviour
{
    public float duration = 5f;
    public bool byBody = false;
    public bool bySoul = true;
    public bool byObject = true;
    public Color offColor = new Color(0.6f, 0.3f, 0.9f);
    public Color onColor = Color.green;
    public Color lowColor = Color.red;   // 1.5초 이하 남았을 때

    private float endTime = -1f;
    private Vector3 baseScale;
    private Vector3 basePos;
    private Renderer rend;

    public bool IsOn => Time.time < endTime;
    public float Remaining => Mathf.Max(0f, endTime - Time.time);

    void Start()
    {
        GetComponent<Collider>().isTrigger = true;
        rend = GetComponent<Renderer>();
        baseScale = transform.localScale;
        basePos = transform.position;
    }

    void Update()
    {
        // 남은 시간만큼 기둥이 바닥 쪽으로 줄어듦
        float ratio = IsOn ? Remaining / duration : 1f;
        float h = baseScale.y * Mathf.Max(ratio, 0.05f);
        transform.localScale = new Vector3(baseScale.x, h, baseScale.z);
        transform.position = basePos + Vector3.down * (baseScale.y - h) * 0.5f;

        if (rend != null)
        {
            Color c = !IsOn ? offColor : (Remaining <= 1.5f ? lowColor : onColor);
            rend.material.color = c;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!Accepts(other)) return;
        endTime = Time.time + duration; // 다시 닿으면 처음부터
        Debug.Log(name + " 켜짐 (" + duration + "초)");
    }

    bool Accepts(Collider other)
    {
        if (byBody && other.GetComponent<PlayerController>() != null) return true;
        if (bySoul && other.CompareTag("Soul")) return true;
        if (byObject && other.GetComponent<Possessable>() != null) return true;
        return false;
    }
}
