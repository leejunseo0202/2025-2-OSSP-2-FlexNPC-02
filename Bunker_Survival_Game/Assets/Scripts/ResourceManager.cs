using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    [Header("재고 (Storage)")]
    [Tooltip("최대 재고량 (예: 100.0)")]
    public int[] maxStock = new int[4];     // Food, Water, Energy
    [Tooltip("현재 재고량")]
    public int[] currentStock = new int[4]; // Food, Water, Energy
    [Tooltip("NPC별 재고 분배량")]
    public int[] sharedStock = new int[13];     // NPC1 = 1,2,3 NPC2 = 4,5,6 NPC3 = 7,8,9 NPC4 = 10,11,12

    void Start()
    {
        for (int i = 0; i < 3; i++)
        {
            maxStock[i] = 100;
            currentStock[i] = 100;
        }
    }
}
