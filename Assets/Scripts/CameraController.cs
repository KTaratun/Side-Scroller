using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    public GameObject[] m_targets;

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
            transform.position = new Vector3(m_targets[0].transform.position.x, m_targets[0].transform.position.y + 1, transform.position.z);
        else
            AverageDistance();   
    }

    private void AverageDistance()
    {
        float greatestX = -5000.0f;
        float greatestY = -5000.0f;
        float smallestX = 5000.0f;
        float smallestY = 5000.0f;
        float camZoom = -6;
        float xMod = -0.5f;
        float yMod = -1.0f;
        float x = 0;
        float y = 0;

        // Find max and min bounds of players
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

        float xDis = xMod * ((-1 * smallestX) + greatestX) - 4;
        float yDis = yMod * ((-1 * smallestY) + greatestY) - 3;

        // Check if screen needs to be wider for x or y
        if (xDis < yDis)
            camZoom = xDis;
        else
            camZoom = yDis;

        // Max zoom
        if (camZoom > -6)
            camZoom = -6;

        // Center screen
        x /= m_targets.Length;
        y /= m_targets.Length;

        transform.position = new Vector3(x, y + 1, camZoom);
    }
}
