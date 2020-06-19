using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class Util : MonoBehaviour
{
	public static string Md5(string strToEncrypt)
	{
		UTF8Encoding uTF8Encoding = new UTF8Encoding();
		byte[] bytes = uTF8Encoding.GetBytes(strToEncrypt);
		MD5CryptoServiceProvider mD5CryptoServiceProvider = new MD5CryptoServiceProvider();
		byte[] array = mD5CryptoServiceProvider.ComputeHash(bytes);
		string text = string.Empty;
		for (int i = 0; i < array.Length; i++)
		{
			text += Convert.ToString(array[i], 16).PadLeft(2, '0');
		}
		return text.PadLeft(32, '0');
	}

	public static void SetLayerRecursively(Transform a_transform, int a_layer)
	{
		a_transform.gameObject.layer = a_layer;
		foreach (Transform item in a_transform)
		{
			item.gameObject.layer = a_layer;
			if (0 < item.childCount)
			{
				SetLayerRecursively(item, a_layer);
			}
		}
	}

	public static float GetLightIntensity(float a_progress, out float a_pip)
	{
		a_pip = 1.33f;
		return Mathf.Clamp01(Mathf.Sin(a_progress * (float)Math.PI * a_pip));
	}

	public static void Attack(Transform a_victim, Transform a_aggressor, float a_damage)
	{
		if (null != a_victim)
		{
			a_victim.SendMessage("ChangeHealthBy", 0f - a_damage, SendMessageOptions.DontRequireReceiver);
			a_victim.SendMessage("SetAggressor", a_aggressor, SendMessageOptions.DontRequireReceiver);
		}
	}

	public static string GetItemDefHash(int a_defId, ulong a_steamId)
	{
		return Md5(a_defId.ToString() + a_steamId + "Version_0_4_8_B");
	}

	public static float GetTerrainHeight(Vector3 a_pos)
	{
		Terrain[] activeTerrains = Terrain.activeTerrains;
		foreach (Terrain terrain in activeTerrains)
		{
			bool num = a_pos.x < 0f;
			Vector3 position = terrain.transform.position;
			if (!(num ^ (position.x < 0f)))
			{
				bool num2 = a_pos.z < 0f;
				Vector3 position2 = terrain.transform.position;
				if (!(num2 ^ (position2.z < 0f)))
				{
					return terrain.SampleHeight(a_pos);
				}
			}
		}
		return 0f;
	}
}
