using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBase : MonoBehaviour {

    public ObjectText m_text;
    public GameObject m_character;

    public float m_attackTime = 1.5f;
    public float m_knockback = 3.5f;
    public int m_damage = 10;
    private List<GameObject> m_hit;

    // Use this for initialization
    void Start ()
    {
        m_hit = new List<GameObject>();
	}
	
	// Update is called once per frame
	void Update ()
    {
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.GetComponent<CharacterBase>() != null && !m_hit.Contains(collider.gameObject))
        {
            m_hit.Add(collider.gameObject);

            ObjectText text;
            CharacterBase defender = collider.gameObject.GetComponent<CharacterBase>();
            text = Instantiate(m_text, new Vector3(collider.transform.position.x, collider.transform.position.y + 1.0f), Quaternion.identity);

            text.ShowDamage(m_damage);
            DealDamage(collider.gameObject);

            if (collider.gameObject.GetComponent<AIBase>() != null)
            {
                AIBase AI = collider.gameObject.GetComponent<AIBase>();
                AI.m_alerted = AI.m_alertTime;

                if (AI.m_coolDown <= 0)
                    AI.m_coolDown = 1.5f;
            }
        }
    }

    public void DealDamage(GameObject _defender)
    {
        CharacterBase defender = _defender.GetComponent<CharacterBase>();

        defender.m_health -= m_damage;

        Knockback(_defender);

        if (defender.m_health <= 0)
        {
            _defender.transform.SetPositionAndRotation(transform.position, Quaternion.Euler(0, 0, 90));
            defender.m_isAlive = false;
        }
    }

    protected void Knockback(GameObject _defender)
    {
        float x = m_knockback;
        float y = 2.5f;
        if (transform.position.x > _defender.transform.position.x)
            x *= -1;

        _defender.GetComponent<Rigidbody2D>().AddForce(new Vector2(x, y), ForceMode2D.Impulse);
    }

    public void ResetWeapon()
    {
        gameObject.SetActive(false);
        m_hit.Clear();
    }
}
