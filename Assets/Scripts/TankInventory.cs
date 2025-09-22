
using Unity.Netcode;
using UnityEngine;

namespace Tank
{
    [RequireComponent(typeof(TankFireInputProcessor))]
    public class TankInventory : NetworkBehaviour
    {
        private readonly NetworkVariable<byte> bulletCount = new(0);

        public byte BulletCount => bulletCount.Value;


        // Cache
        private const float _coolDownTillReloadTime = 2f;
        private const float _bulletLoadDuration = 0.5f;

        private const byte _maxBulletCount = 7;

        private float _lastBulletAddTime = 0f;
        private float _lastFireTime = 0f;


        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn(); 
            if (!IsOwner) return;
            RegisterFireCallbackRpc();
        }


        [Rpc(SendTo.Server)]
        private void RegisterFireCallbackRpc()
        {
            var fireInputProcessorComponent = GetComponent<TankFireInputProcessor>();
            fireInputProcessorComponent.OnFireCallback += OnTankFire;
        }


        private void FixedUpdate()
        {
            if (!IsServer) return;

            if (TimeToLoadBullet) LoadBullet();
        }


        private void OnTankFire()
        {
            _lastFireTime = Time.time;
            bulletCount.Value--;
        }


        private bool TimeToLoadBullet
        {
            get
            {    
                if (bulletCount.Value == _maxBulletCount) return false;

                if (_lastFireTime > _lastBulletAddTime)
                {
                    return Time.time > _lastFireTime + _coolDownTillReloadTime;
                }
                return Time.time > _lastBulletAddTime + _bulletLoadDuration;
            }
        }


        private void LoadBullet()
        {
            bulletCount.Value += 1;
            _lastBulletAddTime = Time.time;
        }
    }
}
