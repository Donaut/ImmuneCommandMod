using UnityEngine;

public class TheOneAndOnly : MonoBehaviour
{
	private void Awake()
	{
		TheOneAndOnly[] array = (TheOneAndOnly[])Object.FindObjectsOfType(typeof(TheOneAndOnly));
		if (1 < array.Length)
		{
			Object.Destroy(base.gameObject);
		}
		else
		{
			Object.DontDestroyOnLoad(base.gameObject);
		}
	}
}
