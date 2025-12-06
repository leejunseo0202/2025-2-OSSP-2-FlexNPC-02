using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    public const int HUNGER = 0;
    public const int TOILET = 1;
    public const int SOCIAL = 2;
    public const int HYGIENE = 3;
    public const int FUN = 4;
    public const int ENERGY = 5;

    public const int NPC1 = 1;
    public const int NPC2 = 2;
    public const int NPC3 = 3;
    public const int NPC4 = 4;
    public int npcType = 0;

    public float[] NPCstatus = new float[6];
    private void Start()
    {
        for (int i = 0; i < NPCstatus.Length; i++)
            NPCstatus[i] = 100.0f;
    }

    void Update()
    {
        if (NPCstatus[HUNGER] <= 0)
            NPCstatus[HUNGER] = 0;
        else
            NPCstatus[HUNGER] -= Time.deltaTime * 5.0f;

        NPCstatus[TOILET] -= Time.deltaTime * 3.0f;
        NPCstatus[SOCIAL] -= Time.deltaTime * 2.0f;
    }
}
