using UnityEngine;
using System.Collections.Generic;

// 혼의 녹화. 혼 상태에서 R을 누르면 혼의 움직임을 녹화하고, 다시 R을 누르면 멈춤.
// 멈추는 순간 녹화를 시작한 자리에 잔상(SoulGhost)이 나타나 같은 경로를 그대로 따라간 뒤 마지막 자리에 머무름.
// 잔상도 혼처럼 혼 스위치를 누를 수 있어서 "혼이 둘인" 퍼즐이 됨. 잔상은 한 번에 하나만 있고, 새로 녹화하면 바뀜.
public class SoulRecorder : MonoBehaviour
{
    public float maxDuration = 10f;
    public Color recordingColor = new Color(1f, 0.35f, 0.35f, 0.6f);

    private readonly List<Vector3> points = new List<Vector3>();
    private readonly List<float> times = new List<float>();
    private bool recording;
    private float startTime;
    private Renderer rend;
    private Color baseColor;
    private SoulGhost ghost;

    public bool IsRecording => recording;
    [System.NonSerialized] public bool toggleRequest; // 자동 테스트용 (R 대신)

    void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend != null) baseColor = rend.material.color;
    }

    void Update()
    {
        bool toggle = Input.GetKeyDown(KeyCode.R) || toggleRequest;
        toggleRequest = false;
        if (toggle)
        {
            if (recording) StopRecording();
            else StartRecording();
        }

        if (recording)
        {
            points.Add(transform.position);
            times.Add(Time.time - startTime);
            if (Time.time - startTime >= maxDuration) StopRecording();
        }
    }

    void OnDisable()
    {
        // 몸으로 돌아가면(혼이 꺼지면) 녹화도 거기서 끝
        if (recording) StopRecording();
    }

    void StartRecording()
    {
        points.Clear();
        times.Clear();
        recording = true;
        startTime = Time.time;
        if (rend != null) rend.material.color = recordingColor;
    }

    void StopRecording()
    {
        recording = false;
        if (rend != null) rend.material.color = baseColor;
        if (points.Count < 2) return;

        if (ghost != null) Destroy(ghost.gameObject);
        ghost = SoulGhost.Spawn(gameObject, points, times);
        Debug.Log("혼 녹화 " + times[times.Count - 1].ToString("F1") + "초 → 잔상 재생");
    }
}
