using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;
    public GameObject playerPrefab;
    public GameObject projectilePrefab;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            //Making sure only one instance of the client class exists
            Debug.Log("Instance already exists");
            Destroy(this);
        }
    }

    private void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 30; // Because the game is running at 30 ticks right now
                                          // If we make it higher, we can update the target frame rate

        // If Unity Editor (because it will jam up the socket when trying to relog 
        //Comment out to test in editor
        //#if UNITY_EDITOR
        //Debug.Log("Build the project to start the server!");

        //#else
        Server.Start(2, 30033); // Start(max players, port)
        //#endif
    }

    public void OnApplicationQuit()
    {
        Server.Stop();
    }

    public Player InstantiatePlayer()
    {
        return Instantiate(playerPrefab, Vector3.zero, Quaternion.identity).GetComponent<Player>();
    }

    public Projectile InstantiateProjectile(Transform _shootOrigin)
    {
        return Instantiate(projectilePrefab, _shootOrigin.position + _shootOrigin.forward * 0.7f, Quaternion.identity).GetComponent<Projectile>();
    }
}
