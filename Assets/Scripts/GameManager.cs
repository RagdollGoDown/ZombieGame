using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;
using Objectives;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    private PlayerController player;
    private ObjectivesManager objectivesManager;
    private ZombieSpawnerManager zombieSpawnerManager;

    private void Awake()
    {
        objectivesManager = transform.Find("ObjectivesManager").GetComponent<ObjectivesManager>();

        zombieSpawnerManager = transform.Find("ZombieSpawnManager").GetComponent<ZombieSpawnerManager>();
    }

    public void StartGame(PlayerInput playerInput)
    {
        player = playerInput.GetComponent<PlayerController>();
        zombieSpawnerManager.SetTarget(playerInput.GetComponent<DamageableObject>());
        zombieSpawnerManager.BeginToSpawn();
    }
}
