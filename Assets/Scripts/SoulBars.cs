using UnityEngine;

// 창살 / 철창
// 혼은 원래 벽을 통과하므로 그대로 지나가고, 몸은 창살 콜라이더에 막힘.
// Open()이 호출되면 창살이 바닥으로 내려가며 열림.
public class SoulBars : MonoBehaviour
{
    public float sinkDepth = 3f;   // 열릴 때 내려가는 깊이
    public float sinkSpeed = 3f;

    private bool isOpen = false;
    private Vector3 openPos;

    public void Open()
    {
        if (isOpen) return;
        isOpen = true;
        openPos = transform.position + Vector3.down * sinkDepth;

        foreach (Collider c in GetComponentsInChildren<Collider>())
            c.enabled = false;

        Debug.Log("철창 열림!");
    }

    void Update()
    {
        if (!isOpen) return;
        transform.position = Vector3.MoveTowards(transform.position, openPos, sinkSpeed * Time.deltaTime);
        if (transform.position == openPos)
            gameObject.SetActive(false);
    }
}
