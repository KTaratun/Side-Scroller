using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBase : CharacterBase
{

    public GameObject[] m_players;

    public float m_patrolSpeed;
    public float m_alerted;

    public float m_alertTime = 5.0f;
    public float m_attackDistance = 2.0f;
    public float m_sightDistance = 7.0f;

    // Use this for initialization
    new protected void Start()
    {
        base.Start();

        m_alerted = 0;
    }

    // Update is called once per frame
    new protected void Update()
    {
        base.Update();

        if (m_coolDown > 0 || !m_ground)
            return;

        if (m_alerted > 0)
        {
            m_alerted -= Time.deltaTime;
            if (m_alerted < 0)
                m_alerted = 0;
        }

        Vector3 direction = new Vector3(transform.position.x + 2, transform.position.y - 1.0f, transform.position.z);

        if (!CheckForPlayer(ref direction))
            Patrol(ref direction);

        CheckSight(direction);
    }

    protected void ChangeDirection()
    {
        if (transform.eulerAngles.y == 0)
            transform.SetPositionAndRotation(transform.position, Quaternion.Euler(0, 180, 0));
        else if (transform.eulerAngles.y == 180)
            transform.SetPositionAndRotation(transform.position, Quaternion.Euler(0, 0, 0));
    }

    protected bool CheckForPlayer(ref Vector3 _direction)
    {
        float closestDistance = 10000.0f;
        GameObject closestPlayer = null;

        for (int i = 0; i < m_players.Length; i++)
        {
            // Check if we are already alerted, in which case we don't care where the opponent is coming from. If not alerted, we want to be facing the right way.
            if (m_players[i].GetComponent<CharacterBase>().m_isAlive && Vector3.Distance(m_players[i].transform.position, transform.position) < m_sightDistance)
                if (m_alerted > 0 ||
                    transform.eulerAngles.y == 0 && m_players[i].transform.position.x > transform.position.x ||
                    transform.eulerAngles.y == 0 && m_players[i].transform.position.x < transform.position.x)
                {

                    RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, (m_players[i].transform.position - transform.position));

                    // If there is something else before the player, they are blocked and skipped.
                    //for (int j = 2; j < hits.Length; j++) // 1 to bypass self and range collider
                    //{
                    if (hits[1].transform == m_players[i].transform)
                    {
                        float currDist = Vector3.Distance(transform.position, m_players[i].transform.position);
                        if (currDist < closestDistance)
                        {
                            m_alerted = m_alertTime;
                            closestDistance = currDist;
                            closestPlayer = m_players[i];
                        }
                    }

                    break;
                    //}
                }
        }

        if (!closestPlayer)
            return false;

        MoveTowards(closestPlayer, ref _direction);
        Debug.DrawRay(transform.position, (closestPlayer.transform.position - transform.position));

        return true;
    }

    protected void MoveTowards(GameObject _player, ref Vector3 _direction)
    {
        if (_player.transform.position.x > transform.position.x)
        {
            transform.SetPositionAndRotation(new Vector3(transform.position.x + m_moveSpeed, transform.position.y, transform.position.z), transform.rotation);

            if (m_coolDown == 0)
                transform.SetPositionAndRotation(transform.position, Quaternion.Euler(0, 0, 0));

            _direction = new Vector3(transform.position.x + 2, transform.position.y - 1.0f, transform.position.z);
        }

        if (_player.transform.position.x < transform.position.x)
        {
            transform.SetPositionAndRotation(new Vector3(transform.position.x - m_moveSpeed, transform.position.y, transform.position.z), transform.rotation);

            if (m_coolDown == 0)
                transform.SetPositionAndRotation(transform.position, Quaternion.Euler(0, 180, 0));

            _direction = new Vector3(transform.position.x - 2, transform.position.y - 1.0f, transform.position.z);
        }

        if (Vector3.Distance(_player.transform.position, transform.position) < m_attackDistance)
            Attack(_player);
    }

    virtual protected void Attack(GameObject _player)
    {
        m_weapon.SetActive(true);
        m_coolDown = m_weapon.GetComponent<WeaponBase>().m_attackTime;
    }

    protected void Patrol(ref Vector3 _direction)
    {
        float speed;

        if (m_alerted > 0)
            speed = m_moveSpeed;
        else
            speed = m_patrolSpeed;

        if (transform.eulerAngles.y == 0)
        {
            transform.SetPositionAndRotation(new Vector3(transform.position.x + speed, transform.position.y, transform.position.z), transform.rotation);
            _direction = new Vector3(transform.position.x + 2, transform.position.y - 1.0f, transform.position.z);
        }
        else if (transform.eulerAngles.y == 180)
        {
            transform.SetPositionAndRotation(new Vector3(transform.position.x - speed, transform.position.y, transform.position.z), transform.rotation);
            _direction = new Vector3(transform.position.x - 2, transform.position.y - 1.0f, transform.position.z);
        }
    }

    protected void CheckSight(Vector3 _direction)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, (_direction - transform.position));
        Debug.DrawRay(transform.position, (_direction - transform.position));

        bool groundInFront = false;

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].transform.gameObject.tag == "Ground")
                groundInFront = true;
            else if (m_alerted > 0 && hits[i].transform.gameObject.tag == "Wall" && m_ground)
                Jump(5.0f);
        }

        if (m_alerted == 0 && !groundInFront)
            ChangeDirection();
        else if (m_alerted > 0 && !groundInFront && m_ground)
            Jump(5.0f);
    }

    override protected void OnCollisionEnter2D(Collision2D collision)
    {
        base.OnCollisionEnter2D(collision);

        if (collision.gameObject.tag == "Player")
            m_alerted = m_alertTime;
    }
}
