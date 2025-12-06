using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    [Header("재고 (Storage)")]
    [Tooltip("최대 재고량 (예: 100.0)")]
    public int[] maxStock = new int[3];     // Food, Water, Energy
    [Tooltip("현재 재고량")]
    public int[] currentStock = new int[3]; // Food, Water, Energy
    [Tooltip("NPC별 재고 분배량")]
    public int[,] sharedStock = new int[3, 5];     // NPC1 = 1, NPC2 = 2, NPC3 = 3, NPC4 = 4

    void Start()
    {
        for (int i = 0; i < 3; i++)
        {
            maxStock[i] = 100;
            currentStock[i] = 100;
        }
    }
}
