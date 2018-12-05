using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PanelScript : MonoBehaviour {

    public enum HUDPan { CHAR_PORT, ACT_PAN, MOV_PASS, ENG_PAN, STS_BUTTS, PARA_PAN}
    public enum dir { UP, DOWN, LEFT, RIGHT, NULL }

    static public Color b_isFree = new Color(.5f, 1, .5f, 1);
    static public Color b_isDisallowed = new Color(1, .5f, .5f, 1);
    static public Color b_isHalf = new Color(1, 1, .5f, 1);
    static public Color b_isSpecial = new Color(1, .2f, 1, 1);

    // General
    public bool m_inView; // This starts the sliding process of the panel
    public dir m_direction;
    public float m_slideSpeed;
    public int m_inBoundryDis;
    public int m_outBoundryDis;
    
    // Children
    public PanelScript[] m_panels;
    public Text[] m_text;
    public Image[] m_images;
    public Button[] m_buttons;

    // Static
    static public List<PanelScript> m_history;
    static public List<PanelScript> m_allPanels;
    static public bool m_locked;
    static public PanelScript m_confirmPanel;

    // References
    public GameObject m_main;
    public AudioSource m_audio;

    // Use this for initialization
    virtual protected void Start()
    {
        m_audio = gameObject.AddComponent<AudioSource>();
        m_locked = false;

        if (m_slideSpeed == 0)
            m_slideSpeed = 30.0f;
        if (GameObject.Find("Confirmation Panel"))
            m_confirmPanel = GameObject.Find("Confirmation Panel").GetComponent<PanelScript>();

        if (m_history == null)
            m_history = new List<PanelScript>();

        m_buttons = GetComponentsInChildren<Button>();

        if (m_inBoundryDis == 0)
        {
            if (m_direction == dir.UP)
                m_inBoundryDis = 540;
            else if (m_direction == dir.RIGHT)
                m_inBoundryDis = 1150;
            else if (m_direction == dir.LEFT)
                m_inBoundryDis = 130;
            else if (m_direction == dir.DOWN)
                m_inBoundryDis = 140;
        }

        if (m_outBoundryDis == 0)
        {
            if (m_direction == dir.UP)
                m_outBoundryDis = 950;
            else if (m_direction == dir.RIGHT)
                m_outBoundryDis = 1450;
            else if (m_direction == dir.LEFT)
                m_outBoundryDis = -200;
            else if (m_direction == dir.DOWN)
                m_outBoundryDis = -120;
        }

        if (GetComponentsInChildren<Text>().Length > 0)
            m_text = GetComponentsInChildren<Text>();
    }

    // Update is called once per frame
    virtual protected void Update()
    {
        RectTransform recTrans = GetComponent<RectTransform>();
        // REFACTOR
        //if (Input.GetMouseButtonDown(1) && name == "Confirmation Panel" && m_history.Count > 0)
            //if (GetRecentHistory().name == "Confirmation Panel" || GetRecentHistory().name != "Confirmation Panel" && !m_locked)
        if (Input.GetMouseButtonDown(1))
            OnRightClick();

        Slide();
    }

    public void OnRightClick()
    {
        if (m_history.Count > 0)
        {
            RemoveFromHistory("");
            if (m_history.Count > 0)
                m_history[m_history.Count - 1].m_inView = true;
        }
    }

    static public void MenuPanelInit(string _canvasName, GameObject _main)
    {
        if (m_history != null)
            m_history.Clear();

        Canvas can = GameObject.Find(_canvasName).GetComponent<Canvas>();
        PanelScript[] pans = can.GetComponentsInChildren<PanelScript>();
        m_allPanels = new List<PanelScript>();

        for (int i = 0; i < pans.Length; i++)
        {
            if (pans[i].transform.parent.name == _canvasName)
            {
                pans[i].m_main = _main;
                PanelScript[] children = pans[i].GetComponentsInChildren<PanelScript>();
                for (int j = 0; j < children.Length; j++)
                    children[j].m_main = _main;

                m_allPanels.Add(pans[i]);
            }
        }
    }

    // General Panel
    public void PopulatePanel()
    {
        if (m_direction == dir.UP && !m_inView)
        {
            if (m_history.Count > 0 && name != "Confirmation Panel")
                m_history[m_history.Count - 1].ClosePanel();

            m_history.Add(this);
        }

        if (m_direction != dir.NULL)
            m_inView = true;

        if (name == "Confirmation Panel")
        {
            if ((int)Input.mousePosition.x < 120)
                transform.SetPositionAndRotation(new Vector3(120, 1000, transform.position.z), transform.rotation);
            else if ((int)Input.mousePosition.x > 1160)
                transform.SetPositionAndRotation(new Vector3(1160, 1000, transform.position.z), transform.rotation);
            else
                transform.SetPositionAndRotation(new Vector3(Input.mousePosition.x, 1000, transform.position.z), transform.rotation);


            if ((int)Input.mousePosition.y > 550)
                m_inBoundryDis = 550 + 80;
            else
                m_inBoundryDis = (int)Input.mousePosition.y + 80;
        }
    }

    public void ClosePanel()
    {
        m_inView = false;

        if (name == "ActionViewer Panel")
            m_panels[3].m_inView = false;
        if (name == "StatusViewer Panel")
            m_panels[0].m_inView = false;
    }

    private void Slide()
    {
        bool isOpen = false;

        if (name == "ActionView Slide")
        {
            PanelScript parent = GetPanel("ActionViewer Panel");
            m_inBoundryDis = (int)parent.transform.position.y;
            m_outBoundryDis = (int)parent.transform.position.y - 50;
        }
        else if (tag == "Action Button" && m_panels[0])
        {
            PanelScript parent = m_panels[0];
            if (parent.m_panels[0].name == "HUD Panel LEFT")
                m_inBoundryDis = (int)parent.transform.position.x + 10;
            else if (parent.m_panels[0].name == "HUD Panel RIGHT")
                m_inBoundryDis = (int)parent.transform.position.x - 10;
            m_outBoundryDis = (int)parent.transform.position.x;
        }

        if (m_direction == dir.UP)
        {
            if (m_inView)
            {
                if (transform.position.y > m_inBoundryDis) //if (recTrans.offsetMax.y > m_inBoundryDis)
                {
                    transform.SetPositionAndRotation(new Vector3(transform.position.x, transform.position.y - m_slideSpeed, transform.position.z), transform.rotation);
                    if (transform.position.y < m_inBoundryDis)
                    {
                        transform.SetPositionAndRotation(new Vector3(transform.position.x, m_inBoundryDis, transform.position.z), transform.rotation);
                        isOpen = true;
                    }
                }
                else
                    isOpen = true;
            }
            else if (transform.position.y < m_outBoundryDis)
            {
                transform.SetPositionAndRotation(new Vector3(transform.position.x, transform.position.y + m_slideSpeed, transform.position.z), transform.rotation);
                if (transform.position.y > m_outBoundryDis)
                    transform.SetPositionAndRotation(new Vector3(transform.position.x, m_outBoundryDis, transform.position.z), transform.rotation);
            }
        }
        else if (m_direction == dir.RIGHT)
        {
            if (m_inView)
            {
                if (transform.position.x > m_inBoundryDis)
                {
                    transform.SetPositionAndRotation(new Vector3(transform.position.x - m_slideSpeed, transform.position.y, transform.position.z), transform.rotation);
                    if (transform.position.x < m_inBoundryDis)
                    {
                        transform.SetPositionAndRotation(new Vector3(m_inBoundryDis, transform.position.y, transform.position.z), transform.rotation);
                        isOpen = true;
                    }
                }
                else
                    isOpen = true;
            }
            else if (transform.position.x < m_outBoundryDis)
            {
                transform.SetPositionAndRotation(new Vector3(transform.position.x + m_slideSpeed, transform.position.y, transform.position.z), transform.rotation);
                if (transform.position.x > m_outBoundryDis)
                    transform.SetPositionAndRotation(new Vector3(m_outBoundryDis, transform.position.y, transform.position.z), transform.rotation);
            }
        }
        else if (m_direction == dir.LEFT)
        {
            if (m_inView)
            {
                if (transform.position.x < m_inBoundryDis)
                {
                    transform.SetPositionAndRotation(new Vector3(transform.position.x + m_slideSpeed, transform.position.y, transform.position.z), transform.rotation);
                    if (transform.position.x > m_inBoundryDis)
                    {
                        transform.SetPositionAndRotation(new Vector3(m_inBoundryDis, transform.position.y, transform.position.z), transform.rotation);
                        isOpen = true;
                    }
                }
                else
                    isOpen = true;
            }
            else if (transform.position.x > m_outBoundryDis)
            {
                transform.SetPositionAndRotation(new Vector3(transform.position.x - m_slideSpeed, transform.position.y, transform.position.z), transform.rotation);
                if (transform.position.x < m_outBoundryDis)
                    transform.SetPositionAndRotation(new Vector3(m_outBoundryDis, transform.position.y, transform.position.z), transform.rotation);
            }
        }
        else if (m_direction == dir.DOWN)
        {
            if (m_inView)
            {
                if (transform.position.y < m_inBoundryDis)
                {
                    transform.SetPositionAndRotation(new Vector3(transform.position.x, transform.position.y + m_slideSpeed, transform.position.z), transform.rotation);
                    if (transform.position.y > m_inBoundryDis)
                    {
                        transform.SetPositionAndRotation(new Vector3(transform.position.x, m_inBoundryDis, transform.position.z), transform.rotation);
                        isOpen = true;
                    }
                }
                else
                    isOpen = true;
            }
            else
            {
                if (transform.position.y > m_outBoundryDis && name != "DamagePreview")
                {
                    transform.SetPositionAndRotation(new Vector3(transform.position.x, transform.position.y - m_slideSpeed, transform.position.z), transform.rotation);
                    if (transform.position.y < m_outBoundryDis)
                        transform.SetPositionAndRotation(new Vector3(transform.position.x, m_outBoundryDis, transform.position.z), transform.rotation);
                }
                else if (transform.position.y > 400)
                {
                    transform.SetPositionAndRotation(new Vector3(transform.position.x, transform.position.y - m_slideSpeed, transform.position.z), transform.rotation);
                    if (transform.position.y < m_outBoundryDis)
                        transform.SetPositionAndRotation(new Vector3(transform.position.x, m_outBoundryDis, transform.position.z), transform.rotation);
                }
            }
        }

        if (name == "ActionViewer Panel" && isOpen)
            m_panels[3].m_inView = true;
        else if (name == "StatusViewer Panel" && isOpen)
            m_panels[0].m_inView = true;
    }

    static public void CloseHistory()
    {
        while(m_history.Count > 0)
        {
            m_history[0].ClosePanel();
            m_history.RemoveAt(0);
        }
    }

    static public bool CheckIfPanelOpen()
    {
        for (int i = 0; i < m_allPanels.Count; i++)
            if (m_allPanels[i].m_inView && m_allPanels[i].m_direction == dir.UP)
                return true;

        return false;
    }

    static public PanelScript GetRecentHistory()
    {
        if (m_history.Count > 0)
            return m_history[m_history.Count - 1];

        return null;
    }

    static public void RemoveFromHistory(string _name)
    {
        if (_name == "")
        {
            m_history[m_history.Count - 1].ClosePanel();
            m_history.RemoveAt(m_history.Count - 1);
            return;
        }

        for (int i = 0; i < m_history.Count; i++)
        {
            if (m_history[i].name == _name)
            {
                m_history[i].ClosePanel();
                m_history.RemoveAt(i);
            }           
        }
    }

    static public PanelScript GetPanel(string _name)
    {
        for (int i = 0; i < m_allPanels.Count; i++)
            if (m_allPanels[i].name == _name)
                return m_allPanels[i].GetComponent<PanelScript>();

        return null;
    }
}