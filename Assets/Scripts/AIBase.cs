using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBase : CharacterBase
{

    public enum sight { WALL, GROUND, FALL, HIGH_JUMP, FAR_HIGH_JUMP, LONG_JUMP, LONGER_JUMP }
    public GameObject[] m_players;
    public GameObject DEBUG_TOOL;
    public List<GameObject> m_currDebugTools;

    public float m_patrolSpeed;
    public float m_alerted;

    public float m_alertTime = 5.0f;
    public float m_attackDistance = 2.0f;
    public float m_sightDistance = 7.0f;
    public float m_environmentalSight = 1.0f;

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

        if (m_coolDown > 0 || GetComponent<Rigidbody2D>().velocity.y != 0)
            return;

        if (m_alerted > 0)
        {
            m_alerted -= Time.deltaTime;
            if (m_alerted < 0)
            {
                m_alerted = 0;
                GetComponent<SpriteRenderer>().color = Color.blue;
            }
        }

        Vector3 direction = new Vector3(transform.position.x + m_environmentalSight, transform.position.y - 1, transform.position.z);

        if (!CheckForPlayer(ref direction))
            Patrol(ref direction);

        CheckSight(sight.LONGER_JUMP);
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
                    transform.eulerAngles.y == 180 && m_players[i].transform.position.x < transform.position.x)
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
                            GetComponent<SpriteRenderer>().color = Color.red;
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
            _direction.x = transform.position.x + m_environmentalSight;
        }
        else if (transform.eulerAngles.y == 180)
        {
            transform.SetPositionAndRotation(new Vector3(transform.position.x - speed, transform.position.y, transform.position.z), transform.rotation);
            _direction.x = transform.position.x - m_environmentalSight;
        }
    }

    protected bool CheckSight(sight _mode)
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        Vector3 start = new Vector3(col.bounds.max.x + 0.1f, transform.position.y, transform.position.z);
        Vector3 end = new Vector3(transform.position.x + m_environmentalSight, transform.position.y, transform.position.z);

        if (transform.eulerAngles.y == 180)
        {
            start.x = col.bounds.min.x - 0.1f;
            end.x = transform.position.x - m_environmentalSight;
        }

        switch (_mode)
        {
            case sight.GROUND:
                end.y -= 1;
                start.y = end.y;
                break;
            case sight.FALL:
                start.x = end.x;
                end.y -= 6;
                break;
            case sight.HIGH_JUMP:
                end.y += 1.5f;
                start.y = end.y;
                break;
            case sight.FAR_HIGH_JUMP:
                end.x += 1.5f;
                end.y += 1.5f;
                start.y = end.y;
                break;
            case sight.LONG_JUMP:
                end.y -= 4;
                end.x = start.x = transform.position.x + 3;
                if (transform.eulerAngles.y == 180)
                    end.x = start.x = transform.position.x - 3;
                break;
            case sight.LONGER_JUMP:
                start.y += 1.0f;
                end.y -= 2;
                end.x = start.x = transform.position.x + 5;
                if (transform.eulerAngles.y == 180)
                    end.x = start.x = transform.position.x - 5;
                break;
            default:
                break;
        }

        RaycastHit2D hit = Physics2D.Linecast(start, end, 1);
        Debug.DrawLine(start, end);

        if (hit.collider == null || hit.transform.gameObject.name != "Collision" && hit.transform.gameObject.name != "Platform")
        {
            if (_mode == sight.WALL && !CheckSight(sight.GROUND))
            {
                if (CheckSight(sight.LONG_JUMP) || CheckSight(sight.LONGER_JUMP))
                    return true;

                if (!CheckSight(sight.FALL))
                    ChangeDirection();
            }
            else if (_mode == sight.HIGH_JUMP && CheckSight(sight.FAR_HIGH_JUMP))
                return true;

            return false;
        }

        switch (_mode)
        {
            case sight.WALL: // If we are looking for a wall, look straight ahead
                if (GetComponent<Rigidbody2D>().velocity.y == 0 && !CheckSight(sight.HIGH_JUMP))
                    return true;

                ChangeDirection();
                break;
            case sight.GROUND: // If we are checking the ground, look down
            case sight.FALL:
            case sight.HIGH_JUMP: // if we are checking for jumpable terrain, look up
                Jump(2.0f, 13.0f);
                return true;
            case sight.FAR_HIGH_JUMP:
                Jump(5.0f, 15.0f);
                return true;
            case sight.LONG_JUMP:
                if (hit.point.y < transform.position.y)
                    Jump(4.0f, 9.0f);
                else
                    return false;

                return true;
            case sight.LONGER_JUMP:
                if (hit.point.y == start.y)
                    return false;
                else if (hit.point.y > transform.position.y)
                    Jump(8.0f, 14.0f);
                else if (hit.point.y < transform.position.y)
                    Jump(7.0f, 10.0f);
                
                return true;
            default:
                break;
        }

        return false;
    }

    override protected void OnCollisionEnter2D(Collision2D collision)
    {
        base.OnCollisionEnter2D(collision);

        if (collision.gameObject.name == "Collision")
        {
            BoxCollider2D col = GetComponent<BoxCollider2D>();

            for (int i = 0; i < collision.contacts.Length; i++)
            {
                if (collision.contacts[i].point.y <= col.bounds.min.y)
                    Debug.Log("GROOOOUND HOOOO");
            }
        }

        if (collision.gameObject.tag == "Player")
        {
            m_alerted = m_alertTime;
            GetComponent<SpriteRenderer>().color = Color.red;
        }
    }

    override protected void OnTriggerStay2D(Collider2D collision)
    {
        base.OnTriggerStay2D(collision);

        if (collision.gameObject.name == "platform")
        {
            float colMaxYBounds = collision.GetComponent<PolygonCollider2D>().bounds.max.y;

            if (colMaxYBounds > transform.position.y)
                Jump(0, 8.0f);
        }
    }
}
