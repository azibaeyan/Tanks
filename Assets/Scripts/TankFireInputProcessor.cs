
using Unity.Netcode;
using UnityEngine;

namespace Tank
{
    [RequireComponent(typeof(TankInput))]
    [RequireComponent(typeof(TankSkin))]
    public class TankFireInputProcessor : NetworkBehaviour
    {
        private readonly NetworkVariable<float> ShootingCoolDownTimer = new();
        private TankInput InputComponent;
        private TankSkin SkinComponent;

        public Vector3 TankForward => -transform.up;


        private void Awake()
        {
            InputComponent = GetComponent<TankInput>();
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
                if (InputComponent.FireInput) FireInputRpc();
            }
        }


        [Rpc(SendTo.Server)]
        private void FireInputRpc()
        {
            if (ShootingCoolDownTimer.Value > 0) return;
            
            ResetCoolDownTimerRpc();

            GlobalTankProperties tankProperties = GlobalTankProperties.Singleton;

            Vector3 bulletSpawnPosition = transform.position + TankForward * tankProperties.BulletSpawnDistance;
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
