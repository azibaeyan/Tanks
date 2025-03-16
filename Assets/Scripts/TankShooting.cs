using System;
using Unity.Netcode;
using UnityEngine;

namespace Tank
{
    public class TankShooting : NetworkBehaviour
    {
        private readonly NetworkVariable<float> ShootingCoolDownTimer = new();
        private TankInput InputComponent;


        private void Awake()
        {
            InputComponent = GetComponent<TankInput>();
        }


        private void FixedUpdate()
        {
            if (IsOwner) 
            {
                ProcessTimerRpc();
                ProcessInputRpc(InputComponent.FireInput);
            }
        }


        [Rpc(SendTo.Server)]
        private void ProcessInputRpc(bool fireInput)
        {
            if (ShootingCoolDownTimer.Value > 0) return;

            if (fireInput)
            {
                NetworkObject bulletObjectPrefab = TankSpawnManager.Singleton.BulletPrefab.GetComponent<NetworkObject>();
                
                bulletObjectPrefab.InstantiateAndSpawn(
                    NetworkManager,
                    OwnerClientId
                );
            }
        }

        
        [Rpc(SendTo.Server)]
        private void ProcessTimerRpc()
        {
            ShootingCoolDownTimer.Value = Math.Min(ShootingCoolDownTimer.Value - Time.fixedDeltaTime, 0);
        }
    }
}
