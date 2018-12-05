using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NameServer : PanelScript
{
    public Server m_myServer;
    public GameObject[] m_servers;
    public InputField m_nameServerinField;
    public InputField m_myNameInField;

    // Use this for initialization
    override protected void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    override protected void Update()
    {
        base.Update();
    }

    public void Confirm()
    {
        if (m_nameServerinField.text == "")
            return;

        for (int i = 0; i < m_servers.Length; i++)
            if (m_servers[i].GetComponentsInChildren<Button>()[1].GetComponent<Image>().color == Color.blue)
            {
                OnRightClick();
                return;
            }

        for (int i = 0; i < m_servers.Length; i++)
            if (m_servers[i].GetComponentInChildren<Text>().text == "EMPTY SERVER")
            {
                OccupyServerSlot(i);
                break;
            }

        m_myNameInField.interactable = false;
        m_myServer.Connect();
        OnRightClick();
    }

    public void JoinServer()
    {
        m_myNameInField.interactable = false;
    }

    public void DisconnectServer()
    {
        m_myNameInField.interactable = true;
    }

    private void OccupyServerSlot(int _index)
    {
        m_servers[_index].GetComponentInChildren<Text>().text = m_nameServerinField.text;

        Button button = m_servers[_index].GetComponentsInChildren<Button>()[1];

        button.GetComponent<Image>().color = Color.blue;
    }
}
