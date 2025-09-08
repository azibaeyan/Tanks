
using Unity.Netcode;
using UnityEngine;

namespace Tank
{
    public class TankSkin : NetworkBehaviour
    {
        public readonly NetworkVariable<byte> SkinIndex = new();
        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                if (TankSpawnManager.Singleton == null)
                {
                    Debug.LogError("TankSpawnManager Singleton is null!");  
                    return;
                }
                SkinIndex.Value = TankSpawnManager.Singleton.GetRandomSkinIndex();
            }

            GetComponent<SpriteRenderer>().sprite = TankSpawnManager.Singleton.ReserveSkin(SkinIndex.Value);
        }
    }
}
