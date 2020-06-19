using UnityEngine;

public class ChatLabel : MonoBehaviour
{
	public bool m_dropShadow;

	public Color m_shadowColor = Color.black;

	public Vector3 m_shadowOffset = new Vector3(-1f, -2f, 0f);

	private Quaternion m_localRot = Quaternion.Euler(55f, 0f, 0f);

	private TextMesh m_shadowText;

	private TextMesh m_text;

	private float m_textDisappearTime;

	private void Start()
	{
		if (m_dropShadow)
		{
			CreateShadow();
		}
	}

	private void LateUpdate()
	{
		if (null != m_text)
		{
			if (Time.time > m_textDisappearTime && m_textDisappearTime > 0f)
			{
				m_text.gameObject.SetActive(false);
				m_textDisappearTime = 0f;
			}
			m_text.transform.rotation = m_localRot;
		}
	}

	public void SetText(string a_text, bool a_stayForever = false)
	{
		if (null == m_text)
		{
			m_text = GetComponent<TextMesh>();
		}
		string text = string.Empty;
		if (a_text.Length > 50)
		{
			string[] array = a_text.Split(' ');
			int num = array.Length / 2 - 1;
			for (int i = 0; i < array.Length; i++)
			{
				text += array[i];
				text = ((i != num) ? (text + " ") : (text + "\n"));
			}
		}
		else
		{
			text = a_text;
		}
		if (null != m_text)
		{
			m_text.text = text;
			if (null != m_shadowText)
			{
				m_shadowText.text = text;
			}
			m_text.transform.rotation = m_localRot;
			m_text.gameObject.SetActive(true);
			m_textDisappearTime = ((!a_stayForever) ? (Time.time + 4f + (float)text.Length * 0.25f) : 0f);
		}
	}

	private void CreateShadow()
	{
		if (null != m_shadowText)
		{
			Object.DestroyImmediate(m_shadowText.gameObject);
		}
		GameObject gameObject = (GameObject)Object.Instantiate(base.gameObject, base.transform.position, base.transform.rotation);
		Object.DestroyImmediate(gameObject.GetComponent<ChatLabel>());
		gameObject.transform.parent = base.transform;
		gameObject.renderer.material.color = m_shadowColor;
		gameObject.transform.localPosition = m_shadowOffset;
		m_shadowText = gameObject.GetComponent<TextMesh>();
	}
}
