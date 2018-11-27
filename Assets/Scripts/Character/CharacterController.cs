using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : CharacterBase {

    enum walls { NO_WALL, WALL_LEFT, WALL_RIGHT};

    walls m_isTouchingWall;

    bool m_runJump;

    // Use this for initialization
    new protected void Start()
    {
        base.Start();

        m_moveSpeed = 5;
        m_runJump = false;
    }

    // Update is called once per frame
    new protected void Update()
    {
        if (!m_isAlive)
            return;

        base.Update();

        ControllerInput();
    }

    public void ControllerInput()
    {
        float maxSpeed = 5;
        float maxRunSpeed = 8;
        float slideFactor = .9f;
        float jumpControl = .2f;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        Animator anim = GetComponent<Animator>();
        AnimatorStateInfo aSInfo = anim.GetCurrentAnimatorStateInfo(0);

        if (aSInfo.fullPathHash != Animator.StringToHash("Base Layer.HeroSwordSwipe"))
        {
            if (Input.GetAxisRaw("Horizontal") < 0 && GetComponent<Rigidbody2D>().velocity.x > -maxSpeed ||
            Input.GetAxisRaw("Horizontal") < 0 && GetComponent<Rigidbody2D>().velocity.x > -maxRunSpeed && Input.GetKey(KeyCode.LeftShift))
            {
                //transform.SetPositionAndRotation(new Vector3(transform.position.x - m_moveSpeed, transform.position.y, transform.position.z), transform.rotation);
                if (rb.velocity.y == 0)
                    rb.AddForce(new Vector2(-m_moveSpeed, 0), ForceMode2D.Impulse);
                else if (rb.velocity.x > -maxSpeed)
                    rb.AddForce(new Vector2(-m_moveSpeed * jumpControl, 0), ForceMode2D.Impulse);

                anim.Play("HeroWalk");

                transform.SetPositionAndRotation(transform.position, Quaternion.Euler(0, 180, 0));
            }
            else if (Input.GetAxisRaw("Horizontal") > 0 && rb.velocity.x < maxSpeed ||
                Input.GetAxisRaw("Horizontal") > 0 && rb.velocity.x < maxRunSpeed && Input.GetKey(KeyCode.LeftShift))
            {
                //transform.SetPositionAndRotation(new Vector3(transform.position.x + m_moveSpeed, transform.position.y, transform.position.z), transform.rotation);
                if (rb.velocity.y == 0)
                    rb.AddForce(new Vector2(m_moveSpeed, 0), ForceMode2D.Impulse);
                else if (rb.velocity.x < maxSpeed)
                    rb.AddForce(new Vector2(m_moveSpeed * jumpControl, 0), ForceMode2D.Impulse);

                anim.Play("HeroWalk");

                transform.SetPositionAndRotation(transform.position, Quaternion.Euler(0, 0, 0));
            }
        }

        // Effects the amount you slide
        if (rb.velocity.x != 0 && rb.velocity.y == 0)
            rb.velocity = new Vector2(rb.velocity.x * slideFactor, rb.velocity.y);

        if (Input.GetButtonDown("Jump") && Input.GetAxisRaw("Vertical") >= 0 && !Input.GetKey(KeyCode.LeftShift))
            Jump(0, m_jumpForce * .75f);
        else if (Input.GetButtonDown("Jump") && Input.GetAxisRaw("Vertical") >= 0 && Input.GetKey(KeyCode.LeftShift))
        {
            rb.velocity = Vector2.zero;
            Jump(0, m_jumpForce);
        }

        if (Input.GetButtonDown("Fire1") && aSInfo.fullPathHash != Animator.StringToHash("Base Layer.HeroSwordSwipe"))
        {
            m_weapon.SetActive(true);
            GetComponent<Animator>().Play("HeroSwordSwipe");
            m_coolDown = 0.5f;
        }
        else if (Input.GetButtonDown("Fire2") && aSInfo.fullPathHash != Animator.StringToHash("Base Layer.HeroSwordSwipe") &&
            rb.velocity.x > -2 && rb.velocity.x < 2)
        {
            Jump(-30, 0);
        }
    }

    virtual protected void OnCollisionStay2D(Collision2D collision)
    {
        //// If it's floor, ignore
        //if (collision.gameObject.name == "RectangleObject" && collision.transform.position.y < transform.position.y - .5f)
        //    return;

        //// If it's wall, stall
        //if (collision.gameObject.name == "RectangleObject" && collision.transform.position.x > transform.position.x + .3f)
        //{
        //    m_isTouchingWall = walls.WALL_RIGHT;
        //    Jump(2f, 0);
        //}
        //else if (collision.gameObject.name == "RectangleObject" && collision.transform.position.x < transform.position.x - .3f)
        //{
        //    m_isTouchingWall = walls.WALL_LEFT;
        //    Jump(-2f, 0);
        //}
    }

    override protected void OnCollisionEnter2D(Collision2D collision)
    {
        // If it's floor, ignore
        if (collision.gameObject.name == "RectangleObject" && collision.transform.position.y < transform.position.y - .5f)
            return;
            

        // If it's wall, stall
        if (collision.gameObject.name == "RectangleObject" && collision.transform.position.x > transform.position.x + .3f)
            GetComponent<Rigidbody2D>().AddForce(new Vector2(-2, 0), ForceMode2D.Impulse);
        else if (collision.gameObject.name == "RectangleObject" && collision.transform.position.x < transform.position.x - .3f)
            GetComponent<Rigidbody2D>().AddForce(new Vector2(2, 0), ForceMode2D.Impulse);
    }

    virtual protected void OnCollisionExit2D(Collision2D collision)
    {
        // Leave the wall
        if (collision.gameObject.name == "RectangleObject" && collision.transform.position.x > transform.position.x + .3f ||
            collision.gameObject.name == "RectangleObject" && collision.transform.position.x < transform.position.x - .3f)
            m_isTouchingWall = walls.NO_WALL;
    }
}
