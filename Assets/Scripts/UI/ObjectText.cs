using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectText : MonoBehaviour {

    TextMesh m_text;
    public float m_currTime;
    public float m_textTime = 2.0f;
    public float m_upSpeed = 0.0001f;

	// Use this for initialization
	void Start ()
    {
    }
	
	// Update is called once per frame
	void Update ()
    {
        m_currTime -= Time.deltaTime;
        m_text.color = new Vector4(1, 1, 1, m_currTime / m_textTime);
        transform.SetPositionAndRotation(new Vector3(transform.position.x, transform.position.y + m_upSpeed, transform.position.z), transform.rotation);

        if (m_currTime <= 0)
            Destroy(gameObject);
	}

    public void ShowDamage(int _damage)
    {
        m_text = GetComponent<TextMesh>();
        m_text.text = _damage.ToString();
        m_text.color = Color.white;
        m_currTime = m_textTime;
    }
}
