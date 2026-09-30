using UnityEngine;

public class Chanthink_FollowCamera : MonoBehaviour
{
    public Transform playerBody;
    public Transform soul;
    public float smoothSpeed = 5f;
    public Vector3 offset;

    private bool isSoulActive = false;

    void Update()
    {
        if (playerBody == null) return;

        Transform target = playerBody;

        if (soul != null && soul.gameObject.activeSelf)
        {
            target = soul;
        }

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        transform.position = smoothedPosition;

        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        transform.LookAt(target);
    }

}
