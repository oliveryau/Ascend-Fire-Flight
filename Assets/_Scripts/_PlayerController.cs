using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region Variables

    public enum PlayerState
    {
        WAIT, TUTORIAL, IRONMAN
    }

    public PlayerState state;

    public float mouseSensitivity;
    public float moveSpeed;
    public float jumpForce;
    public float gravity;
    public float fallMultiplier;

    private CharacterController Controller;
    private Camera PlayerCamera;

    private float verticalRotation = 0f;
    private Vector3 velocity;
    private bool isGrounded;

    #endregion

    private void Start()
    {
        Controller = GetComponent<CharacterController>();
        PlayerCamera = GetComponentInChildren<Camera>();
        Cursor.lockState = CursorLockMode.Locked;

        state = PlayerState.IRONMAN;
    }

    private void Update()
    {   
        CheckPlayerState();
    }

    private void CheckPlayerState()
    {
        switch (state)
        {
            case PlayerState.WAIT:
                break;
            case PlayerState.TUTORIAL:
                //Tutorial();
                break;
            case PlayerState.IRONMAN:
                Movement();
                //Launch();
                //Shoot();
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
}

