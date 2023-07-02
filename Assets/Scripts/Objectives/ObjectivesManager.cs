using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Objectives
{
    public class ObjectivesManager : MonoBehaviour
    {
        private UnityEvent onObjectiveComplete;
        private List<PlayerController> players;

        [SerializeField] private AreaObjectiveObject test;
        [SerializeField] private AreaObjectiveObject test2;
        [SerializeField] private ButtonsObjectiveObject test3;

        private Objective objective1;
        private Objective objective2;
        private Objective objective3;

        private Objective currentObjective;
        private List<Objective> objs;

        [Header("Time")]
        [SerializeField] private float timeBetweenObjectives;
        [SerializeField] private float timeBetweenChecks;

        [Header("Objective Triggers")]
        //ammo fill ratio = current ammo on player / base ammo
        [SerializeField] private float playerAmmoFillRatioForTrigger;

        private void Awake()
        {
            onObjectiveComplete = new UnityEvent();

            onObjectiveComplete.AddListener(CompleteObjective);

            Invoke(nameof(CheckIfShouldStartObjective), timeBetweenChecks);
        }

        private void CompleteObjective()
        {
            Debug.Log("complete");
            Invoke(nameof(CheckIfShouldStartObjective), timeBetweenObjectives);
        }
        private void BeginObjective()
        {
            Debug.Log("begin");
            currentObjective = objective3;
            currentObjective.Begin();
        }

        private void CheckIfShouldStartObjective()
        {

            Debug.Log("check");
            if (CheckIfPlayerNeedsGun() && 
                ZombieSpawnerManager.GetCurrentSpawnerState() != ZombieSpawnerManager.SpawnerState.BREAK) BeginObjective();
            //else Invoke(nameof(CheckIfShouldStartObjective), timeBetweenChecks);
        }

        private bool CheckIfPlayerNeedsGun()
        {
            bool needsNewWeapon = false;

            foreach (PlayerController p in PlayerController.GetPlayers())
            {
                Debug.Log(p.GetPlayerAmmoFillRatio() * 100);
                needsNewWeapon = p.GetPlayerAmmoFillRatio() < playerAmmoFillRatioForTrigger || needsNewWeapon;
            }

            return needsNewWeapon;
        }
    }
}