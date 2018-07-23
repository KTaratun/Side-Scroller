using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBase : MonoBehaviour {

    public GameObject m_weapon;
    public GameObject m_ground;

    public float m_moveSpeed = 0.05f;
    public float m_jumpForce = 8.0f;
    public float m_coolDown = 0.0f;
    public int m_health = 50;
    public bool m_isAlive = true;

    // Use this for initialization
    protected void Start ()
    {
        m_ground = null;	
	}
	
	// Update is called once per frame
	protected void Update ()
    {
        CoolDown();
    }

    virtual protected void Jump(float _x)
    {
        if (transform.eulerAngles.y == 180)
            _x *= -1;
        GetComponent<Rigidbody2D>().AddForce(new Vector2(_x, m_jumpForce), ForceMode2D.Impulse);
        m_ground = null;
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
        if (collision.gameObject.tag == "Ground")
            m_ground = collision.gameObject;
    }

    virtual protected void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground")
            m_ground = null;
    }
}
