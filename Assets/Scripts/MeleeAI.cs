using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAI : AIBase {

    // Use this for initialization
    new protected void Start()
    {
        base.Start();
        m_attackDistance = 3.0f;
    }

    // Update is called once per frame
    new protected void Update()
    {
        if (!m_isAlive)
            return;

        base.Update();
    }

    protected override void CoolDown()
    {
        if (m_coolDown > 0)
        {
            m_coolDown -= Time.deltaTime;

            if (m_coolDown < 0)
                m_coolDown = 0;
        }

        if (m_weapon.activeSelf && m_coolDown <= m_weapon.GetComponent<WeaponBase>().m_attackTime * 0.5)
            m_weapon.GetComponent<WeaponBase>().ResetWeapon();
    }

    protected override void Attack(GameObject _player)
    {
        base.Attack(_player);

        float x = 6.5f;
        float y = 2.5f;
        if (_player.transform.position.x < transform.position.x)
            x *= -1;

        GetComponent<Rigidbody2D>().AddForce(new Vector2(x, y), ForceMode2D.Impulse);
    }
}
