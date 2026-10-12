using UnityEngine;
using System.Collections.Generic;

// 무게 발판
// 몸 또는 상자(PushBox)가 올라가 있는 동안 눌림 상태. 눌려 있는 동안 문(door)이 사라짐.
public class WeightPlate : MonoBehaviour
{
    public GameObject door;           // 눌려 있는 동안 비활성화할 오브젝트
    public float pressDepth = 0.08f;  // 눌렸을 때 내려가는 정도
    public Color idleColor = new Color(0.8f, 0.6f, 0.2f);
    public Color pressedColor = Color.green;

    private Vector3 checkCenter;      // 발판 위 무게를 검사하는 영역 (눌려서 내려가도 고정)
    private Vector3 checkHalfExtents;
    private Vector3 upPos;
    private Renderer rend;
    private bool isPressed = false;

    public bool IsPressed => isPressed;

    void Start()
    {
        upPos = transform.localPosition;
        rend = GetComponent<Renderer>();
        Bounds b = GetComponent<Collider>().bounds;
        checkCenter = new Vector3(b.center.x, b.max.y + 0.2f, b.center.z);
        checkHalfExtents = new Vector3(b.extents.x * 0.95f, 0.3f, b.extents.z * 0.95f);
        Apply(false);
    }

    // 트리거 Enter/Exit는 물체가 멈춰 잠들거나 발판이 내려갈 때 빠지는 경우가 있어서 매 프레임 직접 검사함
    void FixedUpdate()
    {
        bool pressed = false;
        foreach (Collider c in Physics.OverlapBox(checkCenter, checkHalfExtents, Quaternion.identity, ~0, QueryTriggerInteraction.Ignore))
        {
            if (IsWeight(c)) { pressed = true; break; }
        }
        if (pressed != isPressed)
            Apply(pressed);
    }

    bool IsWeight(Collider other)
    {
        // 몸(본체)과 상자만 무게로 인정, 혼은 무게가 없음
        return other.GetComponent<PlayerController>() != null || other.GetComponent<PushBox>() != null
            || other.GetComponent<Possessable>() != null; // 혼이 빙의한 물건도 무게가 있음
    }

    void Apply(bool pressed)
    {
        isPressed = pressed;
        transform.localPosition = pressed ? upPos + Vector3.down * pressDepth : upPos;
        if (rend != null) rend.material.color = pressed ? pressedColor : idleColor;
        if (door != null) door.SetActive(!pressed);
        Debug.Log(pressed ? "무게 발판 눌림 → 문 열림" : "무게 발판 해제 → 문 닫힘");
    }
}
