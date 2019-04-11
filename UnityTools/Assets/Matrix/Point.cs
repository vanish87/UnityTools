using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Point : MonoBehaviour
{
    public Color color = Color.red;
    
    void OnDrawGizmos()
    {
        var old = Gizmos.color;
        Gizmos.color = this.color;
        Gizmos.DrawSphere(this.transform.position, 1);
        Gizmos.color = old;
    }
}
