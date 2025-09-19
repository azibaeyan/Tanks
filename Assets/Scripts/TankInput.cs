
using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using System.Runtime.CompilerServices;

namespace Tank
{
    [DisallowMultipleComponent]
    public class TankInput : NetworkBehaviour
    {
        private float _movementInput;
        private float _rotationInput;
        public float MovementInput => _movementInput;
        public float RotationInput => _rotationInput;


        private bool _fireInput;
        public bool FireInput => _fireInput;
        

        public enum TankInputType
        {
            Forward,
            Back,
            RightTurn,
            LeftTurn,
            Fire
        }


        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            ResetInput();
        }


        private void Update()
        {
            if (IsOwner)
            {
                ResetInput();

                UpdateMovementInput(
                    out _movementInput,
                    out _rotationInput,
                    out _fireInput
                );
            }
        }


        public void ResetInput()
        {
            for (byte i = 0; i < InputTypeCount; i++) InputsStatus[(TankInputType)i] = false;
        }


        private void UpdateMovementInput(out float movementInputValue, out float rotationInputValue, out bool fireInput)
        {
            foreach (var inputType in KeyBindings.Keys) InputsStatus[inputType] = GetInputTypeValue(inputType);

            movementInputValue = ((InputsStatus[TankInputType.Forward] ? 1 : 0) + (InputsStatus[TankInputType.Back] ? -1 : 0)) * Time.fixedDeltaTime;
            rotationInputValue = ((InputsStatus[TankInputType.RightTurn] ? -1 : 0) + (InputsStatus[TankInputType.LeftTurn] ? 1 : 0)) * Time.fixedDeltaTime;
            fireInput = InputsStatus[TankInputType.Fire];
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool GetInputTypeValue(TankInputType inputType)
        {
            foreach (KeyCode key in KeyBindings[inputType]) if (Input.GetKey(key)) return true;
            return false;
        }


        public static readonly Dictionary<TankInputType, KeyCode[]> KeyBindings = new()
        {
            [TankInputType.Forward]   = new KeyCode[]{ KeyCode.W, KeyCode.UpArrow },
            [TankInputType.Back]      = new KeyCode[]{ KeyCode.S, KeyCode.DownArrow },
            [TankInputType.RightTurn] = new KeyCode[]{ KeyCode.D, KeyCode.RightArrow },
            [TankInputType.LeftTurn]  = new KeyCode[]{ KeyCode.A, KeyCode.LeftArrow },
            [TankInputType.Fire]      = new KeyCode[]{ KeyCode.F, KeyCode.E }
        };

        public static byte InputTypeCount => (byte)KeyBindings.Count;

        private readonly Dictionary<TankInputType, bool> InputsStatus = new(InputTypeCount);
    }
}
