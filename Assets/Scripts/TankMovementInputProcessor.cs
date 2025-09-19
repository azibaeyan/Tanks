
using Unity.Netcode;
using UnityEngine;
using System.Runtime.CompilerServices;

namespace Tank
{
    [RequireComponent(typeof(TankInput))]
    [RequireComponent(typeof(NetworkTransformSync))]
    public class TankMovmentInputProcessor : NetworkBehaviour
    {
        private TankInput _tankInputComponent;
        private NetworkTransformSync _transformSyncComponent;

        public Vector3 TankForward => -transform.up;


        void Awake()
        {
            _tankInputComponent = GetComponent<TankInput>();
            _transformSyncComponent = GetComponent<NetworkTransformSync>();
        }

        private void FixedUpdate()
        {
            if (IsOwner)
            {
                // Debug.Log($"Prcessing Input [movement: {_tankInputComponent.MovementInput}, rotation: {_tankInputComponent.RotationInput}]");  // debug 

                if (_tankInputComponent.MovementInput != 0 ||
                    _tankInputComponent.RotationInput != 0)
                    ProcessInputRpc(
                        _tankInputComponent.MovementInput,
                        _tankInputComponent.RotationInput
                    );
            }
        }


        [Rpc(SendTo.Server)]
        private void ProcessInputRpc(float movementInputValue, float rotationInputValue)
        {
            Vector3 positionValue = _transformSyncComponent.Position.Value + GlobalTankProperties.Singleton.MovementSpeed * movementInputValue * TankForward;
            Vector3 rotationEulerValue = new(0, 0, SimplifyEulerRotation(_transformSyncComponent.RotationEuler.Value.z + rotationInputValue * GlobalTankProperties.Singleton.RotationSpeed));

            // Debug.Log($"PositionValue: {positionValue}, RotationValue: {rotationEulerValue}");  // debug

            _transformSyncComponent.SetServerNetworkTransform(positionValue, rotationEulerValue);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float SimplifyEulerRotation(float rotation)
        {
            float value = rotation % 360;
            if (value < 0) value += 360;
            return value;
        }
    }
}
