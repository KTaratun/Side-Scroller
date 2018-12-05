using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : CharacterBase {

    enum walls { NO_WALL, WALL_LEFT, WALL_RIGHT};

    float v_offGroundVal = 0.1f;

    public float m_offGround;
    public GameObject[] m_inventory;

    // Use this for initialization
    new protected void Start()
    {
        base.Start();

        m_moveSpeed = 5;
        m_offGround = 0;

        m_inventory = new GameObject[12];
    }

    // Update is called once per frame
    new protected void Update()
    {
       if (!m_isAlive)
           return;

       base.Update();

       ControllerInput();

       if (m_rb.velocity.y != 0 && !Input.GetButton("Jump"))
           m_offGround = -100;
    }

    public void ControllerInput()
    {
        Moving(); // "A", "D"
        Jumping(); // "SPACE", "S"
        Combat(); // "M1", M2"
    }

    protected void Moving()
    {
        //AnimatorStateInfo aSInfo = m_anim.GetCurrentAnimatorStateInfo(0);

        float maxSpeed = 5;
        float maxRunSpeed = 8;
        float jumpControl = .2f;

        if (m_coolDown == 0) // aSInfo.fullPathHash != Animator.StringToHash("Base Layer.HeroSwordSwipe")
        {
            if (Input.GetAxisRaw("Horizontal") < 0 && m_rb.velocity.x > -maxSpeed ||
            Input.GetAxisRaw("Horizontal") < 0 && m_rb.velocity.x > -maxRunSpeed && Input.GetKey(KeyCode.LeftShift))
            {
                //transform.SetPositionAndRotation(new Vector3(transform.position.x - m_moveSpeed, transform.position.y, transform.position.z), transform.rotation);
                if (m_rb.velocity.y == 0)
                    m_rb.AddForce(new Vector2(-m_moveSpeed, 0), ForceMode2D.Impulse);
                else if (m_rb.velocity.x > -maxSpeed)
                    m_rb.AddForce(new Vector2(-m_moveSpeed * jumpControl, 0), ForceMode2D.Impulse);

                m_anim.Play("HeroWalk");

                transform.SetPositionAndRotation(transform.position, Quaternion.Euler(0, 180, 0));
            }
            else if (Input.GetAxisRaw("Horizontal") > 0 && m_rb.velocity.x < maxSpeed ||
                Input.GetAxisRaw("Horizontal") > 0 && m_rb.velocity.x < maxRunSpeed && Input.GetKey(KeyCode.LeftShift))
            {
                //transform.SetPositionAndRotation(new Vector3(transform.position.x + m_moveSpeed, transform.position.y, transform.position.z), transform.rotation);
                if (m_rb.velocity.y == 0)
                    m_rb.AddForce(new Vector2(m_moveSpeed, 0), ForceMode2D.Impulse);
                else if (m_rb.velocity.x < maxSpeed)
                    m_rb.AddForce(new Vector2(m_moveSpeed * jumpControl, 0), ForceMode2D.Impulse);

                m_anim.Play("HeroWalk");

                transform.SetPositionAndRotation(transform.position, Quaternion.Euler(0, 0, 0));
            }
        }
    }

    protected void Jumping()
    {
        if (Input.GetButtonDown("Jump") && m_offGround > 0 && !Input.GetKey(KeyCode.S) && m_rb.velocity.y == 0)
        {
            m_rb.AddForce(new Vector2(0, 15), ForceMode2D.Impulse);
            m_rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        else if (Input.GetButtonDown("Jump") && m_offGround > 0 && Input.GetKey(KeyCode.S) && m_rb.constraints == RigidbodyConstraints2D.FreezeAll) // if you are grabbing a ledge
        {
            m_rb.velocity = new Vector2(m_rb.velocity.x, -5f);
            m_rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        else if (Input.GetButton("Jump") && m_offGround > 0 && !Input.GetKey(KeyCode.S))
        {
            m_rb.AddForce(new Vector2(0, m_jumpForce), ForceMode2D.Impulse);
        
            m_offGround -= Time.deltaTime;
        }
        //else if (Input.GetButtonUp("Jump") && m_offGround > -100 && m_rb.velocity.y > 0)
        //    m_rb.velocity = new Vector2(m_rb.velocity.x, m_rb.velocity.y * .05f);
    }
    
    protected void Combat()
    {
        if (Input.GetButtonDown("Fire1") && m_coolDown == 0)
        {
            m_weapon.gameObject.SetActive(true);
            GetComponent<Animator>().Play("HeroSwordSwipe");
            m_coolDown = m_weapon.m_attackTime;

            if (m_rb.velocity.y == 0)
            {
                if (transform.eulerAngles.y == 180)
                    m_rb.AddForce(new Vector2(-16f, 0), ForceMode2D.Impulse);
                else
                    m_rb.AddForce(new Vector2(16f, 0), ForceMode2D.Impulse);
            }
        }
        else if (Input.GetButtonDown("Fire2") && m_coolDown == 0)
        {
            if (transform.eulerAngles.y == 0 && m_rb.velocity.x >= -1 ||
                transform.eulerAngles.y == 180 && m_rb.velocity.x <= 1)
            {
                m_rb.velocity = Vector2.zero;
                Jump(-30, 0);
                m_coolDown = .3f;
            }
        }
    }

    protected void PickUp(GameObject _item)
    {
        for (int i = 0; i < m_inventory.Length; i++)
        {
            if (!m_inventory[i])
            {
                m_inventory[i] = _item;
                _item.SetActive(false);
                return;
            }
        }
    }

    protected void OnCollisionEnter2D(Collision2D collision)
    {
        // If it's floor, ignore
        if (collision.gameObject.name == "RectangleObject" && collision.transform.position.y < transform.position.y - .5f)
            return;
            
        // If it's wall, stall
        if (collision.gameObject.name == "RectangleObject" && collision.transform.position.x > transform.position.x + .3f)
            m_rb.AddForce(new Vector2(-2, 0), ForceMode2D.Impulse);
        else if (collision.gameObject.name == "RectangleObject" && collision.transform.position.x < transform.position.x - .3f)
            m_rb.AddForce(new Vector2(2, 0), ForceMode2D.Impulse);
    }

    protected void OnCollisionStay2D(Collision2D collision)
    {
        // Reset Jump when on ground
        if (collision.gameObject.name == "RectangleObject" && collision.transform.position.y < transform.position.y - .5f &&
            m_rb.velocity.y == 0)
            m_offGround = v_offGroundVal;

        // Grab ledges
        else if (collision.gameObject.name == "RectangleObject")
        {
            Vector2 bounds = collision.gameObject.GetComponent<BoxCollider2D>().bounds.center;

            Vector3 start = new Vector3(bounds.x, bounds.y + 1.6f, collision.transform.position.z);
            Vector3 end = new Vector3(bounds.x, bounds.y, collision.transform.position.z);

            RaycastHit2D hit = Physics2D.Linecast(start, end, 1);
            Debug.DrawLine(start, end);

            if (bounds.y - .2f < transform.position.y && bounds.y + .2f > transform.position.y &&
                hit.point.y <= start.y - .2f && !Input.GetButton("Jump"))
            {
                m_rb.velocity = Vector2.zero;
                m_rb.constraints = RigidbodyConstraints2D.FreezeAll;
                m_offGround = v_offGroundVal;
            }
        }
    }

    override protected void OnTriggerStay2D(Collider2D collider)
    {
        base.OnTriggerStay2D(collider);

        // Reset Jump when on ground
        if (collider.gameObject.name == "RectangleObject" && collider.transform.position.y < transform.position.y - .5f &&
            m_rb.velocity.y == 0)
            m_offGround = v_offGroundVal;

        if (collider.gameObject.tag == "Item" && Input.GetKeyDown(KeyCode.E))
            PickUp(collider.gameObject);
    }
}
