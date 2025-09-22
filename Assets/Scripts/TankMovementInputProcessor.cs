
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
                bool movementInputReceived = _tankInputComponent.MovementInput != 0;
                bool rotationInputRecieved = _tankInputComponent.RotationInput != 0;

                if (movementInputReceived || rotationInputRecieved)
                {
                    if (movementInputReceived && rotationInputRecieved)
                        ProcessInputRpc(_tankInputComponent.MovementInput, _tankInputComponent.RotationInput);
                    else if (movementInputReceived)
                        ProcessPositionInputRpc(_tankInputComponent.MovementInput);
                    else
                        ProcessRotationInputRpc(_tankInputComponent.RotationInput);
                }
            }
        }


        [Rpc(SendTo.Server)]
        private void ProcessInputRpc(float movementInputValue, float rotationInputValue)
        {
            Vector3 positionValue = CalculatePositionValueFromInput(movementInputValue);
            Vector3 rotationEulerValue = CalculateRotationValueFromInput(rotationInputValue);

            _transformSyncComponent.SetNetworkTransformRpc(positionValue, rotationEulerValue);
        }


        [Rpc(SendTo.Server)]
        private void ProcessPositionInputRpc(float movementInputValue)
        {
            Vector3 positionValue = CalculatePositionValueFromInput(movementInputValue);
            _transformSyncComponent.SetNetworkPositionRpc(positionValue);
        }


        [Rpc(SendTo.Server)]
        private void ProcessRotationInputRpc(float rotationInputValue)
        {
            Vector3 rotationEulerValue = CalculateRotationValueFromInput(rotationInputValue);
            _transformSyncComponent.SetNetworkRotationEulerRpc(rotationEulerValue);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Vector3 CalculatePositionValueFromInput(float movementInputValue)
        {
            return _transformSyncComponent.Position.Value + GlobalTankProperties.Singleton.MovementSpeed * movementInputValue * TankForward;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Vector3 CalculateRotationValueFromInput(float rotationInputValue)
        {
            return new(0, 0, SimplifyEulerRotation(_transformSyncComponent.RotationEuler.Value.z + rotationInputValue * GlobalTankProperties.Singleton.RotationSpeed));
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
