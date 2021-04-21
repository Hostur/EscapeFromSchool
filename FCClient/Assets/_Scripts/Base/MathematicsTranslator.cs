using System.Runtime.CompilerServices;
using FCData.Common.Vectors;
using UnityEngine;

public static class MathematicsTranslator
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 ToUnityVector(this SerializableVector2 v) => new Vector2(v.X, v.Y);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static SerializableVector2 ToSerializableVector2(this Vector2 v) => new SerializableVector2(v.x, v.y);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static SerializableVector3 ToSerializableVector3(this Vector3 v) => new SerializableVector3(v.x, v.y, v.z);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 ToUnityVector(this SerializableVector3 v) => new Vector3(v.X, v.Y, v.Z);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector4 ToUnityVector(this SerializableVector4 v) => new Vector4(v.X, v.Y, v.Z, v.W);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 ToUnityVector3(this SerializableVector4 v) => new Vector3(v.X, v.Y, v.Z);


	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 ToUnityVector3(this SerializableVector3 v) => new Vector3(v.X, v.Y, v.Z);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static SerializableVector4 ToSerializableVector4(this Quaternion quaternion)
	{
		return new SerializableVector4(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
	}
}