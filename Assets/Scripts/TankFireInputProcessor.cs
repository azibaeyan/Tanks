
using System;
using Unity.Netcode;
using UnityEngine;

namespace Tank
{
    [RequireComponent(typeof(TankInput))]
    [RequireComponent(typeof(TankSkin))]
    [RequireComponent(typeof(TankInventory))]
    public class TankFireInputProcessor : NetworkBehaviour
    {
        private readonly NetworkVariable<float> ShootingCoolDownTimer = new();
        private TankInput _inputComponent;
        private TankSkin _skinComponent;
        private TankInventory _tankInventory;

        public Vector3 TankForward => -transform.up;

        public Action OnFireCallback = null;


        private void Awake()
        {
            _inputComponent = GetComponent<TankInput>();
            _skinComponent = GetComponent<TankSkin>();
            _tankInventory = GetComponent<TankInventory>();
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
                if (_inputComponent.FireInput) FireInputRpc();
            }
        }


        [Rpc(SendTo.Server)]
        private void FireInputRpc()
        {
            if (ShootingCoolDownTimer.Value > 0) return;

            if (_tankInventory.BulletCount == 0) return;

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
            bulletComponent.TransformSyncComponent.SetNetworkPositionRpc(bulletSpawnPosition);

            bulletComponent.SetSkinIndex(_skinComponent.SkinIndex.Value);

            NetworkObject bulletNetworkObject = instantiatedBullet.GetComponent<NetworkObject>();
            bulletNetworkObject.Spawn();

            OnFireCallback?.Invoke();
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
