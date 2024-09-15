using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region Variables

    public enum PlayerState { WAIT, TUTORIAL, NORMAL, IRONMAN }
    public PlayerState currentState;

    [Header("Movement Variables")]
    public float mouseSensitivity;
    public float currentMoveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float jumpForce;
    public float fallMultiplier;

    private float verticalRotation = 0f;
    private bool isGrounded;
    private Vector3 velocity;

    [Header("Launching Variables")]
    public float launchForce;
    public float launchMeter;
    public float minimumLaunchAmount;
    public float floatStrength;

    private bool canLaunch;
    [SerializeField] private float currentLaunchMeter;
    private bool isFloating;
    private float launchCooldown;

    [Header("Right Weapon Variables")]

    public GameObject rightProjectilePrefab;
    public Transform rightFirePoint;
    public float rightShootForce;
    public float rightFireRate;
    public int maxAmmo;

    private float rightNextFireTime = 0f;
    [SerializeField] private int currentAmmo;
    private bool isReloading;

    public Animator RightWeaponAnimator;

    [Header("Left Weapon Variables")]
    public GameObject leftProjectilePrefab;
    public Transform leftFirePoint;
    public float leftShootForce;
    public float leftFireRate;

    public float grenadeExplosionRadius;
    public float grenadeExplosionForce;

    private float leftNextFireTime = 0f;

    //public Animator LeftWeaponAnimator;

    [Header("Shared Weapon Variables")]
    public float aimDistance;
    public LayerMask aimCollisionMask = Physics.DefaultRaycastLayers;

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

        currentLaunchMeter = launchMeter;
        currentAmmo = maxAmmo;
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
                Launching();
                LaunchingCooldown();
                NormalShooting();
                RightWeaponReload();
                break;
            case PlayerState.IRONMAN:
                Movement();
                Levitating();
                LaunchingCooldown();
                IronmanShooting();
                RightWeaponReload();
                break;
        }
    }

    private void ChangePlayerState(PlayerState newState)
    {
        currentState = newState;
    }

    #region Movement
    private void Movement()
    {
        isGrounded = Controller.isGrounded;

        //Looking
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        verticalRotation = Mathf.Clamp(verticalRotation - mouseY, -90f, 90f);
        transform.Rotate(Vector3.up * mouseX);
        PlayerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);

        //Moving
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        currentMoveSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        Controller.Move(move * currentMoveSpeed * Time.deltaTime);

        //Jumping and Falling
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * Physics.gravity.y);
            isFloating = false;
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

        if (isGrounded && currentState == PlayerState.IRONMAN && launchCooldown <= 0 && !isFloating)
            ChangePlayerState(PlayerState.NORMAL);
    }
    #endregion

    #region Launching
    private void Launching()
    {
        if (currentLaunchMeter >= minimumLaunchAmount) canLaunch = true;

        if (Input.GetKeyDown(KeyCode.V) && canLaunch && isGrounded)
        {
            Fly();
        }
        else
        {
            isFloating = false;
        }
    }

    private void Levitating()
    {
        if (Input.GetKey(KeyCode.V) && !isGrounded)
        {
            Float();
        }
        else
        {
            isFloating = false;
        }
    }

    private void Fly()
    {
        ChangePlayerState(PlayerState.IRONMAN);

        velocity.y = Mathf.Sqrt(launchForce * -2f * Physics.gravity.y);
        currentLaunchMeter -= minimumLaunchAmount;
        canLaunch = false;
        isFloating = false;
        launchCooldown = 1f;
    }

    private void Float()
    {
        if (currentLaunchMeter > 0 && velocity.y < 0)
        {
            Debug.Log("A");
            currentLaunchMeter -= Time.deltaTime;
            isFloating = true;
            velocity.y = Mathf.Max(velocity.y, -floatStrength);
        }
        else
        {
            Debug.Log("B");
            isFloating = false;
        }
    }

    private void LaunchingCooldown()
    {
        if (currentLaunchMeter < launchMeter && isGrounded) currentLaunchMeter += Time.deltaTime;

        if (launchCooldown > 0) launchCooldown -= Time.deltaTime;
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
        if (Time.time >= rightNextFireTime && !isReloading && currentAmmo > 0)
        {
            Vector3 rightShotDirection = (aimPoint - rightFirePoint.position).normalized;

            GameObject rightProjectile = Instantiate(rightProjectilePrefab, rightFirePoint.position, rightFirePoint.rotation);
            rightProjectile.GetComponent<Rigidbody>().AddForce(rightShotDirection * rightShootForce, ForceMode.Impulse);
            RightWeaponAnimator.SetTrigger("Shoot");

            currentAmmo--;
            //Destroy(rightProjectile, 2f);
            rightNextFireTime = Time.time + 1f / rightFireRate;
        }
        else if (currentAmmo <= 0)
        {
            //RightWeaponAnimator.SetTrigger("Empty");
        }
    }

    private void RightWeaponReload()
    {
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmo < maxAmmo)
        {
            StartCoroutine(Reload());
        }
    }

    private IEnumerator Reload()
    {
        Debug.Log("Reloading");
        isReloading = true;
        yield return new WaitForSeconds(1.5f);
        //RightWeaponAnimator.SetTrigger("Reload");
        currentAmmo = maxAmmo;
        isReloading = false;
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

            LeftWeaponExplosion(leftProjectile);
        }
    }

    private void LeftWeaponExplosion(GameObject leftProjectile)
    {
        Vector3 explosionPos = leftProjectile.transform.position;
        Collider[] colliders = Physics.OverlapSphere(explosionPos, grenadeExplosionRadius);

        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();

            if (rb != null)
                rb.AddExplosionForce(grenadeExplosionForce, explosionPos, grenadeExplosionRadius, 1f);

            //Add damage logic for objects within the explosion radius
            //Example: hit.GetComponent<Damageable>()?.TakeDamage(calculatedDamage);
        }
    }

    private Vector3 GetAimPoint()
    {
        Ray ray = PlayerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); //Ray from center of the screen
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, aimDistance, aimCollisionMask))
        {
            return hit.point; //Return the point where the ray hits an object
        }
        else
        {
            //Return a point at maximum aim distance along the ray if nothing is hit
            return ray.GetPoint(aimDistance);
        }
    }
    #endregion
}

