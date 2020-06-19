using Lidgren.Network;

public class VerificationData
{
	public string name = string.Empty;

	public string pwhash = string.Empty;

	public ulong id;

	public bool valid;

	public bool unknownAccount;

	public bool inProgress;

	public string error = string.Empty;

	public NetConnection connection;
}
