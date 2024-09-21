using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region Variables
    public enum PlayerState { WAIT, NORMAL, IRONMAN, DEAD }
    public PlayerState currentPlayerState;

    [Header("Health Variables")]
    public int maxHealth;
    public int currentHealth;

    [Header("Movement Variables")]
    public float mouseSensitivity;
    public float currentMoveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float jumpForce;
    public float fallMultiplier;

    private float verticalRotation = 0f;
    [HideInInspector] public bool isGrounded;
    private Vector3 velocity;

    [Header("Launching Variables")]
    public float launchForce;
    public float minimumLaunchAmount;
    public float floatStrength;
    public float launchMeter;
    public float currentLaunchMeter;

    private bool canLaunch;
    private bool isFloating;
    private float launchCooldown;

    [Header("Right Weapon Variables")]
    public GameObject rightProjectilePrefab;
    public Transform rightFirePoint;
    public int rightProjectileDamage;
    public float rightShootForce;
    public float rightFireRate;
    public int maxAmmo;
    public int currentAmmo;
    public int maxHealCharge;
    public int currentHealCharge;

    private float rightNextFireTime = 0f;
    private bool isReloading;
    private bool isHealing;

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

    [Header("World/Checkpoint Variables")]
    public int fallDamage;
    public Vector3 initialPosition;

    [Header("References")]
    public Animator RightWeaponAnimator;
    public Animator LeftWeaponAnimator;
    private GameManager GameManager;
    private CharacterController Controller;
    private Camera PlayerCamera;
    #endregion

    private void Start()
    {
        Initialize();
    }

    private void Update()
    {
        CheckPlayerState();
    }

    #region State Control
    private void Initialize()
    {
        Cursor.lockState = CursorLockMode.Locked;

        GameManager = FindFirstObjectByType<GameManager>();
        Controller = GetComponent<CharacterController>();
        PlayerCamera = GetComponentInChildren<Camera>();

        ChangePlayerState(PlayerState.NORMAL);

        initialPosition = transform.position;

        currentHealth = maxHealth;
        currentLaunchMeter = launchMeter;
        currentAmmo = maxAmmo;
    }

    private void CheckPlayerState()
    {
        if (GameManager.currentGameState == GameManager.GameState.PAUSE) return;

        switch (currentPlayerState)
        {
            case PlayerState.WAIT:
                break;
            case PlayerState.NORMAL:
                Movement();
                LaunchEnabled();
                LaunchingCooldown();
                NormalShooting();
                RightWeaponReload();
                RightWeaponHealing();
                break;
            case PlayerState.IRONMAN:
                Movement();
                LevitateEnabled();
                LaunchingCooldown();
                IronmanShooting();
                RightWeaponReload();
                RightWeaponHealing();
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
            if (velocity.y > 0)
            {
                velocity.y += Physics.gravity.y * currentGravity * Time.deltaTime * 1.5f;
            }
            else
            {
                velocity.y += Physics.gravity.y * currentGravity * Time.deltaTime;
            }
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
    private void LaunchEnabled()
    {
        if (currentLaunchMeter >= minimumLaunchAmount) canLaunch = true;

        if (Input.GetKeyDown(KeyCode.V) && canLaunch && isGrounded)
        {
            Launching();
        }
    }

    private void LevitateEnabled()
    {
        if (Input.GetKey(KeyCode.V) && !isGrounded)
        {
            Levitating();
        }
        else
        {
            StopLevitating();
        }
    }

    private void Launching()
    {
        ChangePlayerState(PlayerState.IRONMAN);


        LeftWeaponAnimator.SetTrigger("Flying");

        velocity.y = Mathf.Sqrt(launchForce * -2f * Physics.gravity.y);
        currentLaunchMeter -= minimumLaunchAmount;
        canLaunch = false;
        launchCooldown = 1f;
    }

    private void Levitating()
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
            StopLevitating();
        }
    }

    private void StopLevitating()
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
        if (isReloading || isHealing) return;

        if (Time.time >= rightNextFireTime && currentAmmo > 0)
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
        if (isReloading || isHealing) return;

        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo)
        {
            StartCoroutine(Reload());
        }
    }

    private IEnumerator Reload()
    {
        Debug.Log("Reloading");
        isReloading = true;
        yield return new WaitForSeconds(1f);
        //RightWeaponAnimator.SetTrigger("Reload");
        currentAmmo = maxAmmo;
        yield return new WaitForSeconds(0.5f);
        isReloading = false;
    }

    private void RightWeaponHealing()
    {
        if (isReloading || isHealing) return;
        
        if (Input.GetKeyDown(KeyCode.E) && currentHealCharge > 0)
        {
            StartCoroutine(Heal());
        }
    }

    private IEnumerator Heal()
    {
        Debug.Log("Healing");
        isHealing = true;
        //RightWeaponAnimator.SetTrigger("Healing");
        yield return new WaitForSeconds(0.5f);
        currentHealth += currentHealCharge;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        currentHealCharge = 0;
        yield return new WaitForSeconds(1f);
        isHealing = false;
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
            if (rb != null) rb.AddExplosionForce(grenadeExplosionForce, explosionPos, grenadeExplosionRadius, 1f);
        }
    }

    private Vector3 GetAimPoint()
    {
        return PlayerCamera.transform.position + PlayerCamera.transform.forward * aimDistance;
    }
    #endregion

    #region Taking Damage
    public void TakeDamage(int damageTaken)
    {
        if (currentPlayerState == PlayerState.DEAD) return;

        currentHealth -= damageTaken;
        //Trigger damage effects or animations here

        if (currentHealth <= 0)
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
        //Death animation
        //Wait 1/2 sec
        //Game over screen
    }
    #endregion

    #region Collisions
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Respawn"))
        {
            //Show blurry ui
            transform.position = initialPosition;
            TakeDamage(fallDamage);
        }
    }
    #endregion
}

