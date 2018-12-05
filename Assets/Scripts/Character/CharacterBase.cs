using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBase : MonoBehaviour {

    protected float v_slideVal = 0.9f;

    public WeaponBase m_weapon;

    public float m_moveSpeed = 0.05f;
    public float m_jumpForce = 8.0f;
    public float m_coolDown = 0.0f;
    public int m_health = 50;
    public bool m_isAlive = true;
    public float m_slideFactor;

    protected Rigidbody2D m_rb;
    protected Animator m_anim;

    // Use this for initialization
    protected void Start()
    {
        m_rb = GetComponent<Rigidbody2D>();
        m_anim = GetComponent<Animator>();
        m_slideFactor = v_slideVal;
    }

    // Update is called once per frame
    protected void Update()
    {
        CoolDown();

       // Affects the amount you slide
       if (m_rb.velocity.x != 0 && m_rb.velocity.y == 0)
            m_rb.velocity = new Vector2(m_rb.velocity.x * m_slideFactor, m_rb.velocity.y);
    }

    virtual protected void Jump(float _x, float _y)
    {
        if (m_rb.velocity.y != 0)
            return;

        if (transform.eulerAngles.y == 180)
            _x *= -1;
        m_rb.AddForce(new Vector2(_x, _y), ForceMode2D.Impulse);
    }

    virtual protected void CoolDown()
    {
        if (m_coolDown > 0)
        {
            m_coolDown -= Time.deltaTime;

            if (m_coolDown <= 0)
            {
                if (m_weapon)
                    m_weapon.ResetWeapon();
                m_coolDown = 0;
            }
        }
    }

    virtual public void Hit(int _damage, float _force)
    {
        m_health -= _damage;

        Knockback(_force);

        if (m_health <= 0)
            Death();
    }

    protected void Knockback(float _force)
    {
        float x = _force;
        float y = 2.5f;

        if (transform.position.x > transform.position.x)
            x *= -1;

        m_rb.AddForce(new Vector2(x, y), ForceMode2D.Impulse);
    }

    virtual public void Death()
    {
        transform.SetPositionAndRotation(new Vector3(transform.position.x, transform.position.y - .55f, transform.position.z), Quaternion.Euler(0, 0, 90));
        m_isAlive = false;
        GetComponent<CapsuleCollider2D>().isTrigger = true;
        m_rb.constraints = RigidbodyConstraints2D.FreezePosition;
    }

    virtual protected void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Platform")
        {
            CapsuleCollider2D boxCol = GetComponent<CapsuleCollider2D>();
            float colMaxYBounds = collision.gameObject.GetComponent<BoxCollider2D>().bounds.max.y;

            if (Input.GetButtonDown("Jump") && Input.GetKey(KeyCode.S))
            {
                transform.SetPositionAndRotation(new Vector3(transform.position.x, colMaxYBounds + 0.3f, transform.position.z), transform.rotation);
                m_rb.velocity = new Vector2(m_rb.velocity.x, -5f);
            }
            else if (m_rb.velocity.y <= 0 && boxCol.bounds.min.y > colMaxYBounds - 0.5f || m_rb.velocity.y < -15)
            {
                m_rb.velocity = new Vector3(m_rb.velocity.x, 0);
                m_rb.angularVelocity = 0;

                float bodyOffset = transform.position.y - boxCol.bounds.min.y;
                transform.SetPositionAndRotation(new Vector3(transform.position.x, colMaxYBounds + bodyOffset, transform.position.z), transform.rotation);

                m_slideFactor = 0.8f;
            }
        }
        //else if (collision.gameObject.tag == "PickUp")
    }

    virtual protected void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.gameObject.tag == "Platform")
            m_slideFactor = v_slideVal;
    }
}
