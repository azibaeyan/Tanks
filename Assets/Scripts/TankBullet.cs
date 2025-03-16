
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;

namespace Tank
{
    public class TankBullet : NetworkBehaviour
    {
        private Vector3 Forward => transform.up;
        
        [SerializeField] private float Speed;

        internal readonly NetworkVariable<Vector3> NetworkPosition = new();


        private void FixedUpdate()
        {
            if (IsOwner) MoveForward();

            SyncTransform();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MoveForward() => transform.position += Speed * Time.fixedDeltaTime * Forward;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SyncTransform()
        {
            transform.position = NetworkPosition.Value;
        }
    }
}
