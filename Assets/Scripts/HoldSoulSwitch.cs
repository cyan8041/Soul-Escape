using UnityEngine;

// 혼이 닿아 있는 동안만 켜지는 스위치. 혼이 떠나거나 몸으로 돌아가면 꺼짐.
public class HoldSoulSwitch : MonoBehaviour
{
    public Color offColor = new Color(0.5f, 0.3f, 0.8f);
    public Color onColor = Color.green;

    private readonly System.Collections.Generic.HashSet<Collider> soulsInside = new System.Collections.Generic.HashSet<Collider>(); // 혼과 잔상이 같이 들어올 수 있음
    private Renderer rend;

    public bool IsOn
    {
        get
        {
            soulsInside.RemoveWhere(c => c == null || !c.gameObject.activeInHierarchy);
            return soulsInside.Count > 0;
        }
    }

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        if (rend != null) rend.material.color = IsOn ? onColor : offColor;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Soul")) soulsInside.Add(other);
    }

    private void OnTriggerExit(Collider other)
    {
        soulsInside.Remove(other);
    }
}
