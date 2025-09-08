using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;

public class TankHealth : NetworkBehaviour
{
    [SerializeField] private float _healthAmount;
    private readonly NetworkVariable<float> HealthAmount = new();


    public override void OnNetworkSpawn()
    {
        if (IsServer) HealthAmount.Value = _healthAmount;
    }


    public void OnDamage(float damageAmount) 
    {
        DecreaseHealthRpc(damageAmount);

        CheckHealth();
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CheckHealth()
    {
        if (IsServer)
        {
            if (HealthAmount.Value <= 0) GetComponent<NetworkObject>().Despawn(true);
        }
    }


    [Rpc(SendTo.Server)]
    private void DecreaseHealthRpc(float damageAmount)
    {
        HealthAmount.Value -= damageAmount;
    }
}  
