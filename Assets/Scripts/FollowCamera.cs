using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform playerBody;
    public Transform soul;
    public float smoothSpeed = 5f;
    public Vector3 offset;
    public float returnTime = 0.35f;   // time to glide back to the body after returning from soul mode
    public float rotateSpeed = 8f;

    private Vector3 velocity;

    void LateUpdate()
    {
        if (playerBody == null) return;

        Transform target = playerBody;
        if (soul != null && soul.gameObject.activeSelf)
        {
            target = soul;
        }
        if (Possessable.Current != null)
        {
            target = Possessable.Current.transform; // 혼이 빙의한 물건을 따라감
        }

        Vector3 desiredPosition = target.position + offset;
        if (target == playerBody)
        {
            // Smooth glide that eases out, so a long-distance return to the body still feels natural
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, returnTime);
        }
        else
        {
            velocity = Vector3.zero;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        }

        Vector3 lookDir = target.position - transform.position;
        if (lookDir.sqrMagnitude > 0.0001f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotateSpeed * Time.deltaTime);
        }
    }
}
