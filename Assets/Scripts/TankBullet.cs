
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;

namespace Tank
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(NetworkTransformSync))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class TankBullet : NetworkBehaviour
    {
        private Vector3 Forward => transform.up;

        internal NetworkTransformSync TransformSyncComponent;

        private Rigidbody2D _bulletRigidbody;

        private readonly NetworkVariable<byte> SkinIndex = new();

        public Vector3 SpawnPosition;

        [SerializeField] private float Speed;
        [SerializeField] private float DamageAmount;

        private float LifeTime;


        public void SetSkinIndex(byte index)
        {
            SkinIndex.Initialize(this);
            SkinIndex.Value = index;
        }


        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                LifeTime = GlobalTankProperties.Singleton.BulletLifeTime;
            }

            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            renderer.sprite = TankSpawnManager.Singleton.AmmoSprites[SkinIndex.Value];
        }


        private void Awake()
        {
            TransformSyncComponent = GetComponent<NetworkTransformSync>();
            _bulletRigidbody = GetComponent<Rigidbody2D>();
        }


        private void FixedUpdate()
        {
            CheckBulletLifeTime();

            if (IsSpawned && IsServer) MoveForward();
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

                NetworkObject.Despawn(true);
            }
        }


        private void MoveForward()
        {
            Vector3 velocity = Speed * Forward;
            _bulletRigidbody.linearVelocity = velocity;
        }
    }
}
