using SimpleJSON;
using UnityEngine;

public class JsonItems
{
	private static JSONNode m_items;

	private static void Init()
	{
		TextAsset textAsset = (TextAsset)Resources.Load("inventory_steam");
		if (null != textAsset)
		{
			m_items = JSONNode.Parse(textAsset.text);
		}
	}

	public static JSONNode GetItem(int a_id)
	{
		if (null == m_items)
		{
			Init();
		}
		if (null != m_items)
		{
			for (int i = 0; i < m_items["items"].Count; i++)
			{
				if (m_items["items"][i]["itemdefid"].AsInt == a_id)
				{
					return m_items["items"][i];
				}
			}
		}
		return null;
	}
}
