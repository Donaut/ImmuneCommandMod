using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;

public class CFX2_Demo : MonoBehaviour
{
	public bool orderedSpawns = true;

	public float step = 1f;

	public float range = 5f;

	private float order = -5f;

	public Material groundMat;

	public Material waterMat;

	public GameObject[] ParticleExamples;

	private int exampleIndex;

	private string randomSpawnsDelay = "0.5";

	private bool randomSpawns;

	private bool slowMo;

	private void OnMouseDown()
	{
		RaycastHit hitInfo = default(RaycastHit);
		if (base.collider.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, 9999f))
		{
			GameObject gameObject = spawnParticle();
			gameObject.transform.position = hitInfo.point + gameObject.transform.position;
		}
	}

	private GameObject spawnParticle()
	{
		GameObject gameObject = (GameObject)Object.Instantiate(ParticleExamples[exampleIndex]);
		Transform transform = gameObject.transform;
		Vector3 position = gameObject.transform.position;
		transform.position = new Vector3(0f, position.y, 0f);
		gameObject.SetActive(true);
		for (int i = 0; i < gameObject.transform.childCount; i++)
		{
			gameObject.transform.GetChild(i).gameObject.SetActive(true);
		}
		return gameObject;
	}

	private void OnGUI()
	{
		GUILayout.BeginArea(new Rect(5f, 20f, Screen.width - 10, 60f));
		GUILayout.BeginHorizontal();
		GUILayout.Label("Effect");
		if (GUILayout.Button("<"))
		{
			prevParticle();
		}
		GUILayout.Label(ParticleExamples[exampleIndex].name, GUILayout.Width(210f));
		if (GUILayout.Button(">"))
		{
			nextParticle();
		}
		GUILayout.Label("Click on the ground to spawn selected particles", GUILayout.Width(150f));
		if (GUILayout.Button((!CFX_Demo_RotateCamera.rotating) ? "Rotate Camera" : "Pause Camera"))
		{
			CFX_Demo_RotateCamera.rotating = !CFX_Demo_RotateCamera.rotating;
		}
		if (GUILayout.Button((!randomSpawns) ? "Start Random Spawns" : "Stop Random Spawns", GUILayout.Width(140f)))
		{
			randomSpawns = !randomSpawns;
			if (randomSpawns)
			{
				StartCoroutine("RandomSpawnsCoroutine");
			}
			else
			{
				StopCoroutine("RandomSpawnsCoroutine");
			}
		}
		randomSpawnsDelay = GUILayout.TextField(randomSpawnsDelay, 10, GUILayout.Width(42f));
		randomSpawnsDelay = Regex.Replace(randomSpawnsDelay, "[^0-9.]", string.Empty);
		if (GUILayout.Button((!base.renderer.enabled) ? "Show Ground" : "Hide Ground", GUILayout.Width(90f)))
		{
			base.renderer.enabled = !base.renderer.enabled;
		}
		if (GUILayout.Button((!slowMo) ? "Slow Motion" : "Normal Speed", GUILayout.Width(100f)))
		{
			slowMo = !slowMo;
			if (slowMo)
			{
				Time.timeScale = 0.33f;
			}
			else
			{
				Time.timeScale = 1f;
			}
		}
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
	}

	private IEnumerator RandomSpawnsCoroutine()
	{
		while (true)
		{
			GameObject particles = spawnParticle();
			if (orderedSpawns)
			{
				Transform transform = particles.transform;
				Vector3 position = base.transform.position;
				float x = order;
				Vector3 position2 = particles.transform.position;
				transform.position = position + new Vector3(x, position2.y, 0f);
				order -= step;
				if (order < 0f - range)
				{
					order = range;
				}
			}
			else
			{
				Transform transform2 = particles.transform;
				Vector3 a = base.transform.position + new Vector3(Random.Range(0f - range, range), 0f, Random.Range(0f - range, range));
				Vector3 position3 = particles.transform.position;
				transform2.position = a + new Vector3(0f, position3.y, 0f);
			}
			yield return new WaitForSeconds(float.Parse(randomSpawnsDelay));
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			prevParticle();
		}
		else if (Input.GetKeyDown(KeyCode.RightArrow))
		{
			nextParticle();
		}
	}

	private void prevParticle()
	{
		exampleIndex--;
		if (exampleIndex < 0)
		{
			exampleIndex = ParticleExamples.Length - 1;
		}
		if (ParticleExamples[exampleIndex].name.Contains("Splash") || ParticleExamples[exampleIndex].name.Contains("Skim"))
		{
			base.renderer.material = waterMat;
		}
		else
		{
			base.renderer.material = groundMat;
		}
	}

	private void nextParticle()
	{
		exampleIndex++;
		if (exampleIndex >= ParticleExamples.Length)
		{
			exampleIndex = 0;
		}
		if (ParticleExamples[exampleIndex].name.Contains("Splash") || ParticleExamples[exampleIndex].name.Contains("Skim"))
		{
			base.renderer.material = waterMat;
		}
		else
		{
			base.renderer.material = groundMat;
		}
	}
}
