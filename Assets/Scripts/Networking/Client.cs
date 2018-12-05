using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Client : MonoBehaviour
{
    public InputField m_myName;
    public Text m_playersInServer;

    private const int MAX_CONNECTION = 100;

    private int m_port = 5357;

    private int m_hostId;

    public int m_reliableChannel;
    public int m_unreliableChannel;

    private int m_ourClientId;
    public int m_connectionId;

    private float m_connectionTime;
    public bool m_isConnected = false;

    private byte error;

    // Use this for initialization
    void Start()
    {
        //Connect();
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_isConnected)
            return;

        int recHostId;
        int connectionId;
        int channelId;
        byte[] recBuffer = new byte[1024];
        int bufferSize = 1024;
        int dataSize;
        byte error;
        NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);
        switch (recData)
        {
            case NetworkEventType.DataEvent:
                string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                Debug.Log("Receiving: " + msg);
                string[] splitData = msg.Split('~');

                switch (splitData[0])
                {
                    case "ASKNAME":
                        OnAskName(splitData);
                        break;
                    case "READY":
                        break;
                    case "CON":
                        
                        break;
                    case "DC":
                        PlayerDisconnected(int.Parse(splitData[1]));
                        break;

                    default:
                        Debug.Log("Invalid Message: " + msg);
                        break;
                }
                break;
        }
    }

    private void OnAskName(string[] _data)
    {
        // Set this client's ID
        m_ourClientId = int.Parse(_data[1]);

        // Send our name to the server
        Send("NAMEIS~" + m_myName.text, m_reliableChannel);

        Text[] textChildren = m_playersInServer.GetComponentsInChildren<Text>();

        // Create all the other players
        for (int i = 2; i < _data.Length; i++)
        {
            string[] d = _data[i].Split('%');

            if (i == 3)
                textChildren[i - 1].text = m_myName.text;
            else
                textChildren[i - 1].text = d[0];
        }
    }

    private void PlayerDisconnected(int _conId)
    {
        //Destroy
        //m_players.Remove(_conId);
    }

    public void Send(string _message, int _channelId)
    {
        Debug.Log("Sending: " + _message);
        byte[] msg = Encoding.Unicode.GetBytes(_message);
        NetworkTransport.Send(m_hostId, m_connectionId, _channelId, msg, _message.Length * sizeof(char), out error);
    }

    public void Connect()
    {
        NetworkTransport.Init();
        ConnectionConfig cc = new ConnectionConfig();

        m_reliableChannel = cc.AddChannel(QosType.Reliable);
        m_unreliableChannel = cc.AddChannel(QosType.Unreliable);

        HostTopology topo = new HostTopology(cc, MAX_CONNECTION);

        m_hostId = NetworkTransport.AddHost(topo, 0);
        m_connectionId = NetworkTransport.Connect(m_hostId, "127.0.0.1", m_port, 0, out error);

        m_connectionTime = Time.time;
        m_isConnected = true;
    }
}
