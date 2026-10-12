using UnityEngine;

// 몸이 부딪히면 축 바깥쪽으로 튕겨냄 (휘두르는 막대에 맞으면 다리 밖으로 날아감)
public class Knockback : MonoBehaviour
{
    public float sideForce = 9f;
    public float upForce = 4f;
    public float cooldown = 0.5f;

    private float lastHit = -10f;

    void OnCollisionEnter(Collision collision)
    {
        if (Time.time - lastHit < cooldown) return;
        Rigidbody body = collision.rigidbody;
        if (body == null || body.GetComponent<PlayerController>() == null) return;

        Vector3 away = body.position - transform.position;
        away.y = 0f;
        if (away.sqrMagnitude < 0.01f) away = transform.right;
        body.GetComponent<PlayerController>().Stun(0.8f);
        body.AddForce(away.normalized * sideForce + Vector3.up * upForce, ForceMode.Impulse);
        lastHit = Time.time;
    }
}
