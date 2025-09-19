
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;

namespace Tank
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(NetworkObject))]
    public class TankBullet : NetworkBehaviour
    {
        private Vector3 Forward => transform.up;

        private readonly NetworkVariable<byte> SkinIndex = new();

        public Vector3 SpawnPosition;
        
        [SerializeField] private float Speed;
        [SerializeField] private float DamageAmount;

        private float LifeTime;

        internal readonly NetworkVariable<Vector3> NetworkPosition = new();


        public void SetSkinIndex(byte index) 
        {
            SkinIndex.Initialize(this);
            SkinIndex.Value = index;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetNetworkPosition(Vector3 position)
        {
            NetworkPosition.Initialize(this);
            NetworkPosition.Value = position;
        }


        public override void OnNetworkSpawn()
        {
            if (IsServer) 
            {
                LifeTime = GlobalTankProperties.Singleton.BulletLifeTime;
                transform.position = NetworkPosition.Value;
            }

            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            renderer.sprite = TankSpawnManager.Singleton.AmmoSprites[SkinIndex.Value];
        }


        private void Start()
        {
            SyncTransform();
        }


        private void FixedUpdate()
        {
            CheckBulletLifeTime();

            if (IsSpawned && IsOwner) MoveForwardRpc();

            SyncTransform();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckBulletLifeTime()
        {
            if (IsServer)
            {
                LifeTime -= Time.fixedDeltaTime;
                if (LifeTime <= 0) GetComponent<NetworkObject>().Despawn(true);
            }
        }


        void OnCollisionEnter2D(Collision2D collision)
        {
            if (IsServer)
            {
                if (collision.gameObject.CompareTag(GlobalTankProperties.TankTag)) 
                    collision.gameObject.GetComponent<TankHealth>().OnDamage(DamageAmount);

                GetComponent<NetworkObject>().Despawn(true);
            }
        }


        [Rpc(SendTo.Server)]
        private void MoveForwardRpc() => NetworkPosition.Value += Speed * Time.fixedDeltaTime * Forward;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SyncTransform()
        {
            transform.position = NetworkPosition.Value;
        }
    }
}
