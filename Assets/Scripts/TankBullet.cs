
using System.Runtime.CompilerServices;
using Unity.Burst.Intrinsics;
using Unity.Netcode;
using UnityEngine;

namespace Tank
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(NetworkTransformSync))]
    public class TankBullet : NetworkBehaviour
    {
        private Vector3 Forward => transform.up;

        [HideInInspector] public NetworkTransformSync TransformSyncComponent;

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
            Vector3 offest = Speed * Time.fixedDeltaTime * Forward;

            TransformSyncComponent.SetNetworkPositionRpc(
                TransformSyncComponent.Position.Value + offest
            );
        }
    }
}
