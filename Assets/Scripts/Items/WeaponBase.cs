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
            text = Instantiate(m_text, new Vector3(collider.transform.position.x, collider.transform.position.y + 1.0f), Quaternion.identity);

            text.ShowDamage(m_damage);
            collider.gameObject.GetComponent<CharacterBase>().Hit(m_damage, m_knockback);

            if (collider.gameObject.GetComponent<AIBase>() != null)
            {
                AIBase AI = collider.gameObject.GetComponent<AIBase>();
                AI.m_alerted = AI.m_alertTime;
            }
        }
    }

    public void ResetWeapon()
    {
        gameObject.SetActive(false);

        if (m_hit != null)
            m_hit.Clear();
    }
}
