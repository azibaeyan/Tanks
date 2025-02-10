using System.Runtime.CompilerServices;
using UnityEngine;

public sealed class GlobalTankProperties : MonoBehaviour
{
    public static GlobalTankProperties Singleton;
    private static bool Instantiated = false;
    private bool IsMainInstance = false;


    [Header("Properties")]
    public float MovementSpeed;
    public float RotationSpeed;
    
    
    private void Awake()
    {
        Singleton = this;

        ApplySingleton();
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ApplySingleton()
    {
        if (Instantiated) Destroy(this);
        else
        {
            IsMainInstance = Instantiated = true;
            DontDestroyOnLoad(this);
        }
    }

    
    private void OnDestroy() 
    {
        if (IsMainInstance) Instantiated = false;
    }
}
