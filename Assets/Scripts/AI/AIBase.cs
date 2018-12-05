using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBase : CharacterBase
{
    public float v_blockDistance = 1.6f; // The space between each block

    public enum sight { WALL, GROUND, FALL, HIGH_JUMP, HIGHER_JUMP, FAR_HIGH_JUMP, FAR_HIGHER_JUMP,
        LONG_JUMP, LONGER_JUMP, EVEN_LONGER_JUMP, STRAIGHT_JUMP, HEAD_SPACE, TOT }

    public GameObject[] m_players;
    public GameObject m_target;

    public float m_patrolSpeed;
    public float m_alerted;

    public float m_alertTime = 5.0f;
    public float m_attackDistance = 2.0f; // How close the player needs to be for an attack
    public float m_sightDistance = 7.0f; // How far an enemy can see
    private float m_lastX;
    public bool m_pause;
    public bool m_platformBelow = false;

    // Use this for initialization
    new protected void Start()
    {
        base.Start();

        m_players = GameObject.Find("Main Camera").GetComponent<CameraController>().m_targets;

        m_alerted = 0;
        m_pause = false;
    }

    // Update is called once per frame
    new protected void Update()
    {
        base.Update();

        float yVel = m_rb.velocity.y;
        if (m_coolDown > 0 || yVel != 0 || m_pause)
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

        Vector3 direction = new Vector3(transform.position.x + v_blockDistance, transform.position.y - 1, transform.position.z);

        if (!CheckForPlayer(ref direction))
            Patrol(ref direction);
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
        {
            m_target = null;
            return false;
        }

        MoveTowards(closestPlayer, ref _direction);
        Debug.DrawRay(transform.position, (closestPlayer.transform.position - transform.position));

        m_target = closestPlayer;
        return true;
    }

    protected void MoveTowards(GameObject _player, ref Vector3 _direction)
    {
        float speed = m_moveSpeed * Time.deltaTime;

        if (_player.transform.position.x > transform.position.x)
        {
            m_rb.AddForce(new Vector2(speed, 0), ForceMode2D.Impulse);
            //transform.SetPositionAndRotation(new Vector3(transform.position.x + speed, transform.position.y, transform.position.z), transform.rotation);

            if (m_coolDown == 0)
                transform.SetPositionAndRotation(transform.position, Quaternion.Euler(0, 0, 0));

            _direction = new Vector3(transform.position.x + 2, transform.position.y - 1.0f, transform.position.z);
        }

        if (_player.transform.position.x < transform.position.x)
        {
            m_rb.AddForce(new Vector2(-speed, 0), ForceMode2D.Impulse);
            //transform.SetPositionAndRotation(new Vector3(transform.position.x - speed, transform.position.y, transform.position.z), transform.rotation);

            if (m_coolDown == 0)
                transform.SetPositionAndRotation(transform.position, Quaternion.Euler(0, 180, 0));

            _direction = new Vector3(transform.position.x - 2, transform.position.y - 1.0f, transform.position.z);
        }

        if (Vector3.Distance(_player.transform.position, transform.position) < m_attackDistance)
            Attack(_player);
    }

    virtual protected void Attack(GameObject _player)
    {
        m_weapon.gameObject.SetActive(true);
        m_coolDown = m_weapon.GetComponent<WeaponBase>().m_attackTime;
    }

    protected void Patrol(ref Vector3 _direction)
    {
        float speed;
        
        if (m_alerted > 0)
            speed = m_moveSpeed;
        else
            speed = m_patrolSpeed;

        speed *= Time.deltaTime;

        if (transform.eulerAngles.y == 0)
        {
            m_rb.AddForce(new Vector2(speed, 0), ForceMode2D.Impulse);
            //transform.SetPositionAndRotation(new Vector3(transform.position.x + speed, transform.position.y, transform.position.z), transform.rotation);
            _direction.x = transform.position.x + v_blockDistance;
        }
        else if (transform.eulerAngles.y == 180)
        {
            m_rb.AddForce(new Vector2(-speed, 0), ForceMode2D.Impulse);
            //transform.SetPositionAndRotation(new Vector3(transform.position.x - speed, transform.position.y, transform.position.z), transform.rotation);
            _direction.x = transform.position.x - v_blockDistance;
        }
    }

    protected void MovementLogic()
    {
        // FOR DEBUGGING
        //if (CheckFor(sight.FAR_HIGHER_JUMP))
        //    Jump(2.0f, 15.0f);
        //return;
    
        int[] possibleDirections = new int[(int)sight.TOT + 1];
        int total = 0;

        if (CheckFor(sight.WALL))
        {
            possibleDirections[(int)sight.WALL] = 2;
            total += 2;

            if (CheckFor(sight.HIGH_JUMP))
            {
                possibleDirections[(int)sight.HIGH_JUMP] = 10;
                total += 10;
            }
            if (CheckFor(sight.HIGHER_JUMP))
            {
                possibleDirections[(int)sight.HIGHER_JUMP] = 10;
                total += 10;
            }
        }
        else
        {
            if (CheckFor(sight.GROUND))
            {
                possibleDirections[(int)sight.GROUND] = 10;
                total += 10;


                if (CheckFor(sight.HIGHER_JUMP))
                {
                    possibleDirections[(int)sight.HIGHER_JUMP] = 10;
                    total += 10;
                }
            }
            else
            {
                if (CheckFor(sight.FALL))
                {
                    possibleDirections[(int)sight.GROUND] = 2;
                    total += 2;
                }

                if (CheckFor(sight.LONG_JUMP))
                {
                    possibleDirections[(int)sight.LONG_JUMP] = 10;
                    total += 10;
                }
                else if (CheckFor(sight.LONGER_JUMP))
                {
                    possibleDirections[(int)sight.LONGER_JUMP] = 10;
                    total += 10;
                }
                else if (CheckFor(sight.EVEN_LONGER_JUMP))
                {
                    possibleDirections[(int)sight.EVEN_LONGER_JUMP] = 10;
                    total += 10;
                }

                if (CheckFor(sight.HIGHER_JUMP))
                {
                    possibleDirections[(int)sight.HIGHER_JUMP] = 10;
                    total += 10;
                }
                else if (CheckFor(sight.FAR_HIGH_JUMP))
                {
                    possibleDirections[(int)sight.FAR_HIGH_JUMP] = 10;
                    total += 10;
                }
                else if (CheckFor(sight.FAR_HIGHER_JUMP))
                {
                    possibleDirections[(int)sight.FAR_HIGHER_JUMP] = 10;
                    total += 10;
                }
            }
        }

        if (m_platformBelow && !m_target || m_platformBelow && m_target && 
            m_target.transform.position.y < transform.position.y - v_blockDistance)
        {
            possibleDirections[(int)sight.FALL] = 1;
            total += 1;
        }

        // Chance of doing nothing and then turning around
        possibleDirections[(int)sight.TOT] = 1;
        total += 1;

        SelectJump(total, possibleDirections);
    }

    private void SelectJump(int _total, int[] _possibleDirections)
    {
        // Add all possibilities and weight them

        int resultNum = Random.Range(0, _total);
        int currPlace = 0;
        sight resultDirection = sight.TOT;

        for (int i = 0; i < _possibleDirections.Length; i++)
        {
            if (_possibleDirections[i] == 0)
                continue;

            currPlace += _possibleDirections[i];

            if (resultNum <= currPlace)
            {
                resultDirection = (sight)i;
                break;
            }
        }

        if (resultDirection != sight.WALL && resultDirection != sight.GROUND)
            m_rb.velocity = Vector2.zero;

        switch (resultDirection)
        {
            case sight.WALL: // Wall in front.
                ChangeDirection();
                break;
            case sight.GROUND: // Keep walking
                break;
            case sight.FALL:
                {
                    transform.SetPositionAndRotation(new Vector3(transform.position.x, GetComponent<CapsuleCollider2D>().bounds.min.y + 0.3f, transform.position.z), transform.rotation);
                    m_rb.velocity = new Vector2(m_rb.velocity.x, -4f);
                }
                break;
            case sight.HIGH_JUMP: // Jump 1x1.
                Jump(2.0f, 11.0f);
                break;
            case sight.HIGHER_JUMP: // Jump 1x2.
                Jump(1.5f, 16f);
                break;
            case sight.FAR_HIGH_JUMP: // Jump 2x1
                Jump(5.0f, 11.0f);
                break;
            case sight.FAR_HIGHER_JUMP: // Jump 2x2
                Jump(4.0f, 15.0f);
                break;
            case sight.LONG_JUMP: // Jump 2x0.
                Jump(4.0f, 10.0f);
                break;
            case sight.LONGER_JUMP: // Jump 3x0.
                Jump(8.0f, 10.0f);
                break;
            case sight.EVEN_LONGER_JUMP: // Jump 4x0
                Jump(10.0f, 10.0f);
                break;
            case sight.STRAIGHT_JUMP: // Jump 0x1.
                Jump(0.0f, 15.0f);
                break;
            case sight.TOT:
                StartCoroutine(BreakTime());
                break;
            default:
                break;
        }
    }

    // Raycasting
    protected bool CheckFor(sight _mode) 
    {
        CapsuleCollider2D col = GetComponent<CapsuleCollider2D>();
        Vector3 start = new Vector3(col.bounds.max.x + 0.5f, transform.position.y, transform.position.z);
        Vector3 end = new Vector3(col.bounds.center.x + v_blockDistance, transform.position.y, transform.position.z);

        if (transform.eulerAngles.y == 180)
        {
            start.x = col.bounds.min.x - 0.5f;
            end.x = col.bounds.center.x - v_blockDistance;
        }

        switch (_mode)
        {
            case sight.GROUND:
                if (transform.eulerAngles.y == 0)
                {
                    start.x = col.bounds.max.x + v_blockDistance - .1f;
                    end.x = col.bounds.max.x + v_blockDistance + .1f;
                }
                else if (transform.eulerAngles.y == 180)
                {
                    start.x = col.bounds.min.x - v_blockDistance - .1f;
                    end.x = col.bounds.min.x - v_blockDistance + .1f;
                }
                end.y -= 1;
                start.y = end.y;
                break;
            case sight.FALL:
                start.x = end.x;
                end.y -= v_blockDistance * 4;
                break;
            case sight.HIGH_JUMP: // Jump 1x1
                start.x = end.x;
                start.y += v_blockDistance;
                break;
            case sight.HIGHER_JUMP: // Jump 1x2
                start.x = end.x;
                start.y += v_blockDistance * 2;
                end.y = start.y - v_blockDistance;
                break;
            case sight.FAR_HIGH_JUMP:
                start.y += v_blockDistance;
                if (transform.eulerAngles.y == 0)
                    end.x += v_blockDistance;
                else if (transform.eulerAngles.y == 180)
                    end.x -= v_blockDistance;
                start.x = end.x;
                break;
            case sight.FAR_HIGHER_JUMP:
                start.y += v_blockDistance * 2;
                end.y += v_blockDistance;
                if (transform.eulerAngles.y == 0)
                    end.x += v_blockDistance;
                else if (transform.eulerAngles.y == 180)
                    end.x -= v_blockDistance;
                start.x = end.x;
                break;
            case sight.LONG_JUMP: // Jump 2x0
                end.y -= v_blockDistance;
                end.x = start.x = transform.position.x + v_blockDistance * 2;
                if (transform.eulerAngles.y == 180)
                    end.x = start.x = transform.position.x - v_blockDistance * 2;
                break;
            case sight.LONGER_JUMP: // Jump 3x0
                start.y += v_blockDistance;
                end.y -= v_blockDistance;
                end.x = start.x = transform.position.x + v_blockDistance * 3;
                if (transform.eulerAngles.y == 180)
                    end.x = start.x = transform.position.x - v_blockDistance * 3;
                break;
            case sight.EVEN_LONGER_JUMP: // Jump 4x0
                end.y -= v_blockDistance;
                end.x = start.x = transform.position.x + v_blockDistance * 4;
                if (transform.eulerAngles.y == 180)
                    end.x = start.x = transform.position.x - v_blockDistance * 4;
                break;
            default:
                break;
        }

        RaycastHit2D hit = Physics2D.Linecast(start, end, 1);
        Debug.DrawLine(start, end);

        if (hit.collider == null || hit.transform.gameObject.name != "RectangleObject" && hit.transform.gameObject.tag != "Platform")
            return false;

        // Check if there is something over your head before you try to jump
        if (_mode == sight.HIGH_JUMP)
        {
            Vector3 startTest = new Vector3(transform.position.x, col.bounds.max.y + 0.5f, transform.position.z);
            Vector3 endTest = new Vector3(transform.position.x, transform.position.y + v_blockDistance, transform.position.z);

            RaycastHit2D headHit = Physics2D.Linecast(startTest, endTest, 1);
            Debug.DrawLine(startTest, endTest);

            if (headHit && headHit.transform.gameObject.tag != "Platform")
                return false;
        }
        else if (_mode == sight.HIGHER_JUMP)
        {
            Vector3 startTest = new Vector3(transform.position.x, transform.position.y + v_blockDistance, transform.position.z);
            Vector3 endTest = new Vector3(transform.position.x, transform.position.y + v_blockDistance * 2, transform.position.z);

            RaycastHit2D headHit = Physics2D.Linecast(startTest, endTest, 1);
            Debug.DrawLine(startTest, endTest);

            if (headHit && headHit.transform.gameObject.tag != "Platform")
                return false;
        }

        switch (_mode)
        {
            case sight.WALL: // If we are looking for a wall, look straight ahead
                if (m_rb.velocity.y == 0)
                    return true;

                ChangeDirection();
                break;
            case sight.GROUND: // If we are checking the ground, look down
            case sight.FALL:
                return true;
            case sight.HIGH_JUMP: // if we are checking for jumpable terrain, look up
            case sight.HIGHER_JUMP:
            case sight.FAR_HIGH_JUMP:
            case sight.FAR_HIGHER_JUMP:
            case sight.LONG_JUMP:
            case sight.LONGER_JUMP:
            case sight.EVEN_LONGER_JUMP:
                if (hit.point.y <= start.y - .2f)
                    return true;
                return false;
            default:
                break;
        }

        return false;
    }

    protected IEnumerator BreakTime()
    {
        m_pause = true;
        yield return new WaitForSeconds(Random.Range(2, 4));
        m_pause = false;
        ChangeDirection();
    }

    protected void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            m_alerted = m_alertTime;
            GetComponent<SpriteRenderer>().color = Color.red;
        }
        else if (collision.gameObject.tag == "Enemy")
            ChangeDirection();
    }

    protected void OnCollisionStay2D(Collision2D collision)
    {
        Bounds bounds = GetComponent<Collider2D>().bounds;
        if (collision.gameObject.name == "RectangleObject")
        {
            Vector3 center = collision.collider.bounds.center;

            if (m_lastX < center.x && transform.position.x > center.x ||
                m_lastX > center.x && transform.position.x < center.x)
                MovementLogic();

            m_lastX = transform.position.x;
        }
        if (collision.gameObject.name == "RectangleObject" &&
            collision.collider.bounds.center.y > bounds.min.y && // Floor check
            collision.collider.bounds.center.y < bounds.max.y) // Ceiling check
        {
            if (transform.position.x > collision.transform.position.x)
                transform.SetPositionAndRotation(transform.position, Quaternion.Euler(0, 0, 0));
            else if (transform.position.x < collision.transform.position.x)
                transform.SetPositionAndRotation(transform.position, Quaternion.Euler(0, 180, 0));
        }
    }

    override protected void OnTriggerExit2D(Collider2D collision)
    {
        base.OnTriggerExit2D(collision);

        if (collision.gameObject.tag == "Platform")
        {
            float colMaxYBounds = collision.GetComponent<BoxCollider2D>().bounds.max.y;

            if (colMaxYBounds < transform.position.y)
                m_platformBelow = false;
        }
    }

    override protected void OnTriggerStay2D(Collider2D collision)
    {
        base.OnTriggerStay2D(collision);

        Vector3 center = collision.bounds.center;

        if (collision.gameObject.tag == "Platform")
        {
            float colMaxYBounds = collision.GetComponent<BoxCollider2D>().bounds.max.y;

            if (colMaxYBounds < transform.position.y)
                m_platformBelow = true;
            else
                m_platformBelow = false;
        }
            
        if (collision.gameObject.tag == "Platform" && center.y < transform.position.y && m_lastX < center.x && transform.position.x > center.x ||
            collision.gameObject.tag == "Platform" && center.y < transform.position.y && m_lastX > center.x && transform.position.x < center.x)
        {
            MovementLogic();
            m_lastX = transform.position.x;
        }
    }
}
