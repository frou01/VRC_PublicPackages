
#if (UNITY_EDITOR) 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class terrainPlacer : MonoBehaviour
{
    public Vector3 offset;
    public void PlaceObject()
    {
        foreach(Transform child in transform)
        {
            Debug.Log("debug perform on " + child.name);
            RaycastHit hitInfo;
            if(Physics.Raycast(child.position + child.rotation * offset, new Vector3(0,-100,0), out hitInfo, 100, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                Debug.Log("debug Collide to " + hitInfo.collider.name);
                child.position = hitInfo.point - child.rotation * new Vector3(offset.x, 0, offset.z);
                Debug.Log("debug Point " + hitInfo.point);
            }
        }
    }
}
#endif
