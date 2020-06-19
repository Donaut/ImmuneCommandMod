using UnityEngine;
using UnityEngine.UI;

public class TextLNG : MonoBehaviour
{
	public string m_lngKey = string.Empty;

	private void Start()
	{
		TranslateText();
	}

	public void TranslateText()
	{
		string text = LNG.Get(m_lngKey);
		Text component = GetComponent<Text>();
		if (null != component)
		{
			component.text = text;
			return;
		}
		TextMesh component2 = GetComponent<TextMesh>();
		if (null != component2)
		{
			component2.text = text;
		}
	}
}
