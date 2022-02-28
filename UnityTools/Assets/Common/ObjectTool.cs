using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Assertions;

namespace UnityTools
{
	public interface TypeValue<T> where T : System.Enum
	{
		T ValueType { get; }
	}
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
		public static void DestoryObj(this MonoBehaviour obj, float t = 0f)
		{
			if (obj != null)
			{
				if (Application.isPlaying)
					Object.Destroy(obj.gameObject, t);
				else
					Object.DestroyImmediate(obj.gameObject);
			}
		}
		public static T FindOrAddTypeInComponentsAndChildren<T>(this GameObject obj) where T : Component
		{
			var ret = obj.GetComponentInChildren<T>();

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

		public static IEnumerable<System.Type> FindAllTypes<T>()
		{
			var type = typeof(T);
			var types = System.AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(s => s.GetTypes())
				.Where(p => p.GetInterfaces().Contains(type));

			return types;
		}

		public static List<T> FindAllObject<T>()
		{
			var users = new List<T>();
			foreach (var g in FindRootObject())
			{
				users.AddRange(g.GetComponentsInChildren<T>());
			}

			return users;
		}
		public static Dictionary<T, V> FindAllValueTypes<T, V>() where V : TypeValue<T> where T : System.Enum
		{
			var ret = new Dictionary<T, V>();
			foreach (var v in ObjectTool.FindAllObject<V>())
			{
				ret.TryAdd(v.ValueType, v);
			}
			return ret;
		}

		public static bool IsNoneSerializable(System.Reflection.FieldInfo field)
		{
			return typeof(Texture).IsAssignableFrom(field.FieldType)
				|| System.Attribute.IsDefined(field, typeof(Common.NoneSerializeAttribute));
		}

		public static IEnumerable<System.Reflection.FieldInfo> FindAllSerializeField(System.Type type, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
		{
			return type.GetFields(flags)
					.Where(field => !IsNoneSerializable(field));
		}
		public static IEnumerable<System.Reflection.FieldInfo> FindAllNoneSerializeField(System.Type type, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
		{
			return type.GetFields(flags)
					.Where(field => IsNoneSerializable(field));
		}
		public static IEnumerable<T> FindAllFieldValue<T>(System.Type type, object obj, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
		{
			return type.GetFields(flags)
					   .Where(field => typeof(T).IsAssignableFrom(field.FieldType))
					   .Select(v => (T)v.GetValue(obj));
		}

		public static T[] SubArray<T>(this T[] data, int index, int length)
		{
			T[] result = new T[length];
			System.Array.Copy(data, index, result, 0, length);
			return result;
		}

		public static T DeepCopy<T>(this T other)
		{
			if (other == null) return default;
			using (MemoryStream ms = new MemoryStream())
			{
				BinaryFormatter formatter = new BinaryFormatter();
				formatter.Serialize(ms, other);
				ms.Position = 0;
				return (T)formatter.Deserialize(ms);
			}
		}

		public static T DeepCopyJson<T>(this T other)
		{
			if (other == null) return default;
			var json = JsonUtility.ToJson(other);
			return JsonUtility.FromJson<T>(json);
		}
	}
}