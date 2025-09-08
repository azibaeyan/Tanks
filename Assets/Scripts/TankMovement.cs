
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;

namespace Tank
{
    [DisallowMultipleComponent]
    public class TankMovement : NetworkBehaviour
    {
        [HideInInspector] internal readonly NetworkVariable<Vector3> TankPosition = new();
        [HideInInspector] internal readonly NetworkVariable<Vector3> TankRotation = new();
        [HideInInspector] internal readonly NetworkVariable<bool> InstantPosition = new();

        [SerializeField] private BoxCollider2D Collider;

        public Vector3 TankForward => transform.up * -1;

        private TankInput TankInputComponent;


        void Awake()
        {
            TankInputComponent = GetComponent<TankInput>();
        }


        public override void OnNetworkSpawn()
        {
            if (GlobalTankProperties.Singleton == null) 
                Debug.LogError("GlobalTankProperties in null!"); 

            if (IsOwner)
            {
                TankInputComponent.ResetInput();
                SetNetworkTransformRpc(transform.position, transform.eulerAngles);
            }

            name = $"Player {NetworkObjectId} Tank";
        }

        
            private void FixedUpdate() 
        {
            if (IsOwner)
            {
                ProecessInputRpc(
                    TankInputComponent.MovementInput, 
                    TankInputComponent.RotationInput
                );
            }
            SyncTransform();
        }


        void OnCollisionStay2D(Collision2D collision)
        {
            SetServerTransformToCurrent();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetServerTransformToCurrent()
        {
            if (IsServer) SetNetworkTransformRpc(transform.position, transform.eulerAngles);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Rpc(SendTo.Server)]
        private void ProecessInputRpc(float movementInputValue, float rotationInputValue)
        {
            SetNetworkTransformRpc(
                TankPosition.Value + movementInputValue * GlobalTankProperties.Singleton.MovementSpeed * TankForward,
                new Vector3(0, 0, TankRotation.Value.z + rotationInputValue * GlobalTankProperties.Singleton.RotationSpeed)
            );
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Rpc(SendTo.Server)]
        internal void SetNetworkTransformRpc(Vector3 position, Vector3 eulerRotation)
        {
            TankPosition.Value = position;
            TankRotation.Value = eulerRotation;
        }

        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SyncTransform()
        {
            Vector3 targetPosition = TankPosition.Value;
            Quaternion targetRotation = Quaternion.Euler(TankRotation.Value);
            
            transform.SetPositionAndRotation(
                Vector3.MoveTowards(transform.position, targetPosition, .1f), 
                Quaternion.RotateTowards(transform.rotation, targetRotation, 10)
            );
        }
    }
}
