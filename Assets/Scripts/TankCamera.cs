
using Unity.Cinemachine;
using Unity.Netcode;

public class TankCamera : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        CinemachineCamera camera = FindFirstObjectByType<CinemachineCamera>();
        camera.Follow = transform;
    }
}
