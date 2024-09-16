using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region Variables
    public enum PlayerState { WAIT, TUTORIAL, NORMAL, IRONMAN, DEAD }
    public PlayerState currentPlayerState;

    [Header("Health Variables")]
    public float maxHealth;
    public float currentHealth;

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
    public int rightProjectileDamage;
    public float rightShootForce;
    public float rightFireRate;
    public int maxAmmo;

    private float rightNextFireTime = 0f;
    [SerializeField] private int currentAmmo;
    private bool isReloading;

    [Header("Left Weapon Variables")]
    public GameObject leftProjectilePrefab;
    public Transform leftFirePoint;
    public int leftProjectileDamage;
    public float leftShootForce;
    public float leftFireRate;

    public float grenadeExplosionRadius;
    public float grenadeExplosionForce;

    private float leftNextFireTime = 0f;

    [Header("Shared Weapon Variables")]
    public float aimDistance;

    [Header("References")]
    public Animator RightWeaponAnimator;
    public Animator LeftWeaponAnimator;
    private _GameManager GameManager;
    private CharacterController Controller;
    private Camera PlayerCamera;
    #endregion

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        GameManager = FindFirstObjectByType<_GameManager>();
        Controller = GetComponent<CharacterController>();
        PlayerCamera = GetComponentInChildren<Camera>();

        ChangePlayerState(PlayerState.NORMAL);

        currentHealth = maxHealth;
        currentLaunchMeter = launchMeter;
        currentAmmo = maxAmmo;
    }

    private void Update()
    {   
        CheckPlayerState();
    }

    #region State Control
    private void CheckPlayerState()
    {
        if (GameManager.currentGameState == _GameManager.GameState.PAUSE) return;

        switch (currentPlayerState)
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
            case PlayerState.DEAD:
                Dead();
                break;
        }
    }

    private void ChangePlayerState(PlayerState newState)
    {
        currentPlayerState = newState;
    }
    #endregion

    #region Tutorial

    #endregion

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

        if (isGrounded && currentPlayerState == PlayerState.IRONMAN && launchCooldown <= 0 && !isFloating)
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
    }

    private void Levitating()
    {
        if (Input.GetKey(KeyCode.V) && !isGrounded)
        {
            Float();
        }
        else
        {
            StopFloating();
        }
    }

    private void Fly()
    {
        ChangePlayerState(PlayerState.IRONMAN);
        LeftWeaponAnimator.SetTrigger("Flying");

        velocity.y = Mathf.Sqrt(launchForce * -2f * Physics.gravity.y);
        currentLaunchMeter -= minimumLaunchAmount;
        canLaunch = false;
        launchCooldown = 1f;
    }

    private void Float()
    {
        LeftWeaponAnimator.SetBool("Floating", true);
        if (currentLaunchMeter > 0 && velocity.y < 0)
        {

            currentLaunchMeter -= Time.deltaTime;
            isFloating = true;
            velocity.y = Mathf.Max(velocity.y, -floatStrength);
        }
        else
        {
            StopFloating();
        }
    }

    private void StopFloating()
    {
        isFloating = false;
        LeftWeaponAnimator.SetBool("Floating", false);
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
            RightWeaponShooting();
            LeftWeaponShooting();
        }
    }

    private void IronmanShooting()
    {
        if (Input.GetButton("Fire1"))
        {
            RightWeaponShooting();
        }
    }

    private void RightWeaponShooting()
    {
        if (Time.time >= rightNextFireTime && !isReloading && currentAmmo > 0)
        {
            Vector3 aimPoint = GetAimPoint();
            Vector3 rightShotDirection = (aimPoint - rightFirePoint.position).normalized;

            GameObject rightProjectile = Instantiate(rightProjectilePrefab, rightFirePoint.position, rightFirePoint.rotation);
            rightProjectile.GetComponent<Rigidbody>().AddForce(rightShotDirection * rightShootForce, ForceMode.Impulse);
            RightWeaponAnimator.SetTrigger("Shoot");

            currentAmmo--;
            rightNextFireTime = Time.time + rightFireRate;
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

    private void LeftWeaponShooting()
    {
        if (Time.time >= leftNextFireTime)
        {
            Vector3 aimPoint = GetAimPoint();
            Vector3 leftShotDirection = (aimPoint - leftFirePoint.position).normalized;

            GameObject leftProjectile = Instantiate(leftProjectilePrefab, leftFirePoint.position, leftFirePoint.rotation);
            leftProjectile.GetComponent<Rigidbody>().AddForce(leftShotDirection * leftShootForce, ForceMode.Impulse);
            LeftWeaponAnimator.SetTrigger("Shoot");

            leftNextFireTime = Time.time + leftFireRate;

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
        return PlayerCamera.transform.position + PlayerCamera.transform.forward * aimDistance;
    }
    #endregion

    #region Taking Damage
    public void TakeDamage(float damageTaken)
    {
        if (currentPlayerState == PlayerState.DEAD) return;

        currentHealth -= damageTaken;
        //Trigger damage effects or animations here

        if (currentHealth < 0)
        {
            ChangePlayerState(PlayerState.DEAD);
        }
        else if (currentHealth < maxHealth * 0.3f)
        {
            //Play low health warning effects
        }
    }
    #endregion

    #region Death
    private void Dead()
    {
        //Death animation, game over screen
    }
    #endregion
}

