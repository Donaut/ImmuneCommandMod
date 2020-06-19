using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Invoker : MonoBehaviour
{
	public delegate void InvokeDelegate();

	private static Thread m_mainThread = null;

	private static List<InvokeDelegate> m_invokeDelegates = new List<InvokeDelegate>();

	public static void Invoke(InvokeDelegate a_d)
	{
		lock (m_invokeDelegates)
		{
			if (m_mainThread == null)
			{
				return;
			}
			if (m_mainThread == Thread.CurrentThread)
			{
				a_d();
				return;
			}
			Monitor.Enter(a_d);
			m_invokeDelegates.Add(a_d);
		}
		Monitor.Wait(a_d);
	}

	private static IEnumerator InvokeOnce()
	{
		while (true)
		{
			lock (m_invokeDelegates)
			{
				foreach (InvokeDelegate d in m_invokeDelegates)
				{
					lock (d)
					{
						d();
						Monitor.Pulse(d);
					}
				}
				m_invokeDelegates.Clear();
			}
			yield return null;
		}
	}

	private void Awake()
	{
		m_mainThread = Thread.CurrentThread;
		StartCoroutine(InvokeOnce());
	}
}
