using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomBase : MonoBehaviour {

    public enum nbors { left, top, right, bottom};

    public RoomBase[] m_neighbors;
    public GameObject m_door;
    public List<GameObject> m_spawnPoints;
    public List<GameObject> m_items;

    public int m_id;
    public int m_x;
    public int m_y;

	// Use this for initialization
	void Start ()
    {
        //InitObjects();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void InitObjects()
    {
        Tiled2Unity.TileLayer[] tileChildren = GetComponentsInChildren<Tiled2Unity.TileLayer>();
        foreach (Tiled2Unity.TileLayer child in tileChildren)
        {
            if (child.name == "door")
            {
                Transform[] childsChildren = child.GetComponentsInChildren<Transform>();
                m_door = childsChildren[0].gameObject;
                child.gameObject.SetActive(false);
                return;
            }
            else if (child.name == "platform")
            {
                Transform[] childsChildren = child.GetComponentsInChildren<Transform>();
                foreach (Transform childsChild in childsChildren)
                {
                    if (childsChild.GetComponent<BoxCollider2D>())
                        childsChild.GetComponent<BoxCollider2D>().isTrigger = true;

                    childsChild.tag = "Platform";
                }
            }
            else if (child.name == "spawn")
                LoadAndDisableObjects(child, m_spawnPoints);
            else if (child.name == "items")
                LoadAndDisableObjects(child, m_spawnPoints);
        }
    }

    private void LoadAndDisableObjects(Tiled2Unity.TileLayer _child, List<GameObject> _list)
    {
        Transform[] childsChildren = _child.GetComponentsInChildren<Transform>();

        foreach (Transform childsChild in childsChildren)
        {
            _list.Add(childsChild.gameObject);
            if (childsChild.GetComponent<BoxCollider2D>())
                childsChild.GetComponent<BoxCollider2D>().enabled = false;
        }
    }

    static public nbors InvertFacing(nbors _facing)
    {
        if (_facing == nbors.left)
            return nbors.right;
        else if (_facing == nbors.top)
            return nbors.bottom;
        else if (_facing == nbors.right)
            return nbors.left;
        else if (_facing == nbors.bottom)
            return nbors.top;

        return 0;
    }
}
