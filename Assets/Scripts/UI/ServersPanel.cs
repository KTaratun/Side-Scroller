using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ServersPanel : PanelScript {

    public GameObject[] m_servers;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Confirm(GameObject _button)
    {
        // Join Server

        for (int i = 0; i < m_servers.Length; i++)
        {
            if (m_servers[i].GetComponentsInChildren<Button>()[1].GetComponent<Image>().color == Color.green)
                m_servers[i].GetComponentsInChildren<Button>()[1].GetComponent<Image>().color = Color.red;
            else if (m_servers[i].GetComponentsInChildren<Button>()[1].GetComponent<Image>().color == Color.blue)
            {
                m_servers[i].GetComponentsInChildren<Button>()[1].GetComponent<Image>().color = Color.white;
                m_servers[i].GetComponentInChildren<Text>().text = "EMPTY SERVER";
            }
        }
    }
}
