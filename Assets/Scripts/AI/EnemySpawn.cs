using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawn : MonoBehaviour {

    public GameObject[] m_uniqueEnemies;

    // Use this for initialization
    void Start ()
    {
        GetComponent<SpriteRenderer>().enabled = false;

        m_uniqueEnemies = Resources.LoadAll<GameObject>("Enemies/");
        SpawnRandomEnemy();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void SpawnRandomEnemy()
    {
        int randEnemy = Random.Range(0, m_uniqueEnemies.Length);
        Instantiate(m_uniqueEnemies[randEnemy], transform.position, transform.rotation);
    }
}
