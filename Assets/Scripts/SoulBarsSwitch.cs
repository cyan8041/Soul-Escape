using UnityEngine;

// 철창 안쪽에 두는 혼 전용 스위치. 혼이 닿으면 연결된 철창을 연다.
public class SoulBarsSwitch : MonoBehaviour
{
    public SoulBars bars;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Soul") && bars != null)
        {
            bars.Open();
            GetComponent<Renderer>().material.color = Color.green;
        }
    }
}
