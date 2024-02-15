using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;
using Player;

[RequireComponent(typeof(Interactable))]
public class OpenMenu : MonoBehaviour
{
    [SerializeField] private PlayerUI.Menu menu;

    public void Open(){
        Interactable interactable = GetComponent<Interactable>();

        PlayerController player = (PlayerController)interactable.GetInteractorInArea();

        player.getPlayerUI().OpenMenu(menu);
    }
}
