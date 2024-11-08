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

    public bool canHeal;
    public bool isHealing;
    private bool hasTakenDamaged;
    private bool isDead;

    [Header("Movement Variables")]
    public float mouseSensitivity;
    public float currentMoveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float floatSpeed;
    public float jumpForce;
    public float fallMultiplier;
    public GameObject sprintLines;

    private float verticalRotation = 0f;
    private bool isGrounded;
    private Vector3 velocity;
    private Vector3 currentVelocity;
    private bool isMoving;
    private bool isWalking;
    private bool isSprinting;
    private bool isInAir;
    private bool walkSoundPlaying;
    private bool sprintSoundPlaying;

    [Header("Launching Variables")]
    public ParticleSystem[] launchParticles;
    public float launchForce;
    public float minimumLaunchAmount;
    public float floatStrength;
    public float maxLaunchMeter;
    public float currentLaunchMeter;
    public GameObject launchFloorParticle;

    private bool canLaunch;
    private bool isFloating;
    private float targetLaunchMeter;
    private float initialLaunchMeter;
    private float launchCooldown;
    private bool floatSoundPlaying;

    [Header("Right Weapon Variables")]
    public GameObject rightProjectilePrefab;
    public Transform rightFirePoint;
    public float rightProjectileDamage;
    public float rightShootForce;
    public float rightFireRate;
    public int maxAmmo;
    public int currentAmmo;

    private float rightNextFireTime = 0f;
    private bool isReloading;
    private float errorSoundCooldown = 0f;
    private const float ERROR_SOUND_INTERVAL = 0.5f;

    [Header("Left Weapon Variables")]
    public GameObject leftProjectilePrefab;
    public Transform leftFirePoint;
    public ParticleSystem leftFlameParticle;
    public float leftProjectileDamage;
    public float leftShootForce;
    public float leftLobForce;
    public float leftFireRate;

    public float grenadeExplosionRadius;
    //public float grenadeExplosionForce;

    private float leftNextFireTime = 0f;

    [Header("Shared Weapon Variables")]
    public float aimDistance;

    [Header("Other Variables")]
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

    #region Initialization
    private void Start()
    {
        Initialize();
    }

    private void Update()
    {
        CheckPlayerState();
    }

    private void Initialize()
    {
        GameManager = FindFirstObjectByType<GameManager>();
        UiManager = FindFirstObjectByType<UiManager>();
        Controller = GetComponent<CharacterController>();
        PlayerCamera = GetComponentInChildren<Camera>();

        ChangePlayerState(PlayerState.NORMAL);

        initialPosition = transform.position;

        currentHealth = maxHealth;
        currentVelocity = Vector3.zero;
        currentLaunchMeter = maxLaunchMeter;
        initialLaunchMeter = currentLaunchMeter;
        targetLaunchMeter = currentLaunchMeter;
        currentAmmo = maxAmmo;
    }
    #endregion

    #region State Control
    private void CheckPlayerState()
    {
        if (GameManager.currentGameState != GameManager.GameState.PLAY) return;

        switch (currentPlayerState)
        {
            case PlayerState.WAIT:
                break;
            case PlayerState.NORMAL:
                CheckHealthBar();
                Movement();
                LaunchEnabled();
                LaunchingCooldown();
                UpdateLaunchMeter();
                CheckLaunchMeter();
                NormalShooting();
                RightWeaponReload();
                HealingEnabled();
                ToggleLeftKeys(false);
                break;
            case PlayerState.IRONMAN:
                CheckHealthBar();
                Movement();
                LevitateEnabled();
                LaunchingCooldown();
                UpdateLaunchMeter();
                CheckLaunchMeter();
                IronmanShooting();
                RightWeaponReload();
                HealingEnabled();
                ToggleLeftKeys(true);
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

    private void ToggleLeftKeys(bool activeOverlay)
    {
        UiManager.OverlayLeftUi(activeOverlay);
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
        isMoving = (moveX != 0 || moveZ != 0);

        if (currentPlayerState != PlayerState.IRONMAN)
        {
            isSprinting = Input.GetKey(KeyCode.LeftShift);
            currentMoveSpeed = isSprinting ? sprintSpeed : walkSpeed;
            sprintLines.SetActive(isSprinting && isMoving && isGrounded);

            if (isSprinting && isMoving && isGrounded)
            {
                UiManager.sprintUi.GetComponent<Animator>().SetBool("Sprinting", true);
                if (!sprintSoundPlaying)
                {
                    sprintSoundPlaying = true;
                    AudioManager.Instance.Play("Sprinting", gameObject);
                }
            }
            else
            {
                UiManager.sprintUi.GetComponent<Animator>().SetBool("Sprinting", false);

                sprintSoundPlaying = false;
                AudioManager.Instance.Stop("Sprinting", gameObject);
            }

            if (isGrounded && isMoving)
            {
                if (!isWalking)
                {
                    isWalking = true;

                    if (!walkSoundPlaying)
                    {
                        walkSoundPlaying = true;
                        AudioManager.Instance.Play("Walking", gameObject);
                    }
                }
            }
            else
            {
                if (isWalking)
                {
                    isWalking = false;
                    isSprinting = false;
                    walkSoundPlaying = false;
                    AudioManager.Instance.Stop("Walking", gameObject);
                }
            }
        }
        else
        {
            currentMoveSpeed = floatSpeed;
            isSprinting = false;
            sprintLines.SetActive(false);
            UiManager.sprintUi.GetComponent<Animator>().SetBool("Sprinting", false);

            walkSoundPlaying = false;
            AudioManager.Instance.Stop("Walking", gameObject);
            sprintSoundPlaying = false;
            AudioManager.Instance.Stop("Sprinting", gameObject);
        }

        if (!isMoving)
        {
            currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, 0.05f);
            Controller.Move(currentVelocity * Time.deltaTime);
        }
        else
        {
            Vector3 move = transform.right * moveX + transform.forward * moveZ;
            Controller.Move(move * currentMoveSpeed * Time.deltaTime);
            currentVelocity = move * currentMoveSpeed;
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
        {
            if (!leftFlameParticle.isPlaying) leftFlameParticle.Play();
            ChangePlayerState(PlayerState.NORMAL);
        }
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
        if (currentPlayerState != PlayerState.NORMAL) return;

        if (launchCooldown <= 0)
        {
            canLaunch = currentLaunchMeter >= minimumLaunchAmount;
        }
        else
        {
            canLaunch = false;
        }

        if (Input.GetKeyDown(KeyCode.V) && canLaunch)
        {
            Launching();
        }
    }

    private void LevitateEnabled()
    {
        if (currentPlayerState != PlayerState.IRONMAN) return;

        if (Input.GetKey(KeyCode.V) && !isGrounded)
        {
            Levitating();
        }
        else if (Input.GetKeyUp(KeyCode.V) || isGrounded)
        {
            StopLevitating();
        }
    }

    private void Launching()
    {
        ChangePlayerState(PlayerState.IRONMAN);

        Vector3 launchFloorParticlePosition = transform.position;
        launchFloorParticlePosition.y = transform.position.y;
        launchFloorParticlePosition += transform.forward * 3f;
        Instantiate(launchFloorParticle, launchFloorParticlePosition, Quaternion.identity);

        LeftWeaponAnimator.SetTrigger("Flying");
        UiManager.playerLaunchCue.GetComponent<Animator>().SetBool("Floating", true);
        StartCoroutine(UiManager.FlashLaunchOverlay());
        AudioManager.Instance.PlayOneShot("Launch", LeftWeaponAnimator.gameObject);

        velocity.y = Mathf.Sqrt(launchForce * -2f * Physics.gravity.y);
        targetLaunchMeter = currentLaunchMeter - minimumLaunchAmount;
        canLaunch = false;
        launchCooldown = 1f;
        isFloating = true;

        if (leftFlameParticle.isPlaying) leftFlameParticle.Stop();
        foreach (var launchParticle in launchParticles)
        {
            if (!launchParticle.isPlaying)
            {
                launchParticle.Play();
            }
        }
    }

    private void Levitating()
    {
        LeftWeaponAnimator.SetBool("Floating", true);
        UiManager.playerLaunchCue.GetComponent<Animator>().SetBool("Floating", true);
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

            if (leftFlameParticle.isPlaying) leftFlameParticle.Stop();
            foreach (var launchParticle in launchParticles)
        {
            if (!launchParticle.isPlaying)
            {
                launchParticle.Play();
            }
        }
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
        UiManager.playerLaunchCue.GetComponent<Animator>().SetBool("Floating", false);
        AudioManager.Instance.Stop("Float", LeftWeaponAnimator.gameObject);

        foreach (var launchParticle in launchParticles)
        {
            if (launchParticle.isPlaying)
            {
                launchParticle.Stop();
            }
        }
    }

    private void LaunchingCooldown()
    {
        if (launchCooldown > 0)
        {
            launchCooldown -= Time.deltaTime;
            canLaunch = false;
        }

        if (targetLaunchMeter < maxLaunchMeter && !Input.GetKey(KeyCode.V))
        {
            targetLaunchMeter += Time.deltaTime;
        }
    }

    private void UpdateLaunchMeter()
    {
        float previousLaunchMeter = currentLaunchMeter;
        currentLaunchMeter = Mathf.Lerp(currentLaunchMeter, targetLaunchMeter, Time.deltaTime * 5f);

        if (!isFloating)
        {
            if (currentLaunchMeter < previousLaunchMeter)
            {
                UpdateLaunchParticle(true);
            }
            else if (currentLaunchMeter > previousLaunchMeter && currentLaunchMeter < maxLaunchMeter)
            {
                UpdateLaunchParticle(false);
            }
        }
    }

    private void CheckLaunchMeter()
    {
        if (!UiManager.launchUi.activeSelf) return;

        if (currentLaunchMeter <= 0.4f * maxLaunchMeter) UiManager.FlashLaunchMeter(true);
        else UiManager.FlashLaunchMeter(false);
    }

    private void UpdateLaunchParticle(bool isDecreasing)
    {
        foreach (var launchParticle in launchParticles)
        {
            if (isDecreasing && !launchParticle.isPlaying)
            {
                launchParticle.Play();
            }
            else if (!isDecreasing && launchParticle.isPlaying)
            {
                launchParticle.Stop();
            }
        }
    }
    #endregion

    #region Shooting
    private void NormalShooting()
    {
        if (Input.GetButton("Fire1")) RightWeaponShooting();

        if (Input.GetButton("Fire2")) LeftWeaponShooting();

        UiManager.ToggleLeftCrosshair(false);
    }

    private void IronmanShooting()
    {
        if (Input.GetButton("Fire1")) RightWeaponShooting();

        UiManager.ToggleLeftCrosshair(true);
    }

    private void RightWeaponShooting()
    {
        if (isReloading || isHealing) return;

        if (Time.time >= rightNextFireTime && currentAmmo > 0)
        {
            Vector3 aimPoint = GetAimPoint();
            Vector3 rightShotDirection = (aimPoint - rightFirePoint.position).normalized;

            GameObject rightProjectile = Instantiate(rightProjectilePrefab, rightFirePoint.position, Quaternion.LookRotation(rightShotDirection));
            Rigidbody projectileRb = rightProjectile.GetComponent<Rigidbody>();
            projectileRb.velocity = Vector3.zero;
            projectileRb.AddForce(rightShotDirection * rightShootForce, ForceMode.Impulse);
            currentAmmo--;
            RightWeaponAnimator.SetTrigger("Shoot");
            UiManager.rightWeaponUi.GetComponent<Animator>().SetTrigger("Shoot");
            AudioManager.Instance.PlayOneShot("Right Gunshot", RightWeaponAnimator.gameObject);
            UiManager.UpdateRightCrosshair("Shoot");
            UiManager.playerAmmoText.GetComponent<Animator>().SetTrigger("Trigger");
            UiManager.UpdatePlayerAmmoCount();
            rightNextFireTime = Time.time + rightFireRate;
        }
        else if (currentAmmo <= 0 && Time.time >= errorSoundCooldown)
        {
            UiManager.playerAmmoText.GetComponent<Animator>().SetTrigger("Trigger");
            AudioManager.Instance.PlayOneShot("Reload Error", RightWeaponAnimator.gameObject);
            errorSoundCooldown = Time.time + ERROR_SOUND_INTERVAL; 
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
        UiManager.reloadUi.GetComponent<Animator>().SetBool("Reload", true);
        AudioManager.Instance.PlayOneShot("Reload", RightWeaponAnimator.gameObject);
        yield return new WaitForSeconds(0.5f);
        currentAmmo = maxAmmo;
        UiManager.UpdatePlayerAmmoCount();
        UiManager.playerAmmoText.GetComponent<Animator>().SetTrigger("Trigger");
        yield return new WaitForSeconds(0.5f);
        UiManager.reloadUi.GetComponent<Animator>().SetBool("Reload", false);
        isReloading = false;
    }

    private void LeftWeaponShooting()
    {
        if (Time.time >= leftNextFireTime)
        {
            Vector3 aimPoint = GetAimPoint();
            Vector3 leftShotDirection = (aimPoint - leftFirePoint.position).normalized;

            GameObject leftProjectile = Instantiate(leftProjectilePrefab, leftFirePoint.position, Quaternion.LookRotation(leftShotDirection));
            Rigidbody projectileRb = leftProjectile.GetComponent<Rigidbody>();
            projectileRb.velocity = Vector3.zero;
            projectileRb.AddForce(leftShotDirection * leftShootForce + Vector3.up * leftLobForce, ForceMode.Impulse);

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

        //foreach (Collider hit in colliders)
        //{
        //    Rigidbody rb = hit.GetComponent<Rigidbody>();
        //    if (rb != null) rb.AddExplosionForce(grenadeExplosionForce, explosionPos, grenadeExplosionRadius, 1f);
        //}
    }

    private Vector3 GetAimPoint()
    {
        Ray ray = PlayerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        Debug.DrawLine(ray.origin, ray.origin + ray.direction * aimDistance, Color.red, 1f);

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
    public void TakeDamage(float damageTaken)
    {
        if (currentPlayerState == PlayerState.DEAD) return;
        if (hasTakenDamaged) return;

        StartCoroutine(Iframe());
        currentHealth -= damageTaken;
        RandomiseHurtAudio();
        UiManager.DisplayDamagedOverlay();

        if (currentHealth <= 0)
        {
            ChangePlayerState(PlayerState.DEAD);
        }
        else if (currentHealth <= maxHealth * 0.4f)
        {
            UiManager.DisplayLowHealthOverlay(true);
        }
    }

    private void CheckHealthBar()
    {
        if (currentHealth <= 0.4f * maxHealth) UiManager.FlashHealth(true);
        else UiManager.FlashHealth(false);
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
        if (!isDead)
        {
            StartCoroutine(DeathSequence());
        }
    }

    private IEnumerator DeathSequence()
    {
        isDead = true;
        AudioManager.Instance.PlayOneShot("Death", gameObject);
        UiManager.DisplayDeathOverlay();
        yield return new WaitForSeconds(2f);
        UiManager.DisplayLoseScreen();
        GameManager.ChangeGameState(GameManager.GameState.WAIT);
    }
    #endregion

    #region Healing
    private void HealingEnabled()
    {
        if (isReloading) return;
        if (!canHeal || isHealing) return;
        if (currentHealth >= maxHealth) return;

        if (Input.GetKeyDown(KeyCode.E)) StartCoroutine(HealPlayer());
    }

    public IEnumerator HealPlayer()
    {
        isHealing = true;
        RightWeaponAnimator.SetTrigger("Healing");
        UiManager.playerHealUi.GetComponent<Animator>().SetBool("Healing", true);
        UiManager.DisplayLowHealthOverlay(false);
        yield return new WaitForSeconds(0.5f);
        AudioManager.Instance.PlayOneShot("Healing", RightWeaponAnimator.gameObject);
        currentHealth = maxHealth;
        UiManager.DisplayHealOverlay();
        yield return new WaitForSeconds(1f);
        UiManager.playerHealUi.GetComponent<Animator>().SetBool("Healing", false);
        isHealing = false;
    }
    #endregion

    #region Other Collisions
    private void OnTriggerEnter(Collider target)
    {
        if (target.CompareTag("Healing"))
        {
            canHeal = true;
        }

        if (target.CompareTag("Respawn"))
        {
            StartCoroutine(FallingOut());
        }

        if (target.CompareTag("Music 1"))
        {
            AudioManager.Instance.FadeOut("Main BGM", 5f);
            Destroy(target.gameObject);
        }

        if (target.CompareTag("Music 2"))
        {
            AudioManager.Instance.FadeIn("Boss BGM", 5f);
            Destroy(target.gameObject);
        }

        if (target.CompareTag("Ending"))
        {
            StartCoroutine(GameManager.FadeToggle(false, "Ending"));
        }
    }

    private void OnTriggerExit(Collider target)
    {
        if (target.CompareTag("Healing"))
        {
            canHeal = false;
        }
    }
    #endregion
}

