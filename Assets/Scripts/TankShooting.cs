
using Unity.Netcode;
using UnityEngine;

namespace Tank
{
    public class TankShooting : NetworkBehaviour
    {
        private readonly NetworkVariable<float> ShootingCoolDownTimer = new();
        private TankInput InputComponent;
        private TankMovement MovementComponent;
        private TankSkin SkinComponent;


        private void Awake()
        {
            InputComponent = GetComponent<TankInput>();
            MovementComponent = GetComponent<TankMovement>();
            SkinComponent = GetComponent<TankSkin>();
        }


        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                ResetCoolDownTimerRpc();
            }
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
                ResetCoolDownTimerRpc();

                GlobalTankProperties tankProperties = GlobalTankProperties.Singleton;

                Vector3 bulletSpawnPosition = transform.position + MovementComponent.TankForward * tankProperties.BulletSpawnDistance;
                Quaternion bulletSpawnRotation = Quaternion.Euler(transform.rotation.eulerAngles - new Vector3(0, 0, 180));

                GameObject instantiatedBullet = Instantiate(
                    TankSpawnManager.Singleton.BulletPrefab,
                    bulletSpawnPosition, 
                    bulletSpawnRotation
                );

                TankBullet bulletComponent = instantiatedBullet.GetComponent<TankBullet>();
                bulletComponent.SetNetworkPosition(bulletSpawnPosition);

                bulletComponent.SetSkinIndex(SkinComponent.SkinIndex.Value);

                NetworkObject bulletNetworkObject = instantiatedBullet.GetComponent<NetworkObject>();
                bulletNetworkObject.Spawn();

            }
        }

        
        [Rpc(SendTo.Server)]
        private void ProcessTimerRpc()
        {
            ShootingCoolDownTimer.Value -= Time.fixedDeltaTime;
        }
        

        [Rpc(SendTo.Server)]
        private void ResetCoolDownTimerRpc()
        {   
            ShootingCoolDownTimer.Value = GlobalTankProperties.Singleton.ShootingCoolDownTimer;
        }   
    }
}
