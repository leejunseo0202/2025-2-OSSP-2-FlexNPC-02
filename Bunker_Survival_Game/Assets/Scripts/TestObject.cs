using UnityEngine;

public class TestObject : MonoBehaviour
{
    public const int TOTAL = 0;
    public const int SHAREDNPC1 = 1;
    public const int SHAREDNPC2 = 2;
    public const int SHAREDNPC3 = 3;
    public const int SHAREDNPC4 = 4;
    public int[] sharedObject = new int[5];

    public const int REFRIGERATOR= 1;
    public const int WATERTANK = 2;  
    public const int OBJECTTYPE3 = 3;
    public const int OBJECTTYPE4 = 4;
    public int objectType = 0;

    void Start()
    {
        sharedObject[TOTAL] = 100;
        for(int i = 1; i < sharedObject.Length; i++)
            sharedObject[i] = 0;
    }
}
