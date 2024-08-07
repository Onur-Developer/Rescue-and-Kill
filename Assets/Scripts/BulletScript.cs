using UnityEngine;
using Random = UnityEngine.Random;



public class BulletScript : MonoBehaviour
{
    private Animator anim;
    private CircleCollider2D cr;
    private Rigidbody2D rb;
    [SerializeField] private Vector3 defaultScale;
    [SerializeField] private float damage;
    [SerializeField] private float pushValue;
    private Vector3 startedPosition;
    [SerializeField] private Settings _settings;
    private SpriteRenderer sr;


    private void Awake()
    {
        anim = GetComponent<Animator>();
        cr = GetComponent<CircleCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Enemy") || col.gameObject.CompareTag("Player"))
        {
            IDamageable es = col.gameObject.GetComponent<IDamageable>();
            StartCoroutine(es.OnHeartBar(damage));
            Rigidbody2D rigidbody = col.gameObject.GetComponent<Rigidbody2D>();
            Vector2 thrust = (col.transform.position - startedPosition).normalized;
            //thrust.Normalize();
            rigidbody.AddForce(thrust * pushValue);
            if (!col.CompareTag("Player"))
                col.gameObject.layer = 6;
            cr.enabled = false;
            rb.simulated = false;
            if (_settings.isJuice)
            {
                int randomValue = Random.Range(1, 6);
                anim.Play("BulletBlood" + randomValue);
            }
            else
            {
                anim.Play("BulletEmpty");
            }
        }

        else if (!col.gameObject.CompareTag("Post") && col.gameObject.layer!=7)
        {
            if (_settings.isJuice)
            {
                cr.enabled = false;
                rb.simulated = false;
                anim.Play("BulletExplosion");
            }
            else
            {
                CloseObject();
            }
        }
    }

    public void CloseObject()
    {
        rb.simulated = true;
        transform.localScale = defaultScale;
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        cr.enabled = true;
        startedPosition = transform.position;
    }
}