
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;


[DisallowMultipleComponent]
public class TankMovement : NetworkBehaviour
{
    [HideInInspector] internal readonly NetworkVariable<Vector3> TankPosition = new();
    [HideInInspector] internal readonly NetworkVariable<Vector3> TankRotation = new();

    private Vector3 TankForward => transform.up * -1;

    private static readonly Dictionary<TankInputType, KeyCode[]> KeyBindings = new()
    {
        [TankInputType.Forward]   = new KeyCode[]{ KeyCode.W, KeyCode.UpArrow },
        [TankInputType.Back]      = new KeyCode[]{ KeyCode.S, KeyCode.DownArrow },
        [TankInputType.RightTurn] = new KeyCode[]{ KeyCode.D, KeyCode.RightArrow },
        [TankInputType.LeftTurn]  = new KeyCode[]{ KeyCode.A, KeyCode.LeftArrow },
        [TankInputType.Fire]      = new KeyCode[]{ KeyCode.F, KeyCode.E }
    };

    
    private readonly Dictionary<TankInputType, bool> Inputs = new(5);


    public override void OnNetworkSpawn()
    {
        if (GlobalTankProperties.Singleton == null) Debug.LogWarning("GlobalTankProperties in null!"); 

        ResetInput();
    }

    
    private void Update()
    {
        if (!IsOwner) return;
        ResetInput();
        UpdateInput();
    }

    
    private void FixedUpdate() 
    {
        if (IsOwner)
        {
            ProecessInput();
        }
        SyncTransform();
    }


    private void UpdateInput()
    {
        foreach (var inputType in KeyBindings.Keys) Inputs[inputType] = GetInputTypeValue(inputType);
    }

    
    private void ResetInput()
    {
        for (byte i = 0; i < 5; i++) Inputs[(TankInputType)i] = false;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool GetInputTypeValue(TankInputType inputType)
    {
        foreach (KeyCode key in KeyBindings[inputType]) if (Input.GetKey(key)) return true;
        return false;
    }


    private void ProecessInput()
    {
        float movementInputValue = ((Inputs[TankInputType.Forward] ? 1 : 0) + (Inputs[TankInputType.Back] ? -1 : 0)) * Time.fixedDeltaTime;
        float rotationInputValue = ((Inputs[TankInputType.RightTurn] ? -1 : 0) + (Inputs[TankInputType.LeftTurn] ? 1 : 0)) * Time.fixedDeltaTime;
        
        UpdateTransformServerRpc(
            TankPosition.Value + movementInputValue * GlobalTankProperties.Singleton.MovementSpeed * TankForward,
            new Vector3(0, 0, TankRotation.Value.z + rotationInputValue * GlobalTankProperties.Singleton.RotationSpeed)
        );
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Rpc(SendTo.Server)]
    internal void UpdateTransformServerRpc(Vector3 position, Vector3 eulerRotation, RpcParams rpcParams = default)
    {
        TankPosition.Value = position;
        TankRotation.Value = eulerRotation;
    }

    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SyncTransform()
    {
        transform.SetPositionAndRotation(
            TankPosition.Value, 
            Quaternion.Euler(TankRotation.Value)
        );
    }


    private enum TankInputType
    {
        Forward,
        Back,
        RightTurn,
        LeftTurn,
        Fire
    }
}
