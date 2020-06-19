using Steamworks;
using UnityEngine;

public class SteamAuth : MonoBehaviour
{
	private void Start()
	{
		if (SteamManager.Initialized)
		{
			CSteamID steamID = SteamUser.GetSteamID();
			ulong steamID2 = steamID.m_SteamID;
			string personaName = SteamFriends.GetPersonaName();
			Debug.Log(personaName + " id " + steamID2);
		}
	}
}
