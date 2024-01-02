using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Utility;
using Objectives;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    private PlayerController player;
    private ObjectivesManager objectivesManager;
    private ZombieSpawnerManager zombieSpawnerManager;

    [SerializeField] private UnityEvent onStartGame;

    private void Awake()
    {
        //objectivesManager = transform.Find("ObjectivesManager").GetComponent<ObjectivesManager>();

        zombieSpawnerManager = transform.Find("ZombieSpawnManager").GetComponent<ZombieSpawnerManager>();
    }

    public void StartGame(PlayerInput playerInput)
    {

        Debug.Log("Started Game");
        onStartGame.Invoke();
        player = playerInput.GetComponent<PlayerController>();
        zombieSpawnerManager.SetTarget(playerInput.GetComponent<DamageableObject>());
        zombieSpawnerManager.BeginToSpawn();
    }
}
