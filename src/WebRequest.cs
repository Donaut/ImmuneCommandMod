using System.Collections;
using UnityEngine;

public static class WebRequest
{
	public static string m_serverlist = string.Empty;

	private static string m_url = "http://gameinterface.vidiludi.com/gi.php";

	public static IEnumerator UpdateServer(int a_port, string a_name, int a_playerCount)
	{
		a_name = a_name.Replace(" ", "%20");
		a_name = a_name.Replace(";", string.Empty);
		string url = m_url + "?n=" + a_name + "&p=" + a_port + "&pc=" + a_playerCount + "&pm=" + 50;
		yield return new WWW(url);
	}

	public static IEnumerator DeleteServer(int a_port)
	{
		string url = m_url + "?ds=1&p=" + a_port;
		yield return new WWW(url);
	}

	public static IEnumerator GetServers()
	{
		string url = m_url + "?gs=1";
		WWW www = new WWW(url);
		yield return www;
		if (www.error == null)
		{
			m_serverlist = www.text;
		}
	}
}
