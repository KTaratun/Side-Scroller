using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBase : MonoBehaviour {

    public GameObject m_weapon;

    public float m_moveSpeed = 0.05f;
    public float m_jumpForce = 8.0f;
    public float m_coolDown = 0.0f;
    public int m_health = 50;
    public bool m_isAlive = true;

    // Use this for initialization
    protected void Start()
    {
    }

    // Update is called once per frame
    protected void Update()
    {
        CoolDown();
    }

    virtual protected void Jump(float _x, float _y)
    {
        if (GetComponent<Rigidbody2D>().velocity.y != 0)
            return;

        if (transform.eulerAngles.y == 180)
            _x *= -1;
        GetComponent<Rigidbody2D>().AddForce(new Vector2(_x, _y), ForceMode2D.Impulse);
    }

    virtual protected void CoolDown()
    {
        if (m_coolDown > 0)
        {
            m_coolDown -= Time.deltaTime;

            if (m_coolDown <= 0)
            {
                if (m_weapon)
                    m_weapon.GetComponent<WeaponBase>().ResetWeapon();
                m_coolDown = 0;
            }
        }
    }

    virtual protected void OnCollisionEnter2D(Collision2D collision)
    {
        //if (collision.contacts.Length > 0)
        //{
        //    ContactPoint2D contact = collision.contacts[0];
        //    if (Vector3.Dot(contact.normal, Vector3.up) > 0.5)
        //        m_ground = collision.gameObject;
        //}
    }

    virtual protected void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.name == "platform")
        {
            Rigidbody2D rigidbody = GetComponent<Rigidbody2D>();
            BoxCollider2D boxCol = GetComponent<BoxCollider2D>();
            float colMaxYBounds = collision.GetComponent<BoxCollider2D>().bounds.max.y;

            if (rigidbody.velocity.y <= 0 && boxCol.bounds.min.y > colMaxYBounds - 0.5f &&
                (Input.GetAxisRaw("Vertical") >= 0 || !Input.GetButton("Jump")))
            {
                rigidbody.velocity = Vector3.zero;
                rigidbody.angularVelocity = 0;

                float bodyOffset = transform.position.y - boxCol.bounds.min.y;
                transform.SetPositionAndRotation(new Vector3(transform.position.x, colMaxYBounds + bodyOffset, transform.position.z), transform.rotation);
            }
        }
    }
}
