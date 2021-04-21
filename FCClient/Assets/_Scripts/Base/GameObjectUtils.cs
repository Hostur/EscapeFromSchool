using System;
using System.Linq;
using System.Reflection;
using FCData.Common;
using FCData.Common.Vectors;
using FCData.Reflection;
using UnityEngine;

namespace Base
{
	public static class GameObjectsUtils
	{
		public static void InjectComponents<T>(this T obj) where T : FCBehaviour
		{
			foreach (FieldInfo fieldInfo in obj.GetFieldsWithAttribute<T, GetAttribute>().Where(f => f.GetValue(obj) == null))
			{
				try
				{
					fieldInfo.SetValue((object)obj, obj.GetComponent(fieldInfo.FieldType));
				}
				catch (Exception e)
				{
					throw new Exception($"Exception occur during injecting field {fieldInfo.FieldType} into {typeof(T)}: {e.ToString()}");
				}
			}
			foreach (PropertyInfo propertyInfo in obj.GetPropertiesWithAttribute<T, GetAttribute>().Where(f => f.GetValue(obj) == null))
			{
				try
				{
					propertyInfo.SetValue((object)obj, obj.GetComponent(propertyInfo.PropertyType));
				}
				catch (Exception e)
				{
					throw new Exception($"Exception occur during injecting property {propertyInfo.PropertyType} into {typeof(T)}: {e.ToString()}");
				}
			}
		}

		public static void FindComponents<T>(this T obj) where T : FCBehaviour
		{
			foreach (FieldInfo fieldInfo in obj.GetFieldsWithAttribute<T, FindAttribute>().Where(f => f.GetValue(obj) == null))
			{
				try
				{
					fieldInfo.SetValue((object)obj, GameObject.FindObjectOfType(fieldInfo.FieldType));
				}
				catch (Exception e)
				{
					throw new Exception($"Exception occur during injecting field {fieldInfo.FieldType} into {typeof(T)}: {e.ToString()}");
				}
			}
			foreach (PropertyInfo propertyInfo in obj.GetPropertiesWithAttribute<T, GetAttribute>().Where(f => f.GetValue(obj) == null))
			{
				try
				{
					propertyInfo.SetValue((object)obj, GameObject.FindObjectOfType(propertyInfo.PropertyType));
				}
				catch (Exception e)
				{
					throw new Exception($"Exception occur during injecting property {propertyInfo.PropertyType} into {typeof(T)}: {e.ToString()}");
				}
			}
		}

		public static void AssertSerializeFields<T>(this T obj) where T : FCBehaviour
		{
			foreach (FieldInfo fieldInfo in obj.GetFieldsWithAttribute<T, SerializeField>().Where(f => f.GetValue(obj) == null))
				obj.Log($"SerializeField {fieldInfo.Name} not assigned.", LogLevel.Error);
		}

		public static Quaternion LookRotation(this SerializableVector4 quaternion) =>
			Quaternion.LookRotation(quaternion.ToUnityVector3());
	}
}