using UnityEngine;

// 혼이 몸에서 maxDistance 이상 멀어지지 못하게 묶어둠 (혼 오브젝트에 붙임)
// 몸을 어디에 세워두고 혼을 꺼낼지가 퍼즐이 됨
public class SoulLeash : MonoBehaviour
{
    public Transform body;
    public float maxDistance = 7f;
    public LineRenderer line; // 선택: 몸과 혼을 잇는 선

    void LateUpdate()
    {
        if (body == null) return;

        Vector3 offset = transform.position - body.position;
        offset.y = 0f;
        if (offset.magnitude > maxDistance)
        {
            Vector3 clamped = body.position + offset.normalized * maxDistance;
            transform.position = new Vector3(clamped.x, transform.position.y, clamped.z);
        }

        if (line != null)
        {
            line.SetPosition(0, body.position);
            line.SetPosition(1, transform.position);
        }
    }
}
