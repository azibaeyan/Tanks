
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class NetworkTransformSync : NetworkBehaviour
{
    [HideInInspector]
    public readonly NetworkVariable<Vector3> Position = new(), RotationEuler = new();

    private Vector3? _spawnPosition = null, _spawnRotationEuler = null;

    public Vector3? SpawnPosition
    {
        get => _spawnPosition;
        set
        {
            if (IsSpawned)
            {
                Debug.LogWarning("The object is already spawned. Settting SpawnPosition has no effect.");
                return;
            }

            _spawnPosition = value;
        }
    }

    public Vector3? SpawnRotationEuler
    {
        get => _spawnRotationEuler;
        set
        {
            if (IsSpawned)
            {
                Debug.LogWarning("The object is already spawned. Settting SpawnRotationEuler has no effect.");
                return;
            }

            _spawnRotationEuler = value;
        }
    }


    [SerializeField] private bool _localTransform;

    [SerializeField] private float _positionInterpolationMaxDelta;
    [SerializeField] private float _rotationInterpolationQuaternionMaxDelta;

    [SerializeField] private bool _positionSync = true;
    [SerializeField] private bool _rotationSync = true;

    [SerializeField] private bool _enableColliderEffect;
    public bool EnableColliderEffect
    {
        get => _enableColliderEffect;
        set
        {
            if (IsSpawned)
            {
#if UNITY_EDITOR
                Debug.LogError("Modifiying EnableColliderEffect after Object spawn is not allowed!");
#endif
                return;
            }

            _enableColliderEffect = value;
        }
    }


    public override void OnNetworkSpawn()
    {
        SetOnSpawnPosition();
        SetNetworkPositionFromLocal();
    }


    private void FixedUpdate()
    {
        InterpolateClientTransformSync();
    }


    void OnCollisionStay(Collision collision)
    {
        if (_enableColliderEffect) SetNetworkTransformFromLocal();
    }


    void OnCollisionStay2D(Collision2D collision)
    {
        if (_enableColliderEffect) SetNetworkTransformFromLocal();
    }


    private void SetOnSpawnPosition()
    {
        if (!IsOwner) return;

        if (SpawnPosition.HasValue)
            SetNetworkPositionRpc(SpawnPosition.Value);
        else
            SetNetworkPositionFromLocal();

        if (SpawnRotationEuler.HasValue)
            SetNetworkRotationEulerRpc(SpawnRotationEuler.Value);
        else
            SetNetworkRotationFromLocal();
    }


    [Rpc(SendTo.Server)]
    public void SetNetworkTransformRpc(Vector3 position, Vector3 rotationEuler)
    {
        if (_positionSync) Position.Value = position;
        if (_rotationSync) RotationEuler.Value = rotationEuler;
    }


    [Rpc(SendTo.Server)]
    public void SetNetworkTransformRpc(Vector3 position, Quaternion rotationQuaternion)
    {
        if (_positionSync) Position.Value = position;
        if (_rotationSync) RotationEuler.Value = rotationQuaternion.eulerAngles;
    }


    [Rpc(SendTo.Server)]
    public void SetNetworkPositionRpc(Vector3 position) => Position.Value = position;

    [Rpc(SendTo.Server)]
    public void SetNetworkRotationEulerRpc(Vector3 rotationEuler) => RotationEuler.Value = rotationEuler;

    [Rpc(SendTo.Server)]
    public void SetNetworkRotationQuaternionRpc(Quaternion rotationQuaternion) => RotationEuler.Value = rotationQuaternion.eulerAngles;


    public void SetNetworkTransformFromLocal()
    {
        if (_positionSync) SetNetworkPositionFromLocal();
        if (_rotationSync) SetNetworkRotationFromLocal();
    }


    public void SetNetworkPositionFromLocal()
    {
        if (!IsServer) return;
        SetNetworkPositionRpc(
            _localTransform ?
                transform.localPosition : 
                transform.position
        );
    }


    public void SetNetworkRotationFromLocal()
    {
        if (!IsServer) return;
        SetNetworkRotationQuaternionRpc(
            _localTransform ? 
                transform.localRotation : 
                transform.rotation
        );
    }


    private void InterpolateClientTransformSync()
    {
        if (_positionSync)
        {
            Vector3 positionValue = Vector3.MoveTowards(
                transform.position,
                Position.Value,
                _positionInterpolationMaxDelta
            );

            if (_localTransform) transform.localPosition = positionValue;
            else transform.position = positionValue;
        }

        if (_rotationSync)
        {
            Quaternion rotationValue = Quaternion.Lerp(
                transform.rotation,
                Quaternion.Euler(RotationEuler.Value),
                _rotationInterpolationQuaternionMaxDelta
            );

            if (_localTransform) transform.localRotation = rotationValue;
            else transform.rotation = rotationValue;
        }
    }
}
