using UnityEngine;

public class FakeServer : MonoBehaviour
{
	public int m_maxCharCount = 10;

	public int m_maxHouseCount = 100;

	public GameObject m_playerPrefab;

	public GameObject m_housePrefab;

	public GUIText m_debugGuiText;

	public float m_radius = 3000f;

	private int m_curHouses;

	private int m_curStep = 1;

	private float m_nextUpdateTime;

	private void Start()
	{
	}

	private void Update()
	{
		if (Time.time > m_nextUpdateTime)
		{
			if (m_curHouses < m_maxHouseCount)
			{
				Object.Instantiate(position: new Vector3(Random.Range(0f - m_radius, m_radius), 1f, Random.Range(0f - m_radius, m_radius)), original: m_housePrefab, rotation: Quaternion.identity);
				m_curHouses++;
			}
			int a_id = Random.Range(0, m_curStep);
			AssignInput(a_id, Random.Range(0, 9));
			m_nextUpdateTime = Time.time + 1f / (float)m_curStep;
			if (m_curStep < m_maxCharCount)
			{
				m_curStep++;
			}
			m_debugGuiText.text = "curStep: " + m_curStep + " houses: " + m_curHouses + " dt: " + Time.smoothDeltaTime;
		}
	}

	private void AssignInput(int a_id, int a_inputdir)
	{
		bool flag = false;
		FakePlayer[] array = (FakePlayer[])Object.FindObjectsOfType(typeof(FakePlayer));
		FakePlayer[] array2 = array;
		foreach (FakePlayer fakePlayer in array2)
		{
			if (a_id == fakePlayer.m_id)
			{
				fakePlayer.SetInput(a_inputdir);
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			SpawnPlayer(a_id, a_inputdir);
		}
	}

	private void SpawnPlayer(int a_id, int a_inputdir = 0)
	{
		GameObject gameObject = (GameObject)Object.Instantiate(position: new Vector3(Random.Range((0f - m_radius) * 0.8f, m_radius * 0.8f), 1f, Random.Range((0f - m_radius) * 0.8f, m_radius * 0.8f)), original: m_playerPrefab, rotation: Quaternion.identity);
		FakePlayer component = gameObject.GetComponent<FakePlayer>();
		component.m_id = a_id;
		component.SetInput(a_inputdir);
	}
}
