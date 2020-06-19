using System.Threading;
using UnityEngine;

public class ThreadTest : MonoBehaviour
{
	private object oThreadLock = new object();

	private float m_threadTime;

	private Thread m_thread;

	private void Start()
	{
		m_thread = new Thread(ThreadFunc);
		m_thread.IsBackground = true;
		m_thread.Start();
	}

	private void ThreadFunc()
	{
		while (true)
		{
			Thread.Sleep(1000);
			float num = 0f;
			lock (oThreadLock)
			{
				num = m_threadTime;
			}
			Debug.Log(num);
		}
	}

	private void OnApplicationQuit()
	{
		m_thread.Abort();
		Debug.Log("OnApplicationQuit()");
	}

	private void Update()
	{
		lock (oThreadLock)
		{
			m_threadTime += Time.deltaTime;
		}
	}
}
