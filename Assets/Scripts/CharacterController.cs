using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : CharacterBase {

    // Use this for initialization
    new protected void Start()
    {
        base.Start();
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
        if (Input.GetAxisRaw("Horizontal") < 0)
        {
            transform.SetPositionAndRotation(new Vector3(transform.position.x - m_moveSpeed, transform.position.y, transform.position.z), transform.rotation);

            if (m_coolDown == 0)
                transform.SetPositionAndRotation(transform.position, Quaternion.Euler(0, 180, 0));
        }
        else if (Input.GetAxisRaw("Horizontal") > 0)
        {
            transform.SetPositionAndRotation(new Vector3(transform.position.x + m_moveSpeed, transform.position.y, transform.position.z), transform.rotation);

            if (m_coolDown == 0)
                transform.SetPositionAndRotation(transform.position, Quaternion.Euler(0, 0, 0));
        }

        if (Input.GetButtonDown("Jump") && Input.GetAxisRaw("Vertical") >= 0)
            Jump(0, m_jumpForce);

        if (Input.GetButtonDown("Fire1") && m_coolDown == 0)
        {
            m_weapon.SetActive(true);
            m_coolDown = 0.5f;
        }
    }
}
