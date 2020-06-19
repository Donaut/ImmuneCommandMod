using UnityEngine;
using UnityEngine.UI;

public class LanguageSetter : MonoBehaviour
{
	public Text m_curLngTxt;

	private void Awake()
	{
		string @string = PlayerPrefs.GetString("prefLang", "English");
		SetLanguage(@string);
	}

	public void SetLanguage(string a_lng)
	{
		LNG.Init(a_lng);
		PlayerPrefs.SetString("prefLang", a_lng);
		if (null != m_curLngTxt)
		{
			m_curLngTxt.text = a_lng;
		}
		TextLNG[] array = Object.FindObjectsOfType<TextLNG>();
		TextLNG[] array2 = array;
		foreach (TextLNG textLNG in array2)
		{
			if (null != textLNG)
			{
				textLNG.TranslateText();
			}
		}
	}
}
