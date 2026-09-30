using UnityEngine;

public class Chanthink_PlayerController : MonoBehaviour
{
    public GameObject soulObject;  // 씬에 두고 비활성화 상태인 유체 오브젝트

    private Rigidbody rb;
    public float moveSpeed = 5f;
    public float jumpForce = 7f;

    public bool isSoulMode = false;
    private bool isSoulOut = false;
    private bool isGrounded = true;

    public LayerMask groundLayer; // 바닥 레이어 지정

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (soulObject != null)
            soulObject.SetActive(false);
        GameObject startPoint = GameObject.Find("StartPoint");
        if (startPoint != null)
        {
            transform.position = startPoint.transform.position;
        }
        else
        {
            Debug.LogWarning("StartPoint 오브젝트를 찾을 수 없습니다.");
        }
    }

    void Update()
    {
        if (!isSoulOut)
        {
            HandleMovement();

            if (Input.GetKeyDown(KeyCode.F))
            {
                StartSoulOut();
            }
        }
        else
        {
            // 유체 상태일 땐 본체 움직임 제한

            if (soulObject != null)
            {
                // 유체 이동은 SoulController에서 처리

                // 본체와 유체 거리 체크 후 복귀 가능
                float dist = Vector3.Distance(transform.position, soulObject.transform.position);
                if (dist < 3f && Input.GetKeyDown(KeyCode.F))
                {
                    ReturnToBody();
                }
            }
        }
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 moveDir = new Vector3(horizontal, 0f, vertical).normalized;

        // 이동
        Vector3 velocity = moveDir * moveSpeed;
        Vector3 currentVelocity = rb.linearVelocity;
        rb.linearVelocity = new Vector3(velocity.x, currentVelocity.y, velocity.z);

        // 점프
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false; // 점프 직후엔 땅에 닿지 않은 상태
        }
    }

    void StartSoulOut()
    {
        if (soulObject != null)
        {
            isSoulMode = true;
            Debug.Log("혼 모드 상태: " + isSoulMode);
            soulObject.transform.position = transform.position + Vector3.up * 1.5f;
            soulObject.SetActive(true);
            isSoulOut = true;

            // 본체 물리 멈춤
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
        }
    }

    void ReturnToBody()
    {
        if (soulObject != null)
        {
            isSoulMode = false;
            Debug.Log("혼 모드 상태: " + isSoulMode);
            soulObject.SetActive(false);
            isSoulOut = false;

            rb.isKinematic = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 바닥에 닿으면 isGrounded true
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        // 바닥에서 떨어지면 isGrounded false
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            isGrounded = false;
        }
    }
}
