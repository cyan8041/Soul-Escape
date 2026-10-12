using UnityEngine;
using System.Collections.Generic;

// 녹화된 혼의 경로를 그대로 따라가는 잔상. 끝까지 가면 마지막 자리에 머무름.
// "Soul" 태그와 트리거를 가져서 혼 스위치(HoldSoulSwitch, SoulSwitch 등)를 혼처럼 누름.
public class SoulGhost : MonoBehaviour
{
    private List<Vector3> points;
    private List<float> times;
    private float startTime;
    private int index;

    public static SoulGhost Spawn(GameObject soul, List<Vector3> pts, List<float> ts)
    {
        GameObject go = new GameObject("SoulGhost");
        go.tag = "Soul";
        go.transform.position = pts[0];
        go.transform.localScale = soul.transform.lossyScale;

        MeshFilter srcMesh = soul.GetComponent<MeshFilter>();
        MeshRenderer srcRend = soul.GetComponent<MeshRenderer>();
        if (srcMesh != null && srcRend != null)
        {
            go.AddComponent<MeshFilter>().sharedMesh = srcMesh.sharedMesh;
            MeshRenderer r = go.AddComponent<MeshRenderer>();
            r.material = srcRend.sharedMaterial;
            Color c = new Color(0.7f, 0.5f, 1f, 0.45f); // 보라색 반투명 잔상
            r.material.color = c;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        SphereCollider col = go.AddComponent<SphereCollider>();
        SphereCollider srcCol = soul.GetComponent<SphereCollider>();
        col.radius = srcCol != null ? srcCol.radius : 0.5f;
        col.isTrigger = true; // 몸을 밀지 않음
        Rigidbody rb = go.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        SoulGhost g = go.AddComponent<SoulGhost>();
        g.points = new List<Vector3>(pts);
        g.times = new List<float>(ts);
        g.startTime = Time.time;
        return g;
    }

    void Update()
    {
        float t = Time.time - startTime;
        while (index < times.Count - 1 && times[index + 1] <= t) index++;
        if (index >= times.Count - 1)
        {
            transform.position = points[points.Count - 1];
            return;
        }
        float seg = times[index + 1] - times[index];
        float k = seg > 0f ? (t - times[index]) / seg : 1f;
        transform.position = Vector3.Lerp(points[index], points[index + 1], k);
    }
}
