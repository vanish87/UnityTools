using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.Algorithm
{
    [ExecuteInEditMode]
    public class Portal3D : MonoBehaviour
    {
        [SerializeField] protected Bounds bounds;
        [SerializeField] protected Vector3 direction;
        [SerializeField] protected Vector3 localPosition;
        internal Vector3 Direction { get { return this.direction; } }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            this.direction = this.transform.forward;
        }

        public bool IsInside(Vector3 position)
        {
            return this.bounds.Contains(position);
        }
        public Vector3 GetWorldFromLocal(Vector3 localPos, Vector3 oldDirection)
        {
            var rotAngle = Vector3.Angle(oldDirection, this.direction);
            var axis = Vector3.Cross(oldDirection, this.direction).normalized;

            var rot = Quaternion.AngleAxis(rotAngle, axis);

            var newLocal = rot * localPos;

            return this.transform.position + newLocal;
        }

        private void OnDrawGizmos()
        {
            Gizmos.matrix = this.transform.localToWorldMatrix;
            Gizmos.DrawWireCube(this.bounds.center, this.bounds.size);
        }
    }
}