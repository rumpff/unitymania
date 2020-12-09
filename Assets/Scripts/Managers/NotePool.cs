using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotePool : MonoBehaviour
{
    private GameObject m_prefab;
    private GameObject[] m_pool;

    private int nextObject;
    private string poolName;

    public string PoolName { get { return poolName; } }

    public void Build(int amount, GameObject prefab, string name)
    {
        m_pool = new GameObject[amount];

        for (int i = 0; i < m_pool.Length; i++)
        {
            m_pool[i] = Instantiate(prefab);
        }
    }

    private void Double()
    {
        int initialLength = m_pool.Length;
        GameObject[] newPool = new GameObject[m_pool.Length*2];


        for (int i = 0; i < initialLength; i++)
        {
            newPool[i] = m_pool[i];
        }

        for (int i = 0; i < initialLength; i++)
        {
            newPool[i] = Instantiate(m_prefab);
        }
    }

    public GameObject GetNext()
    {
        // Search for new gameobject
        for (int i = 0; i < m_pool.Length; ++i)
        {
            // See if the object is available
            if (!m_pool[nextObject].activeInHierarchy)
            {
                return m_pool[nextObject];
            } 
            // if not increment 
            else ++nextObject;

            // Limit nextObject to the length
            // of the object pool
            if (nextObject >= m_pool.Length)
                nextObject %= m_pool.Length;
        }

        // if none is found
        // double the size
        // of the object pool
        Double();

        // return the first
        // of the new half
        // of the object pool
        return m_pool[nextObject++];
    }
}
