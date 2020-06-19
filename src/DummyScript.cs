using UnityEngine;

public class DummyScript : MonoBehaviour
{
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.T))
		{
			Util.GetTerrainHeight(new Vector3(-1300f, 0f, 500f));
			Util.GetTerrainHeight(new Vector3(500f, 0f, 500f));
			Util.GetTerrainHeight(new Vector3(500f, 0f, -1400f));
			Util.GetTerrainHeight(new Vector3(-1300f, 0f, -1300f));
		}
	}

	private void Start()
	{
		float num = 345600f;
		for (int i = 0; i < 1000; i++)
		{
			num -= 0.01f;
		}
		Debug.Log(num);
	}
}
