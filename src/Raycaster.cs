using System;
using UnityEngine;

public static class Raycaster
{
	public static float Attack(Transform a_aggressor, ItemDef a_weapon, Vector3 a_targetPos, ref Transform a_target)
	{
		if (a_weapon.damage < 1f)
		{
			Debug.Log(a_aggressor.name + " is firing an invalid weapon at " + Time.time);
			return 0f;
		}
		Transform transform = null;
		Vector3 a = (!(null != a_target)) ? a_targetPos : a_target.position;
		a.y = 0f;
		bool flag = 0 == a_weapon.ammoItemType;
		Vector3 vector = Vector3.up * ((!flag) ? 1.5f : 0.8f);
		Vector3 position = a_aggressor.position;
		position.y = 0f;
		position += vector;
		Vector3 a2 = a + vector;
		Vector3 vector2 = a2 - position;
		float num = a_weapon.damage;
		if (vector2.sqrMagnitude > 3.5f || null == a_target)
		{
			int layerMask = -5;
			RaycastHit hitInfo;
			if (flag)
			{
				Collider[] array = Physics.OverlapSphere(a_targetPos + vector, 0.4f, layerMask);
				if (array != null && 0 < array.Length)
				{
					transform = array[0].transform;
				}
			}
			else if (Physics.Raycast(position, vector2.normalized, out hitInfo, a_weapon.range, layerMask))
			{
				transform = hitInfo.transform;
				float num2 = vector2.sqrMagnitude / (a_weapon.range * a_weapon.range) * 0.5f + 0.5f;
				num *= Mathf.Sin(num2 * (float)Math.PI);
				if (num < 1f)
				{
					num = 1f;
				}
			}
		}
		else
		{
			transform = a_target;
		}
		if (null != transform)
		{
			Util.Attack(transform, a_aggressor, num);
		}
		else
		{
			num = 0f;
		}
		a_target = transform;
		return num;
	}

	public static bool BuildingSphereCast(Vector3 a_pos)
	{
		Vector3 a_pos2 = a_pos;
		a_pos2.y = 2f;
		return CheckSphere(a_pos2, 0.8f);
	}

	public static bool CheckSphere(Vector3 a_pos, float a_radius)
	{
		Collider[] array = Physics.OverlapSphere(a_pos, a_radius);
		bool flag = array != null && 0 < array.Length;
		if (flag && Application.isEditor)
		{
			string str = "CheckSphere colliders: ";
			for (int i = 0; i < array.Length; i++)
			{
				str = str + array[i].name + ", ";
			}
		}
		return flag;
	}
}
