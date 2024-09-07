using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region Variables

    public enum PlayerState
    {
        WAIT, TUTORIAL, NORMAL, IRONMAN
    }

    public PlayerState currentState;

    [Header("Movement")]
    public float mouseSensitivity;
    public float moveSpeed;
    public float jumpForce;
    public float gravity;
    public float fallMultiplier;

    private float verticalRotation = 0f;
    private Vector3 velocity;
    private bool isGrounded;

    [Header("Launching")]
    public float launchForce;

    [Header("Shooting")]
    public GameObject rightProjectilePrefab;
    public Transform rightFirePoint;
    public float rightShootForce;
    public float rightFireRate;

    public GameObject leftProjectilePrefab;
    public Transform leftFirePoint;
    public float leftShootForce;
    public float leftFireRate;

    public float maxShotDistance;
    private float rightNextFireTime = 0f;
    private float leftNextFireTime = 0f;

    [Header("References")]
    private CharacterController Controller;
    private Camera PlayerCamera;

    #endregion

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        Controller = GetComponent<CharacterController>();
        PlayerCamera = GetComponentInChildren<Camera>();

        currentState = PlayerState.NORMAL;
    }

    private void Update()
    {   
        CheckPlayerState();
    }

    private void CheckPlayerState()
    {
        switch (currentState)
        {
            case PlayerState.WAIT:
                break;
            case PlayerState.TUTORIAL:
                //Tutorial();
                break;
            case PlayerState.NORMAL:
                Movement();
                //Launch();
                Shoot();
                break;
        }
    }

    private void Movement()
    {
        //Ground check
        isGrounded = Controller.isGrounded;

        //Look
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);

        transform.Rotate(Vector3.up * mouseX);
        PlayerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);

        //Move
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        Controller.Move(move * moveSpeed * Time.deltaTime);

        //Jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }

        if (!isGrounded)
        {
            float currentGravity = velocity.y <= 0 ? fallMultiplier : 1f;
            velocity.y += gravity * currentGravity * Time.deltaTime;
        }
        else if (velocity.y < 0)
        {
            velocity.y = -2f;
        }

        Controller.Move(velocity * Time.deltaTime);
    }

    private void Shoot()
    {
        if (Input.GetButton("Fire1"))
        {
            Vector3 aimPoint = GetAimPoint();

            if (Time.time >= rightNextFireTime)
            {
                Vector3 rightShootDirection = (aimPoint - rightFirePoint.position).normalized;

                GameObject rightProjectile = Instantiate(rightProjectilePrefab, rightFirePoint.position, rightFirePoint.rotation);
                Rigidbody rightProjectileRb = rightProjectile.GetComponent<Rigidbody>();
                rightProjectileRb.AddForce(rightFirePoint.forward * rightShootForce, ForceMode.Impulse);

                Destroy(rightProjectile, 3f);

                rightNextFireTime = Time.time + 1f / rightFireRate;
            }

            if (Time.time >= leftNextFireTime)
            {
                Vector3 leftShootDirection = (aimPoint - leftFirePoint.position).normalized;

                GameObject leftProjectile = Instantiate(leftProjectilePrefab, leftFirePoint.position, leftFirePoint.rotation);
                Rigidbody leftProjectileRb = leftProjectile.GetComponent<Rigidbody>();
                leftProjectileRb.AddForce(leftFirePoint.forward * leftShootForce, ForceMode.Impulse);

                Destroy(leftProjectile, 3f);

                leftNextFireTime = Time.time + 1f / leftFireRate;
            }
        }
    }

    private Vector3 GetAimPoint()
    {
        Ray ray = PlayerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); //Center of the screen
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxShotDistance))
        {
            return hit.point;
        }
        else
        {
            // If we didn't hit anything, use a point maxShootDistance units away from the camera
            return ray.GetPoint(maxShotDistance);
        }
    }
}

