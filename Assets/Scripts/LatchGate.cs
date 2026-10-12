using UnityEngine;

// 연결된 무게 발판과 혼 스위치가 "동시에" 모두 켜지면 문이 열리고 계속 열려 있음
public class LatchGate : MonoBehaviour
{
    public WeightPlate[] plates;
    public HoldSoulSwitch[] soulSwitches;

    private bool isOpen = false;

    void Update()
    {
        if (isOpen) return;

        foreach (WeightPlate p in plates)
            if (p == null || !p.IsPressed) return;
        foreach (HoldSoulSwitch s in soulSwitches)
            if (s == null || !s.IsOn) return;

        isOpen = true;
        Debug.Log("조건 동시 충족 → 문 열림");
        gameObject.SetActive(false);
    }
}
