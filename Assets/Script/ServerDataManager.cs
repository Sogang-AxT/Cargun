using UnityEngine;

public class ServerDataManager : MonoBehaviour
{
    public static int TotalPlayer;

    public static int Ship_Shield;
    public static int Ship_Attack;
    public static int[] Start_Item;

    public static float[] Turret_Rotation;
    public static bool[] Turret_Shoot;
    public static int[] Turret_Player;

    public void get_point(int PlayerNum)
    {
        //
    }

    public void set_phase(int phase)
    {
        //
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            Debug.Log("Total Player : " + TotalPlayer);
            Debug.Log("Ship_Shield : " + Ship_Shield);
            Debug.Log("Ship_Attack : " + Ship_Attack);
            Debug.Log("Start_Item : [" + string.Join(", ", Start_Item) + "]");
            Debug.Log("Turret_Rotation : [" + string.Join(", ", Turret_Rotation) + "]");
            Debug.Log("Turret_Shoot : [" + string.Join(", ", Turret_Shoot) + "]");
            Debug.Log("Turret_Player : [" + string.Join(", ", Turret_Player) + "]");
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            GetComponent<ServerManager>().BroadcastPhaseChange("prepare");
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            GetComponent<ServerManager>().BroadcastPhaseChange("battle");
        }
    }
}
