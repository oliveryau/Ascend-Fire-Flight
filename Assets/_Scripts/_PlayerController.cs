using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    #region Variables

    public enum PlayerState { WAIT, TUTORIAL, NORMAL, IRONMAN }
    public PlayerState currentState;

    [Header("Movement")]
    public float mouseSensitivity;
    public float currentMoveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float jumpForce;
    public float fallMultiplier;

    private float verticalRotation = 0f;
    private bool isGrounded;
    private Vector3 velocity;

    [Header("Launching")]
    public float launchForce;

    [Header("Right Weapon")]
    public GameObject rightProjectilePrefab;
    public Transform rightFirePoint;
    public float rightShootForce;
    public float rightFireRate;
    private float rightNextFireTime = 0f;

    [Header("Left Weapon")]
    public GameObject leftProjectilePrefab;
    public Transform leftFirePoint;
    public float leftShootForce;
    public float leftFireRate;
    private float leftNextFireTime = 0f;

    public float aimDistance;

    [Header("References")]
    private CharacterController Controller;
    private Camera PlayerCamera;

    #endregion

    private void Start()
    {
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;

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
                NormalShooting();
                break;
        }
    }

    #region Movement
    private void Movement()
    {
        isGrounded = Controller.isGrounded;

        #region Looking
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        verticalRotation = Mathf.Clamp(verticalRotation - mouseY, -90f, 90f);
        transform.Rotate(Vector3.up * mouseX);
        PlayerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        #endregion

        #region Moving
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        currentMoveSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed; //Sprint

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        Controller.Move(move * currentMoveSpeed * Time.deltaTime);
        #endregion

        #region Jumping
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * Physics.gravity.y);
        }

        if (!isGrounded)
        {
            float currentGravity = velocity.y <= 0 ? fallMultiplier : 1f;
            velocity.y += Physics.gravity.y * currentGravity * Time.deltaTime;
        }
        else if (velocity.y < 0)
        {
            velocity.y = -2f;
        }

        Controller.Move(velocity * Time.deltaTime);
        #endregion
    }
    #endregion

    #region Shooting
    private void NormalShooting()
    {
        if (Input.GetButton("Fire1"))
        {
            Vector3 aimPoint = GetAimPoint();
            RightWeaponShooting(aimPoint);
            LeftWeaponShooting(aimPoint);
        }
    }

    private void IronmanShooting()
    {
        if (Input.GetButton("Fire1"))
        {
            Vector3 aimPoint = GetAimPoint();
            RightWeaponShooting(aimPoint);
        }
    }

    private void RightWeaponShooting(Vector3 aimPoint)
    {
        if (Time.time >= rightNextFireTime)
        {
            Vector3 rightShotDirection = (aimPoint - rightFirePoint.position).normalized;

            GameObject rightProjectile = Instantiate(rightProjectilePrefab, rightFirePoint.position, rightFirePoint.rotation);
            rightProjectile.GetComponent<Rigidbody>().AddForce(rightShotDirection * rightShootForce, ForceMode.Impulse);

            Destroy(rightProjectile, 2f);
            rightNextFireTime = Time.time + 1f / rightFireRate;
        }
    }

    private void LeftWeaponShooting(Vector3 aimPoint)
    {
        if (Time.time >= leftNextFireTime)
        {
            Vector3 leftShotDirection = (aimPoint - leftFirePoint.position).normalized;

            GameObject leftProjectile = Instantiate(leftProjectilePrefab, leftFirePoint.position, leftFirePoint.rotation);
            leftProjectile.GetComponent<Rigidbody>().AddForce(leftShotDirection * leftShootForce, ForceMode.Impulse);

            Destroy(leftProjectile, 2f);
            leftNextFireTime = Time.time + 1f / leftFireRate;
        }
    }

    private Vector3 GetAimPoint()
    {
        return PlayerCamera.transform.position + PlayerCamera.transform.forward * aimDistance;
    }
    #endregion
}

