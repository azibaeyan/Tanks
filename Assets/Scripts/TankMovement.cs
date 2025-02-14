
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

    
    private static byte InputTypeCount => (byte)KeyBindings.Count;

    private float MovementInput;
    private float RotationInput;

    
    private readonly Dictionary<TankInputType, bool> InputsStatus = new(InputTypeCount);


    public override void OnNetworkSpawn()
    {
        if (GlobalTankProperties.Singleton == null) 
            Debug.LogError("GlobalTankProperties in null!"); 

        if (IsOwner)
        {
            ResetInput();
            SetNetworkTransformRpc(
                TankSpawnManager.GetSpawnCoordinate(),
                transform.eulerAngles
            );
        }

        name = $"Player {NetworkObjectId} Tank";    
    }

    
    private void Update()
    {
        if (IsOwner)
        {
            ResetInput();
            UpdateInput(out MovementInput, out RotationInput);
        }
    }

    
    private void FixedUpdate() 
    {
        if (IsOwner)
        {
            ProecessInputRpc(MovementInput, RotationInput);
        }
        SyncTransform();
    }


    private void UpdateInput(out float movementInputValue, out float rotationInputValue)
    {
        foreach (var inputType in KeyBindings.Keys) InputsStatus[inputType] = GetInputTypeValue(inputType);

        movementInputValue = ((InputsStatus[TankInputType.Forward] ? 1 : 0) + (InputsStatus[TankInputType.Back] ? -1 : 0)) * Time.fixedDeltaTime;
        rotationInputValue = ((InputsStatus[TankInputType.RightTurn] ? -1 : 0) + (InputsStatus[TankInputType.LeftTurn] ? 1 : 0)) * Time.fixedDeltaTime;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool GetInputTypeValue(TankInputType inputType)
    {
        foreach (KeyCode key in KeyBindings[inputType]) if (Input.GetKey(key)) return true;
        return false;
    }


    void OnCollisionStay2D(Collision2D collision)
    {
        SetNetworkTransformToCurrent();
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetNetworkTransformToCurrent()
    {
        if (IsServer) SetNetworkTransformRpc(transform.position, transform.eulerAngles);
    }


    private void ResetInput()
    {
        for (byte i = 0; i < InputTypeCount; i++) InputsStatus[(TankInputType)i] = false;
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
