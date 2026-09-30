using UnityEngine;

public class Chanthink_SoulController : MonoBehaviour
{
    public float floatSpeed = 5f;

    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 move = new Vector3(h, 0, v);
        transform.Translate(move * floatSpeed * Time.deltaTime);
    }
}

