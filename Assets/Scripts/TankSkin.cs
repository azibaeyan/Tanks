using Unity.Netcode;
using UnityEngine;

public class TankSkin : NetworkBehaviour
{
    private readonly NetworkVariable<byte> SkinIndex = new();
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            if (TankSpawnManager.Singleton == null)
            {
                Debug.LogError("TankSpawnManager Singleton is null!");  // debug
                return;
            }
            SkinIndex.Value = TankSpawnManager.Singleton.GetRandomSkinIndex();
        }

        Debug.Log($"Skin index: {SkinIndex.Value}");  // debug

        GetComponent<SpriteRenderer>().sprite = TankSpawnManager.Singleton.ReserveSkin(SkinIndex.Value);
    }
}
