using UnityEngine;
using UnityEngine.Assertions;

namespace UnityTools
{
    public static class ObjectTool
    {
        public static void DestoryObj(this Object obj, float t = 0f)
        {
            if (obj != null)
            {
                if (Application.isPlaying)
                    Object.Destroy(obj, t);
                else
                    Object.DestroyImmediate(obj);
            }
        }
        public static T FindOrAddTypeInComponentsAndChilden<T>(this GameObject obj) where T : Component
        {
            var ret = obj.GetComponent<T>();

            if (ret == null)
            {
                ret = obj.GetComponentInChildren<T>();
            }
            if (ret == null)
            {
                ret = obj.AddComponent<T>();
            }

            Assert.IsTrue(ret != null);

            return ret;
        }
        public static GameObject[] FindRootObject()
        {
            return System.Array.FindAll(GameObject.FindObjectsOfType<GameObject>(), (item) => item.transform.parent == null);
        }
    }
}