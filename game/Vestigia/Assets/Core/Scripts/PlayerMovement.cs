using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 3f;
    [SerializeField] private Animator animator;

    private Rigidbody rb;
    private Transform cameraTransform;
    private Vector3 moveDirection;
    private Vector3 lastFacingDirection = Vector3.forward;
    public Vector3 LastFacingDirection => lastFacingDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        Camera mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError(
                "PlayerMovement: No se encontró una cámara con el tag MainCamera."
            );

            enabled = false;
            return;
        }

        cameraTransform = mainCamera.transform;

        if (animator == null)
        {
            Debug.LogError(
                "PlayerMovement: No se asignó el Animator."
            );

            enabled = false;
        }
    }

    void Update()
    {
        Vector2 input = Vector2.zero;

        if (Keyboard.current != null)
        {
            input.x = Keyboard.current.aKey.isPressed ? -1 :
                      Keyboard.current.dKey.isPressed ? 1 : 0;

            input.y = Keyboard.current.sKey.isPressed ? -1 :
                      Keyboard.current.wKey.isPressed ? 1 : 0;
        }

        if (input.sqrMagnitude < 0.001f)
        {
            moveDirection = Vector3.zero;

            animator.SetBool("IsWalking", false);
            animator.SetFloat("MoveX", 0f);
            animator.SetFloat("MoveY", 0f);

            return;
        }

        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;

        if (cameraForward.sqrMagnitude > 0.001f)
            cameraForward.Normalize();

        if (cameraRight.sqrMagnitude > 0.001f)
            cameraRight.Normalize();

        moveDirection =
            cameraForward * input.y +
            cameraRight * input.x;

        if (moveDirection.sqrMagnitude > 0.001f)
            moveDirection.Normalize();

        if (moveDirection.sqrMagnitude > 0.001f)
            lastFacingDirection = moveDirection;

        animator.SetBool("IsWalking", true);
        animator.SetFloat("MoveX", input.x);
        animator.SetFloat("MoveY", input.y);
    }

    void FixedUpdate()
    {
        if (moveDirection.sqrMagnitude < 0.001f)
            return;

        Vector3 movement =
            moveDirection * speed * Time.fixedDeltaTime;

        if (rb.SweepTest(
            movement.normalized,
            out RaycastHit hit,
            movement.magnitude,
            QueryTriggerInteraction.Ignore))
        {
            Vector3 slideDirection =
                Vector3.ProjectOnPlane(movement, hit.normal);

            rb.MovePosition(rb.position + slideDirection);
            return;
        }

        rb.MovePosition(rb.position + movement);
    }
}