using UnityEngine;

public class AntFood : MonoBehaviour
{
	public void Consume()
	{
		base.transform.position = new Vector3(Random.Range(-10f, 10f), 0f, Random.Range(-10f, 10f));
	}
}