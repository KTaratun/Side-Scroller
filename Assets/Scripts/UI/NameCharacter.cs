using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NameCharacter : MonoBehaviour {

    public InputField m_name;
    public Button[] m_buttons;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void NameChange()
    {
        if (m_name.text == "")
        {
            for (int i = 0; i < m_buttons.Length; i++)
                m_buttons[i].interactable = false;
        }
        else
        {
            for (int i = 0; i < m_buttons.Length; i++)
                m_buttons[i].interactable = true;
        }
    }
}
