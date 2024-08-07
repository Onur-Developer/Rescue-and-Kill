using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class PlayerScript : MonoBehaviour, IDamageable
{
    #region PlayerStatics

    [Header("Player Statics")] [SerializeField]
    private float speed;

    [SerializeField] private float coolDown;

    [SerializeField] private bool isAttack;
    private bool isReload = true;
    private float FinalValue;
    [SerializeField] private float heart;
    private Rigidbody2D rb;

    #endregion

    #region PlayerAnimation

    [Header("Player Animation")] [SerializeField]
    private Animator anim;

    [SerializeField] private PlayerSituation playerSituation;
    [SerializeField] private PlayerAnimation playerAnimation;

    #endregion

    #region Mouse Position

    private Vector3 myMousePos;
    [SerializeField] private Camera maincam;

    #endregion

    #region Bullets

    [Header("Bullets")] [SerializeField] private GameObject Bullet;
    [SerializeField] private GameObject Bullet2;
    [SerializeField] private Transform bulletPosition;
    [SerializeField] private GameObject[] Bullets;
    private int bulletCount, bulletCount2 = 5;
    [SerializeField] private float bulletForce;
    [SerializeField] private float ammo1Max;
    [SerializeField] private float ammo2Max;
    [SerializeField] private float ammo1;
    [SerializeField] private float ammo2;
    [SerializeField] private float currentMax;
    [SerializeField] private float currentAmmo;

    #endregion

    #region WeaponStatics

    [Header("WeaponSettings")] [SerializeField]
    private int activeWeapon;

    #endregion

    #region Canvas

    [Header("Canvas")] [SerializeField] private Text AmmoText;
    [SerializeField] private Text MaxAmmoText;
    [SerializeField] private Text GunNameText;
    [SerializeField] private Image heartBar;
    [SerializeField] private Text gainText;
    [SerializeField] private ShakeScript shakeCamera;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject winPanel, deathPanel;

    #endregion

    #region Lights

    [Header("Lights")] [SerializeField] private Light2D pointLight;

    #endregion

    private GameManager gm;


    enum PlayerSituation
    {
        Idle,
        Move,
        Attack,
        GunAttack,
        Reload
    }

    enum PlayerAnimation
    {
        Knife,
        Handgun,
        Shotgun
    }

    private void Awake()
    {
        InstantiateBullet();
        rb = GetComponent<Rigidbody2D>();
        gm = GameObject.FindWithTag("Manager").GetComponent<GameManager>();
    }

    private void Start()
    {
        if (playerAnimation == PlayerAnimation.Handgun)
        {
            Ammo1 = 0;
            currentAmmo = Ammo1;
            currentMax = ammo1Max;
        }
        else if (playerAnimation == PlayerAnimation.Shotgun)
        {
            Ammo2 = 0;
            currentAmmo = Ammo2;
            currentMax = ammo2Max;
        }
    }


    private void Update()
    {
        if (Time.timeScale != 0)
        {
            if (isReload)
            {
                if (Input.GetMouseButtonDown(1) && isAttack)
                {
                    gm.PlaySfx(12);
                    playerSituation = PlayerSituation.Attack;
                    isAttack = false;
                }
                else if (Input.GetMouseButtonDown(0) && isAttack && playerAnimation != PlayerAnimation.Knife)
                {
                    playerSituation = PlayerSituation.GunAttack;
                    StartCoroutine(Attack());
                    if (playerAnimation == PlayerAnimation.Handgun)
                        Shoot();
                    else
                        Shoot2();
                }

                Move();
                ChangeWeapon();
            }

            if (Input.GetKeyDown(KeyCode.R) && playerAnimation != PlayerAnimation.Knife)
            {
                if (currentMax > 0 && currentAmmo != 5 && playerAnimation == PlayerAnimation.Handgun)
                {
                    isReload = false;
                    playerSituation = PlayerSituation.Reload;
                    gm.PlaySfx(5);
                    Animation();
                }
                else if (currentMax > 0 && currentAmmo != 2 && playerAnimation == PlayerAnimation.Shotgun)
                {
                    isReload = false;
                    playerSituation = PlayerSituation.Reload;
                    gm.PlaySfx(3);
                    Animation();
                }
            }

            FollowMousePos();

            if (Input.GetKeyDown(KeyCode.Q) && playerAnimation != PlayerAnimation.Knife && gm.settings.isJuice)
            {
                pointLight.enabled = !pointLight.enabled;
                gm.PlaySfx(7);
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
            PauseButton(0);
    }


    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("HandgunAmmo"))
        {
            gm.PlaySfx(10);
            GainText("+5 Pistol Ammo", col.gameObject);
            ammo1Max += 5;
            if (playerAnimation == PlayerAnimation.Handgun)
            {
                Ammo1 = 0;
                currentMax = ammo1Max;
            }
        }
        else if (col.CompareTag("ShotgunAmmo"))
        {
            gm.PlaySfx(10);
            GainText("+2 Shotgun Ammo", col.gameObject);
            ammo2Max += 2;
            if (playerAnimation == PlayerAnimation.Shotgun)
            {
                Ammo2 = 0;
                currentMax = ammo2Max;
            }
        }
        else if (col.CompareTag("Hostage"))
        {
            gm.PlaySfx(11);
            GainText("Hostage Rescued", col.gameObject);
            winPanel.SetActive(gm.SavedHostage());
            if (winPanel.activeSelf)
            {
                gm.ChangeCursor(-1);
                Time.timeScale = 0;
            }
        }
        else if (col.CompareTag("HeartPlus"))
        {
            gm.PlaySfx(9);
            GainText("+25 Heart", col.gameObject);
            StartCoroutine(OnHeartBar(25f));
        }
    }

    public void PauseButton(int value)
    {
        switch (value)
        {
            case 0:
                pausePanel.SetActive(!pausePanel.activeSelf);
                if (pausePanel.activeSelf)
                {
                    gm.ChangeCursor(-1);
                    Time.timeScale = 0;
                }
                else
                {
                    gm.ChangeCursor(0);
                    Time.timeScale = 1;
                }

                break;
            case 1:
                Time.timeScale = 1;
                SceneManager.LoadScene(0);
                break;
            case 2:
                Application.Quit();
                break;
        }
    }

    void GainText(string textc, GameObject obj)
    {
        gainText.gameObject.SetActive(true);
        gainText.transform.localPosition = obj.transform.position;
        gainText.text = textc;
        Destroy(obj.gameObject);
        StartCoroutine(TextUp());
    }

    void Move()
    {
        float xAxis = Input.GetAxisRaw("Horizontal");
        float yAxis = Input.GetAxisRaw("Vertical");
        Vector3 movement = new Vector3(xAxis, yAxis) * speed * Time.deltaTime;
        transform.position += movement;

        if (movement == Vector3.zero && !anim.GetCurrentAnimatorStateInfo(0).IsName(playerAnimation.ToString() + 0)
                                     && isAttack)
        {
            playerSituation = PlayerSituation.Idle;
        }
        else if (movement != Vector3.zero &&
                 !anim.GetCurrentAnimatorStateInfo(0).IsName(playerAnimation.ToString() + 1) && isAttack)
        {
            playerSituation = PlayerSituation.Move;
        }

        Animation();
    }

    void Animation()
    {
        bool currentAnim = anim.GetCurrentAnimatorStateInfo(0)
            .IsName(playerAnimation.ToString() + (int)playerSituation);

        if (!currentAnim)
        {
            anim.Play(playerAnimation.ToString() + (int)playerSituation);
        }
    }

    void FollowMousePos()
    {
        myMousePos = maincam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 rotation = myMousePos - transform.position;

        float rotz = Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0, 0, rotz);
    }

    IEnumerator Reload()
    {
        isReload = false;
        playerSituation = PlayerSituation.Reload;
        Animation();
        yield return null;
    }

    IEnumerator Attack()
    {
        gm.ChangeCursor(1);
        isAttack = false;
        yield return new WaitForSeconds(coolDown);
        gm.ChangeCursor(0);
        isAttack = true;
        playerSituation = PlayerSituation.Move;
    }

    IEnumerator TextUp()
    {
        float myTime = 0f;
        while (myTime < 1f && Time.timeScale != 0)
        {
            Vector2 gainPos = gainText.transform.localPosition;
            gainText.transform.localPosition = new Vector2(gainPos.x, gainPos.y + 0.2f);
            myTime += Time.deltaTime;
            yield return null;
        }

        gainText.gameObject.SetActive(false);
    }

    public void AttackControl()
    {
        isAttack = true;
    }

    public void ReloadControl()
    {
        isReload = true;
        UpdateGunBullet();
    }

    void InstantiateBullet()
    {
        for (int i = 0; i < 5; i++)
        {
            GameObject bullet = Instantiate(Bullet, bulletPosition.position, bulletPosition.rotation);
            Bullets[i] = bullet;
        }

        for (int i = 5; i < 10; i++)
        {
            GameObject bullet = Instantiate(Bullet2, bulletPosition.position, bulletPosition.rotation);
            Bullets[i] = bullet;
        }

        for (int i = 0; i < Bullets.Length; i++)
        {
            Bullets[i].SetActive(false);
        }
    }

    void Shoot()
    {
        if (ammo1 > 0)
        {
            Bullets[bulletCount].transform.position = bulletPosition.position;
            Bullets[bulletCount].transform.rotation = bulletPosition.rotation;
            Bullets[bulletCount].SetActive(true);
            Rigidbody2D rb = Bullets[bulletCount].GetComponent<Rigidbody2D>();
            Vector3 mousePos = maincam.ScreenToWorldPoint(Input.mousePosition);
            Vector3 shootPoint = mousePos - transform.position;
            shootPoint.Normalize();
            rb.AddForce(shootPoint * bulletForce, ForceMode2D.Impulse);
            bulletCount++;
            if (bulletCount >= 5)
                bulletCount = 0;
            Ammo1 = 1;
            shakeCamera.ShakeCamera(1f);
            gm.PlaySfx(0);
        }
        else
            gm.PlaySfx(2);
    }

    void Shoot2()
    {
        if (ammo2 > 0)
        {
            Bullets[bulletCount2].transform.position = bulletPosition.position;
            Bullets[bulletCount2].transform.rotation = bulletPosition.rotation;
            Bullets[bulletCount2].SetActive(true);
            Rigidbody2D rb = Bullets[bulletCount2].GetComponent<Rigidbody2D>();
            Vector3 mousePos = maincam.ScreenToWorldPoint(Input.mousePosition);
            Vector3 shootPoint = mousePos - transform.position;
            shootPoint.Normalize();
            rb.AddForce(shootPoint * bulletForce, ForceMode2D.Impulse);
            bulletCount2++;
            if (bulletCount2 >= 10)
                bulletCount2 = 5;
            Ammo2 = 1;
            shakeCamera.ShakeCamera(2f);
            gm.PlaySfx(1);
        }
        else
            gm.PlaySfx(2);
    }

    void ChangeWeapon()
    {
        float scrollWheel = Input.GetAxis("Mouse ScrollWheel");

        if (scrollWheel != 0)
        {
            if (scrollWheel > 0)
                activeWeapon++;
            else if (scrollWheel < 0)
                activeWeapon--;

            if (activeWeapon >= 3)
                activeWeapon = 0;
            else if (activeWeapon <= -1)
                activeWeapon = 2;

            playerAnimation = (PlayerAnimation)activeWeapon;

            if (playerAnimation == PlayerAnimation.Knife)
            {
                AmmoText.text = 0.ToString();
                MaxAmmoText.text = 0.ToString();
                GunNameText.text = "Knife";
                pointLight.enabled = false;
            }
            else if (playerAnimation == PlayerAnimation.Handgun)
            {
                Ammo1 = 0;
                currentMax = ammo1Max;
            }
            else if (playerAnimation == PlayerAnimation.Shotgun)
            {
                Ammo2 = 0;
                currentMax = ammo2Max;
            }

            gm.PlaySfx(6);
        }
    }

    void UpdateGunBullet()
    {
        if (playerAnimation == PlayerAnimation.Handgun)
        {
            float addetAmmo = 0;
            for (float i = 0; i < ammo1Max; i++)
            {
                ammo1++;
                addetAmmo++;
                if (ammo1 == 5)
                {
                    break;
                }
            }

            ammo1Max -= addetAmmo;
            Ammo1 = 0;
            currentMax = ammo1Max;
            currentAmmo = Ammo1;
        }
        else if (playerAnimation == PlayerAnimation.Shotgun)
        {
            gm.PlaySfx(4);
            float addetAmmo = 0;
            for (float i = 0; i < ammo2Max; i++)
            {
                ammo2++;
                addetAmmo++;
                if (ammo2 == 2)
                {
                    break;
                }
            }

            ammo2Max -= addetAmmo;
            Ammo2 = 0;
            currentMax = ammo2Max;
            currentAmmo = Ammo2;
        }
    }

    public void MeeleAttack(float damage)
    {
        Collider2D[] isEnemy = Physics2D.OverlapCircleAll(bulletPosition.position, 0.75f);
        foreach (var Enemy in isEnemy)
        {
            if (Enemy.CompareTag("Enemy"))
            {
                EnemyScript es = Enemy.GetComponent<EnemyScript>();
                es.StartCoroutine(es.OnHeartBar(damage));
            }
        }
    }

    float Ammo1
    {
        get { return ammo1; }
        set
        {
            ammo1 -= value;
            currentAmmo = ammo1;
            AmmoText.text = Ammo1.ToString();
            MaxAmmoText.text = ammo1Max.ToString();
            GunNameText.text = "Pistol";
        }
    }

    float Ammo2
    {
        get { return ammo2; }
        set
        {
            ammo2 -= value;
            currentAmmo = ammo2;
            AmmoText.text = Ammo2.ToString();
            MaxAmmoText.text = ammo2Max.ToString();
            GunNameText.text = "ShotGun";
        }
    }

    public IEnumerator OnHeartBar(float value)
    {
        speed = 0f;
        float visionTime = 0f;
        FinalValue = heart + value;
        float OldHeart = heart;
        heart += value;
        heart = Mathf.Clamp(heart, 0, 100);

        while (heartBar.fillAmount != heart / 100)
        {
            visionTime += Time.deltaTime;
            heartBar.fillAmount = Mathf.Lerp(OldHeart, FinalValue, visionTime) / 100;
            yield return null;
        }

        if (heart <= 0)
        {
            deathPanel.SetActive(true);
            gm.ChangeCursor(-1);
            Time.timeScale = 0;
        }

        if (rb != null)
        {
            //rb.velocity = Vector2.zero;
            StartCoroutine(VelocityControl());
        }

        speed = 3.5f;
    }

    IEnumerator VelocityControl()
    {
        float timer = 0f;
        while (timer < 1f)
        {
            timer += Time.deltaTime;
            rb.velocity=Vector2.zero;
            yield return null;
        }
    }
}

interface IDamageable
{
    public IEnumerator OnHeartBar(float damage);
}