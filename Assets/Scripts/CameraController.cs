using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    public GameObject[] m_targets;
    public float m_camZoom;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        FollowTargets();
	}

    private void FollowTargets()
    {
        if (m_targets.Length == 1)
        {
            transform.position = new Vector3(m_targets[0].transform.position.x, m_targets[0].transform.position.y + 1, transform.position.z);
        }
        else
        {
            AverageDistance();   
        }
    }

    private void AverageDistance()
    {
        float greatestX = -5000.0f;
        float greatestY = -5000.0f;
        float smallestX = 5000.0f;
        float smallestY = 5000.0f;
        float camZoom = -6;
        float zoomMod = -0.8f;
        float x = 0;
        float y = 0;

        for (int i = 0; i < m_targets.Length; i++)
        {
            x += m_targets[i].transform.position.x;
            y += m_targets[i].transform.position.y;

            if (m_targets[i].transform.position.x < smallestX)
                smallestX = m_targets[i].transform.position.x;
            if (m_targets[i].transform.position.x > greatestX)
                greatestX = m_targets[i].transform.position.x;

            if (m_targets[i].transform.position.y < smallestY)
                smallestY = m_targets[i].transform.position.y;
            if (m_targets[i].transform.position.y > greatestY)
                greatestY = m_targets[i].transform.position.y;
        }

        if (Mathf.Abs(smallestX) + Mathf.Abs(greatestX) > Mathf.Abs(smallestY) + Mathf.Abs(greatestY))
            camZoom = zoomMod * (Mathf.Abs(smallestX) + Mathf.Abs(greatestX)) - 1;
        else
            camZoom = zoomMod * (Mathf.Abs(smallestY) + Mathf.Abs(greatestY)) - 1;

        if (camZoom > -6)
            camZoom = -6;

        x /= m_targets.Length;
        y /= m_targets.Length;

        m_camZoom = camZoom;

        transform.position = new Vector3(x, y + 1, camZoom);
    }
}
