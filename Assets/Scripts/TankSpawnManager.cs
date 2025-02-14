
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class TankSpawnManager : NetworkBehaviour
{
    public static TankSpawnManager Singleton { get; private set; }
    private bool _instantiated = false;
    private bool _isMainInstance = false;


    private NetworkVariable<ulong> SkinsTakenData = new(0);
    public List<Sprite> Skins;
    private BoolList SkinsTaken;


    private void OnAwake()
    {
        ApplySingleton();
    }


    private byte GetRandomSkinIndex()
    {
        byte availabeSkinsCount = (byte)(SkinsTaken.Count - SkinsTaken.TrueValuesCount);
        byte skinOrder = (byte)Math.Floor(Random.Range(0.0001f , availabeSkinsCount - Mathf.Epsilon));
        for (byte i = 0; i < SkinsTaken.Count; i++) 
        {
            if (!SkinsTaken[i])
            {
                if (skinOrder == 0) 
                {
                    SkinsTaken[i] = true;
                    return i;
                }
                skinOrder--;
            }
        }

        throw new Exception("No Tank Skins Where Available");
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ApplySingleton()
    {
        if (_instantiated) 
        {
            Destroy(this);
            return;
        }

        _instantiated = true;
        _isMainInstance = true;
        DontDestroyOnLoad(this);
    }
    

    public static Vector2 GetSpawnCoordinate(float distanceWithOthers = 5f)
    {
        NetworkManager networkManager = NetworkManager.Singleton;

        Vector2 spawnPoint = Vector2.zero;

        for (sbyte i = 0; i < networkManager.ConnectedClientsList.Count; i++)
        {
            NetworkClient client = networkManager.ConnectedClients[(byte)i];
            if (client.PlayerObject == null) break;

            if (Vector2.Distance(spawnPoint, client.PlayerObject.transform.position) < distanceWithOthers)
            {
                spawnPoint += (GetRandomBool() ? 1 : -1) * distanceWithOthers * (GetRandomBool() ? Vector2.up : Vector2.right);
                i = -1;
            }
        }   

        return spawnPoint;
    }  


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool GetRandomBool() => Random.value > 0.5f;


    public override void OnDestroy()
    {
        base.OnDestroy();

        if (_isMainInstance) 
        {
            _instantiated = false;
            Singleton = null;
        }
    }
}
