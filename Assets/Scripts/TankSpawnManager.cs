
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Tank
{
    public class TankSpawnManager : NetworkBehaviour
    {
        public static TankSpawnManager Singleton { get; private set; }
        private bool _instantiated = false;
        private bool _isMainInstance = false;


        public List<Sprite> SkinsSprites;
        public List<Sprite> AmmoSprites;
        [SerializeField] private GameObject bulletPrefab;
        public GameObject BulletPrefab => bulletPrefab;
        private BoolList SkinsTaken = new(4);


        private void Awake()
        {
            ApplySingleton();
        }


        // protected override void OnNetworkPreSpawn(ref NetworkManager networkManager)
        // {
        //     base.OnNetworkPreSpawn(ref networkManager);
        // }


        private void Start()
        {
            NetworkManager.ConnectionApprovalCallback = ConnectionApprovalCallback;
        }


        private static void ConnectionApprovalCallback(
            NetworkManager.ConnectionApprovalRequest request,
            NetworkManager.ConnectionApprovalResponse response
        )
        {
            response.Approved = true;
            response.CreatePlayerObject = true;
            response.Position = GetSpawnCoordinate();
        }


        public byte GetRandomSkinIndex()
        {
            byte availabeSkinsCount = (byte)(SkinsTaken.Count - SkinsTaken.TrueValuesCount);

            byte skinOrder = (byte)Math.Floor(
                Random.Range(
                    0, 
                    availabeSkinsCount - Mathf.Epsilon
                )
            );

            for (byte i = 0; i < SkinsTaken.Count; i++)
            {
                if (!SkinsTaken[i])
                {
                    if (skinOrder == 0) return i;
                    skinOrder--;
                }
            }

            throw new Exception("No Tank Skins Where Available");
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Sprite ReserveSkin(byte index)
        {
            SkinsTaken[index] = true;
            return SkinsSprites[index];
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReleaseSkin(byte index) => SkinsTaken[index] = false;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReleaseSkin(Sprite skin) => ReleaseSkin((byte)SkinsSprites.IndexOf(skin));


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
            Singleton = this;
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
}
