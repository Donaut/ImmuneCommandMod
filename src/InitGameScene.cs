using UnityEngine;

public class InitGameScene : MonoBehaviour
{
	public GameObject m_exlMarkPrefab;

	public Texture m_dummyTex;

	private void Start()
	{
		Renderer[] array = Object.FindObjectsOfType<Renderer>();
		for (int i = 0; i < array.Length; i++)
		{
			if (m_dummyTex == array[i].material.mainTexture)
			{
				Object.Destroy(array[i]);
			}
		}
		if (Global.isServer)
		{
			return;
		}
		BodyHeadAnim[] array2 = Object.FindObjectsOfType<BodyHeadAnim>();
		for (int j = 0; j < array2.Length; j++)
		{
			if (array2[j].gameObject.layer == 9)
			{
				GameObject gameObject = (GameObject)Object.Instantiate(m_exlMarkPrefab, array2[j].transform.position + Vector3.up * 4.4f, Quaternion.Euler(270f, 0f, 0f));
				gameObject.transform.parent = array2[j].transform;
			}
		}
	}
}
