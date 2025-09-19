using System.Runtime.CompilerServices;
using UnityEngine;

public sealed class GlobalTankProperties : MonoBehaviour
{
    public static GlobalTankProperties Singleton;
    private bool IsMainInstance = false;

    public const string TankTag = "Tank";


    [Header("Properties")]
    public float MovementSpeed;
    public float RotationSpeed;
    public float ShootingCoolDownTimer;
    public float BulletSpawnDistance;
    public float BulletLifeTime;
    
    
    private void Awake()
    {
        ApplySingleton();
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ApplySingleton()
    {
        if (Singleton != null) Destroy(this);
        else
        {
            IsMainInstance = true;
            Singleton = this;
        }
    }


    private void OnDestroy()
    {
        if (IsMainInstance) Singleton = null;
    }
}
