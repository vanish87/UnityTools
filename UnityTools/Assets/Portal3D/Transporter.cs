using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Transporter : MonoBehaviour
{
    [SerializeField] protected Portal3D input;
    [SerializeField] protected Portal3D output;

    [SerializeField] protected GameObject target;
    [SerializeField] protected GameObject targetOutput;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        targetOutput.transform.position = output.GetWorldFromLocal(target.transform.position - input.transform.position, input.Direction);
    }

    private void OnDrawGizmos()
    {
        
    }
}
