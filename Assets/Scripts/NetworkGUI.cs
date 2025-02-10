using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;

public class NetworkGUI : MonoBehaviour
{
    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));

        DisplayGUIButtons();

        GUILayout.EndArea();
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void DisplayGUIButtons()
    {
        if (NetworkManager.Singleton == null  || 
            NetworkManager.Singleton.IsClient || 
            NetworkManager.Singleton.IsServer) return;

        NetworkManager networkManager = NetworkManager.Singleton;
        
        if (GUILayout.Button("Start Host")) networkManager.StartHost();
        else if (GUILayout.Button("Start Server")) networkManager.StartServer();
        else if (GUILayout.Button("Start Client")) networkManager.StartClient();
    }
}
