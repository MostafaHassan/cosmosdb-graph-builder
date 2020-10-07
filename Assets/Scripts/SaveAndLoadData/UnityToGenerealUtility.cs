using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class UnityToGenerealUtility : MonoBehaviour
{
    public static Dictionary<int, GameObject> GetEntitiesInstanceIDs()
    {
        EntityManager em = Camera.main.GetComponent<EntityManager>();
        
        Dictionary<int, GameObject> m_instanceMap = new Dictionary<int, GameObject>();
        //record instance map
        m_instanceMap.Clear();
        List<GameObject> gos = new List<GameObject>();
        //foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)))
        foreach (GameObject go in em.entities)
        {
            if (gos.Contains(go))
            {
                continue;
            }
            gos.Add(go);
            m_instanceMap[go.GetInstanceID()] = go;
        }

        return m_instanceMap;
    }
}