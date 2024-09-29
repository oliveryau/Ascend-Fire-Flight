using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region Variables
    public enum PlayerState { WAIT, NORMAL, IRONMAN, DEAD }
    public PlayerState currentPlayerState;

    [Header("Health Variables")]
    public float maxHealth;
    public float currentHealth;

    private bool hasTakenDamaged;

    [Header("Movement Variables")]
    public float mouseSensitivity;
    public float currentMoveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float jumpForce;
    public float fallMultiplier;
    public float footstepWalkInterval;
    public float footstepSprintInterval;

    private float verticalRotation = 0f;
    [HideInInspector] public bool isGrounded;
    private Vector3 velocity;
    private Coroutine footstepCoroutine;
    private bool isWalking;
    private bool isSprinting;
    private bool isInAir;

    [Header("Launching Variables")]
    public ParticleSystem launchParticle;
    public float launchForce;
    public float minimumLaunchAmount;
    public float floatStrength;
    public float launchMeter;
    public float currentLaunchMeter;

    private bool canLaunch;
    private bool isFloating;
    private float targetLaunchMeter;
    private float initialLaunchMeter;
    private float launchCooldown;
    private bool floatSoundPlaying;

    [Header("Landing Variables")]
    public LayerMask enemyLayer;
    public float landingCheckDistance;
    public float landingHorizontalAdjustment;

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
    private UiManager UiManager;
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
        UiManager = FindFirstObjectByType<UiManager>();
        Controller = GetComponent<CharacterController>();
        PlayerCamera = GetComponentInChildren<Camera>();

        ChangePlayerState(PlayerState.NORMAL);

        initialPosition = transform.position;

        currentHealth = maxHealth;
        currentLaunchMeter = launchMeter;
        initialLaunchMeter = currentLaunchMeter;
        targetLaunchMeter = currentLaunchMeter;
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
                UpdateLaunchMeter();
                CheckLanding();
                NormalShooting();
                RightWeaponReload();
                RightWeaponHealing();
                break;
            case PlayerState.IRONMAN:
                Movement();
                LevitateEnabled();
                LaunchingCooldown();
                UpdateLaunchMeter();
                CheckLanding();
                IronmanShooting();
                RightWeaponReload();
                RightWeaponHealing();
                break;
            case PlayerState.DEAD:
                CheckLanding();
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

        if (currentPlayerState != PlayerState.IRONMAN) currentMoveSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;
        else currentMoveSpeed = walkSpeed;

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        Controller.Move(move * currentMoveSpeed * Time.deltaTime);

        if (isGrounded && (moveX != 0 || moveZ != 0))
        {
            if (!isWalking)
            {
                isWalking = true;
                isSprinting = Input.GetKey(KeyCode.LeftShift);
                footstepCoroutine = StartCoroutine(PlayFootstepSounds());
            }
        }
        else
        {
            if (isWalking)
            {
                isWalking = false;
                isSprinting = false;
                if (footstepCoroutine != null)
                {
                    StopCoroutine(footstepCoroutine);
                }
            }
        }

        //Jumping and Falling
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * Physics.gravity.y);
            isFloating = false;
            isInAir = true;
            AudioManager.Instance.PlayOneShot("Jump", gameObject);
        }

        if (!isGrounded)
        {
            float currentGravity = velocity.y <= 0 ? fallMultiplier : 1f;
            isInAir = true;
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

        if (isGrounded && isInAir)
        {
            RandomiseLandingAudio();
            isInAir = false;
        }

        if (isGrounded && currentPlayerState == PlayerState.IRONMAN && launchCooldown <= 0 && !isFloating)
            ChangePlayerState(PlayerState.NORMAL);
    }

    private IEnumerator PlayFootstepSounds()
    {
        while (isWalking)
        {
            float interval = isSprinting ? footstepSprintInterval : footstepWalkInterval;
            float adjustedInterval = interval / (currentMoveSpeed / walkSpeed);

            RandomiseFootstepAudio();
            yield return new WaitForSeconds(adjustedInterval);

            isSprinting = Input.GetKey(KeyCode.LeftShift);
        }
    }

    private void RandomiseFootstepAudio()
    {
        int soundIndex = Random.Range(1, 5);
        string soundName = $"Footstep {soundIndex}";
        AudioManager.Instance.PlayOneShot(soundName, gameObject);
    }

    private void RandomiseLandingAudio()
    {
        int soundIndex = Random.Range(1, 3);
        string soundName = $"Land {soundIndex}"; 
        AudioManager.Instance.PlayOneShot(soundName, gameObject);
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
        AudioManager.Instance.PlayOneShot("Launch", LeftWeaponAnimator.gameObject);

        velocity.y = Mathf.Sqrt(launchForce * -2f * Physics.gravity.y);
        targetLaunchMeter = currentLaunchMeter - minimumLaunchAmount;
        canLaunch = false;
        launchCooldown = 1f;
    }

    private void Levitating()
    {
        LeftWeaponAnimator.SetBool("Floating", true);
        if (!floatSoundPlaying)
        {
            floatSoundPlaying = true;
            AudioManager.Instance.Play("Float", LeftWeaponAnimator.gameObject);
        }

        if (currentLaunchMeter > 0 && velocity.y < 0)
        {
            targetLaunchMeter = currentLaunchMeter -= Time.deltaTime;
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
        floatSoundPlaying = false;
        LeftWeaponAnimator.SetBool("Floating", false);
        AudioManager.Instance.Stop("Float", LeftWeaponAnimator.gameObject);
    }

    private void LaunchingCooldown()
    {
        if (targetLaunchMeter < launchMeter && isGrounded) targetLaunchMeter += Time.deltaTime;

        if (launchCooldown > 0) launchCooldown -= Time.deltaTime;
    }

    private void UpdateLaunchMeter()
    {
        float previousLaunchMeter = currentLaunchMeter;
        currentLaunchMeter = Mathf.Lerp(currentLaunchMeter, targetLaunchMeter, Time.deltaTime * 5f);

        if (currentLaunchMeter < previousLaunchMeter)
        {
            UpdateLaunchParticle(true);
        }
        else if (currentLaunchMeter > previousLaunchMeter && currentLaunchMeter < launchMeter)
        {
            UpdateLaunchParticle(false);
        }
    }

    private void UpdateLaunchParticle(bool isDecreasing)
    {
        if (isDecreasing)
        {
            if (!launchParticle.isPlaying) launchParticle.Play();
        }
        else
        {
            if (launchParticle.isPlaying) launchParticle.Stop();
        }
    }

    private void CheckLanding()
    {
        if (!isGrounded && velocity.y < 0)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, landingCheckDistance, enemyLayer))
            {
                Vector3 adjustmentDirection = Random.insideUnitCircle.normalized; //Enemy detected below, adjust landing position
                Vector3 newPosition = transform.position + new Vector3(adjustmentDirection.x, 0, adjustmentDirection.y) * landingHorizontalAdjustment;

                if (!Physics.Raycast(newPosition, Vector3.down, landingCheckDistance, enemyLayer)) //Check if the new position is clear
                {
                    transform.position = newPosition;
                }
            }
        }
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
            AudioManager.Instance.PlayOneShot("Right Gunshot", RightWeaponAnimator.gameObject);

            currentAmmo--;
            UiManager.UpdatePlayerAmmoCount();
            rightNextFireTime = Time.time + rightFireRate;
        }
        else if (currentAmmo <= 0)
        {
            RightWeaponAnimator.SetTrigger("Empty");
            //Play error sound
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
        isReloading = true;
        RightWeaponAnimator.SetTrigger("Reload");
        AudioManager.Instance.PlayOneShot("Reload", RightWeaponAnimator.gameObject);
        yield return new WaitForSeconds(0.5f);
        currentAmmo = maxAmmo;
        UiManager.UpdatePlayerAmmoCount();
        yield return new WaitForSeconds(0.5f);
        isReloading = false;
    }

    private void RightWeaponHealing()
    {
        if (isReloading || isHealing) return;
        if (currentHealth >= maxHealth) return;
        
        if (Input.GetKeyDown(KeyCode.E) && currentHealCharge > 0)
        {
            StartCoroutine(Heal());
        }
    }

    private IEnumerator Heal()
    {
        isHealing = true;
        RightWeaponAnimator.SetTrigger("Healing");
        AudioManager.Instance.PlayOneShot("Healing", RightWeaponAnimator.gameObject);
        yield return new WaitForSeconds(1f);
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
            AudioManager.Instance.PlayOneShot("Left Gunshot", LeftWeaponAnimator.gameObject);

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
        //Ray ray = PlayerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Ray ray = PlayerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(PlayerCamera.transform.position, PlayerCamera.transform.forward, out hit, aimDistance))
        {
            return hit.point;
        }
        else
        {
            return ray.GetPoint(aimDistance);
        }
    }
    #endregion

    #region Taking Damage
    public void TakeDamage(int damageTaken)
    {
        if (currentPlayerState == PlayerState.DEAD) return;
        if (hasTakenDamaged) return;

        StartCoroutine(Iframe());
        currentHealth -= damageTaken;
        RandomiseHurtAudio();
        UiManager.DisplayDamagedOverlay();
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

    private IEnumerator FallingOut()
    {
        StartCoroutine(UiManager.DisplayFallingOutOverlay());
        AudioManager.Instance.PlayOneShot("Falling", gameObject);
        yield return new WaitForSeconds(0.5f);
        Controller.enabled = false;
        yield return new WaitForSeconds(0.5f);
        TakeDamage(fallDamage);
        transform.position = initialPosition;
        Controller.enabled = true;
    }

    private IEnumerator Iframe()
    {
        hasTakenDamaged = true;
        yield return new WaitForSeconds(0.5f);
        hasTakenDamaged = false;
    }

    private void RandomiseHurtAudio()
    {
        int soundIndex = Random.Range(1, 3);
        string soundName = $"Hurt {soundIndex}";
        AudioManager.Instance.PlayOneShot(soundName, gameObject);
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
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.layer == LayerMask.NameToLayer("Enemy") && !isGrounded)
        {
            Vector3 slideDirection = Vector3.ProjectOnPlane(velocity, hit.normal).normalized; //Slide off the enemy
            Controller.Move(slideDirection * Time.deltaTime * currentMoveSpeed);
        }
    }

    private void OnTriggerEnter(Collider target)
    {
        if (target.CompareTag("Respawn"))
        {
            StartCoroutine(FallingOut());
        }
    }
    #endregion
}

