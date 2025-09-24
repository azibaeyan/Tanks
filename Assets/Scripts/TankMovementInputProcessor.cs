
using Unity.Netcode;
using UnityEngine;

namespace Tank
{
    [RequireComponent(typeof(TankInput))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class TankMovmentInputProcessor : NetworkBehaviour
    {
        private TankInput _tankInputComponent;
        private Rigidbody2D _tankRigidbody;

        public Vector3 TankForward => -transform.up;


        void Awake()
        {
            _tankInputComponent = GetComponent<TankInput>();
            _tankRigidbody = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            if (IsOwner)
            {
                ProcessInputRpc(_tankInputComponent.MovementInput, _tankInputComponent.RotationInput);
            }
        }


        [Rpc(SendTo.Server)]
        private void ProcessInputRpc(float movementInputValue, float rotationInputValue)
        {
            Vector3 velocity = CalculateVelocityFromMovementInput(movementInputValue);
            float torque = CalculateTorqueFormRotationInput(rotationInputValue);

            _tankRigidbody.linearVelocity = velocity;
            _tankRigidbody.totalTorque = torque;

            /*
                Currently Rotation works along with Rigidbody's Angular Damping
            */
        }


        private Vector3 CalculateVelocityFromMovementInput(float movementInputValue)
        {
            return GlobalTankProperties.Singleton.MovementSpeed * movementInputValue * TankForward;
        }


        private float CalculateTorqueFormRotationInput(float rotationInputValue)
        {
            return GlobalTankProperties.Singleton.RotationSpeed * rotationInputValue;
        }
    }
}
