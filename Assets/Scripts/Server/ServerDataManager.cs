using UnityEngine;

public class ServerDataManager : MonoBehaviour
{
    public static int TotalPlayer;

    public static float[] Turret_Rotation;
    public static bool[] Turret_Shoot;
    public static int[] Turret_Player;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            Debug.Log("Total Player : " + TotalPlayer);
            Debug.Log("Turret_Rotation : [" + string.Join(", ", Turret_Rotation) + "]");
            Debug.Log("Turret_Shoot : [" + string.Join(", ", Turret_Shoot) + "]");
            Debug.Log("Turret_Player : [" + string.Join(", ", Turret_Player) + "]");
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            ServerManager.OnBroadcastPhaseChange.Invoke(GCEnumManager.GAMEPHASE.PREPARE);
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            ServerManager.OnBroadcastPhaseChange.Invoke(GCEnumManager.GAMEPHASE.BATTLE);
        }
    }
}