using UnityEngine;

public class EditorDebugObj : MonoBehaviour
{
	public string m_debugLanguage = "English";

	private void Awake()
	{
		if (Application.isEditor)
		{
			LNG.Init(m_debugLanguage);
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}
}
