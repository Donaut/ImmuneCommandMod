using UnityEngine;

public class TutorialMessages : MonoBehaviour
{
	public enum eMsg
	{
		ePickupItem,
		eEatFood,
		eDriveCar,
		eGatherResource,
		eAttackEnemy,
		eStarving,
		eChat,
		eBuildBuilding,
		ePlayerProfile,
		eShovel,
		eBuildBuilding2,
		eMsgCount
	}

	public float m_displayIntervall = 120f;

	private float[] m_nextMsgShowTime = new float[11];

	private MessageBarGUI m_msgBar;

	private LidClient m_client;

	private InventoryGUI m_inventoryGui;

	private void Start()
	{
		m_client = (LidClient)Object.FindObjectOfType(typeof(LidClient));
		m_msgBar = GetComponent<MessageBarGUI>();
		m_inventoryGui = (InventoryGUI)Object.FindObjectOfType(typeof(InventoryGUI));
	}

	private void Update()
	{
		if (!(null != m_client))
		{
			return;
		}
		if (string.Empty != m_client.m_notificationMsg)
		{
			m_msgBar.DisplayMessage(m_client.m_notificationMsg, 1000);
			m_client.m_notificationMsg = string.Empty;
		}
		if (!(null != m_client) || PlayerPrefs.GetInt("prefHints", 1) != 1 || !(0f < m_client.GetHealth()) || !(Time.timeSinceLevelLoad > 20f))
		{
			return;
		}
		int num = 0;
		while (true)
		{
			if (num < 11)
			{
				if (Time.time > m_nextMsgShowTime[num] && DisplayMessage((eMsg)num))
				{
					break;
				}
				num++;
				continue;
			}
			return;
		}
		m_nextMsgShowTime[num] = Time.time + m_displayIntervall;
	}

	private bool DisplayMessage(eMsg a_msg)
	{
		bool flag = false;
		int a_prio = 100;
		Vector3 pos = m_client.GetPos();
		switch (a_msg)
		{
		case eMsg.eEatFood:
			flag = (m_inventoryGui.IsVisible() && m_inventoryGui.HasFood());
			break;
		case eMsg.eBuildBuilding:
			flag = (m_inventoryGui.IsVisible() && m_inventoryGui.HasBuilding());
			break;
		case eMsg.eBuildBuilding2:
		{
			int num;
			if (!m_inventoryGui.IsVisible())
			{
				ItemDef itemDef = Items.GetItemDef(m_client.GetHandItem());
				num = ((itemDef.buildingIndex > 0) ? 1 : 0);
			}
			else
			{
				num = 0;
			}
			flag = ((byte)num != 0);
			break;
		}
		case eMsg.eShovel:
			flag = (m_inventoryGui.IsVisible() && m_inventoryGui.HasItemType(109));
			break;
		case eMsg.eStarving:
			flag = (0f == m_client.GetEnergy());
			a_prio = 110;
			break;
		case eMsg.ePickupItem:
		{
			RemoteItem nearestItem = m_client.GetNearestItem(pos);
			flag = (null != nearestItem && 25f > (nearestItem.transform.position - pos).sqrMagnitude);
			break;
		}
		case eMsg.eGatherResource:
		{
			RemoteBuilding nearestResource = m_client.GetNearestResource(pos);
			flag = (null != nearestResource && 25f > (nearestResource.transform.position - pos).sqrMagnitude);
			break;
		}
		case eMsg.eAttackEnemy:
		{
			RemoteCharacter nearestNpc = m_client.GetNearestNpc(pos);
			flag = (null != nearestNpc && 36f > (nearestNpc.transform.position - pos).sqrMagnitude);
			break;
		}
		case eMsg.eDriveCar:
		{
			RemoteCharacter nearestCharacter2 = m_client.GetNearestCharacter(pos, true);
			flag = (null != nearestCharacter2 && 36f > (nearestCharacter2.transform.position - pos).sqrMagnitude);
			break;
		}
		case eMsg.eChat:
		case eMsg.ePlayerProfile:
		{
			RemoteCharacter nearestCharacter = m_client.GetNearestCharacter(pos);
			flag = (null != nearestCharacter && 49f > (nearestCharacter.transform.position - pos).sqrMagnitude);
			break;
		}
		}
		if (flag)
		{
			m_msgBar.DisplayMessage(LNG.Get("TUTORIAL_MESSAGE_" + (int)a_msg), a_prio);
		}
		return flag;
	}
}
