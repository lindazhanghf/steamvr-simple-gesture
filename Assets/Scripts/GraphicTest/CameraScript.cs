using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public GameObject[] objects;
    public LayerMask layerMask;
    public int num;

    GameObject[] OutOfSight;

    void Update()
    {
        num = 0;
        foreach (GameObject obj in objects)
        {
            Debug.DrawRay(transform.position, obj.transform.position - transform.position);
            if (Physics.Raycast(transform.position, obj.transform.position - transform.position, 20, layerMask, QueryTriggerInteraction.Collide))
            {
                num++;
            }
        }
        Debug.Log(num);
    }
}
