using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class LinePlaceObjects : MonoBehaviour
{
    public GameObject objToPlace;
    private List<GameObject> m_placedObjects;
    public float objDistance = 1f;
    public int numOfObjs = 5;
    public bool updateObjects = false;

    void Start()
    {
        this.enabled = false;
    }

    public void Update()
    {

        if(!updateObjects)
            return;

        updateObjects = false;

        if(objToPlace == null)
            return;

        if(m_placedObjects.Count > 0)
        {
            for(int i = 0; i < m_placedObjects.Count; i++)
            {
                DestroyImmediate(m_placedObjects[i]);
            }

            m_placedObjects.Clear();
        }

        for(int i = 0; i < numOfObjs; i++)
        {
            Vector3 pos = Vector3.zero;
            pos= (i * objDistance) * gameObject.transform.right;

            Debug.LogError(pos + transform.position);

            RaycastHit hit;

            if(Physics.Raycast(pos + transform.position, Vector3.down, out hit, 1000f))
                pos = hit.point - transform.position;

            GameObject obj = Instantiate(objToPlace, gameObject.transform);
            obj.transform.localPosition = pos;

            m_placedObjects.Add(obj);
        }
    }
}
