using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;



public class EnemyScript : MonoBehaviour, IDamageable
{
    #region Sprites

    [Header("Situations")] [SerializeField]
    private Sprite gunSprite;

    [SerializeField] private Sprite reloadSprite;
    [SerializeField] private Sprite handSprite;
    [SerializeField] private GameObject mainSprite;
    [SerializeField] private Image reloadBar;
    private SpriteRenderer sr;
    [SerializeField] private Sprite[] deathSprite;

    [Header("Heart")] [SerializeField] private GameObject HeartBarObject;
    [SerializeField] private Image heartBar;

    #endregion

    #region PlayerSettings

    [Header("Player Settings")] [SerializeField]
    private LayerMask ly;

    private Transform player;
    private bool isİnside, isChase;

    #endregion

    #region EnemySettings

    [Header("Enemy Settings")] private bool isFire = true;
    [SerializeField] public GameObject Bullet;
    private GameObject bullet;
    [SerializeField] private Transform bulletPosition;
    [SerializeField] private float speed;
    [SerializeField] private float heart;
    private float FinalValue;
    private bool isMove = true;
    private Rigidbody2D rb;

    #endregion

    #region Posts

    [Header("Posts")] [SerializeField] private Transform[] Posts;
    private byte activePost;

    #endregion

    #region Sounds

    private GameManager gm;

    #endregion

    private void Awake()
    {
        player = GameObject.FindWithTag("Player").transform;
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        gm = GameObject.FindWithTag("Manager").GetComponent<GameManager>();
    }

    private void Start()
    {
        bullet = Instantiate(Bullet, bulletPosition.position, quaternion.identity);
        bullet.SetActive(false);
    }

    private void Update()
    {
        CheckPlace();
    }

    void CheckPlace()
    {
        isİnside = Physics2D.OverlapCircle(transform.position, 4f, ly);

        LookPlayer(LookAt());

        if (isChase)
        {
            float distance = Vector2.Distance(transform.position, player.position);
            if (DistanceControl(distance))
            {
                if (isFire)
                    StartCoroutine(Shoot());
            }
            else if (isFire)
            {
                GoPost();
            }
        }

        else if (isİnside)
        {
            isChase = true;
            /*Debug.Log("İçeride");
            if (isFire)
                StartCoroutine(Shoot()); */
        }
        else if (isFire)
            GoPost();
    }

    bool DistanceControl(float distance)
    {
        return distance < 3.5f && isİnside;
    }

    Transform LookAt()
    {
        if (isİnside || isChase)
            return player;

        return Posts[activePost];
    }

    void LookPlayer(Transform Look)
    {
        Vector3 direction = Look.position - transform.position;
        direction.Normalize();
        Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, direction);
        Quaternion newRotation = Quaternion.Euler(targetRotation.eulerAngles.x, targetRotation.eulerAngles.y,
            targetRotation.eulerAngles.z + 95f);

        transform.rotation = newRotation;
    }

    void GoPost()
    {
        if (isMove)
            transform.position =
                Vector2.MoveTowards(transform.position, LookAt().position, speed * Time.deltaTime);
    }

    IEnumerator Shoot()
    {
        isFire = false;
        yield return new WaitForSeconds(1f);
        if (heart <= 0)
            yield break;
        if (isİnside)
        {
            bullet.transform.position = bulletPosition.position;
            bullet.SetActive(true);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            Vector2 forcePosition = player.transform.position - transform.position;
            forcePosition.Normalize();
            rb.AddForce(forcePosition * 35, ForceMode2D.Impulse);
            StartCoroutine(Reload());
        }
        else if (!isİnside)
            isFire = true;
    }

    IEnumerator Reload()
    {
        if (heart <= 0)
            yield break;
        mainSprite.SetActive(true);
        float fireCooldown = 0f;
        while (fireCooldown < 1f)
        {
            fireCooldown += Time.deltaTime / 3;
            reloadBar.fillAmount = fireCooldown;
            if (heart <= 0)
                break;
            yield return null;
        }

        mainSprite.SetActive(false);
        if (heart > 0)
        {
            sr.sprite = gunSprite;
            yield return new WaitForSeconds(1f);
            isFire = true;
        }
    }

    public IEnumerator OnHeartBar(float damage)
    {
        HeartBarObject.SetActive(true);
        float visionTime = 0f;
        FinalValue = heart + damage;
        float OldHeart = heart;
        heart += damage;
        isMove = false;
        while (heartBar.fillAmount > FinalValue / 100 && FinalValue > 0)
        {
            visionTime += Time.deltaTime;
            heartBar.fillAmount = Mathf.Lerp(OldHeart, FinalValue, visionTime) / 100;
            yield return null;
        }

        if (heart <= 0)
        {
            if (gm.settings.isJuice)
            {
                int randomValue = Random.Range(0, deathSprite.Length);
                sr.sprite = deathSprite[randomValue];
                sr.color = Color.red;
                CircleCollider2D cr = GetComponent<CircleCollider2D>();
                Destroy(rb);
                Destroy(cr);
                gm.PlaySfx(8);
                enabled = false;
            }
            else
            {
                CircleCollider2D cr = GetComponent<CircleCollider2D>();
                Destroy(rb);
                Destroy(cr);
                sr.sprite = null;
                Destroy(gameObject,1f);
            }
        }

        HeartBarObject.SetActive(false);
        isMove = true;
        if (rb != null)
            rb.velocity = Vector2.zero;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, 4f);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Post"))
        {
            switch (activePost)
            {
                case 0:
                    activePost = 1;
                    break;
                case 1:
                    activePost = 0;
                    break;
            }
        }
    }
}