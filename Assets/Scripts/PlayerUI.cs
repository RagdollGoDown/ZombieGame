using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    private static Vector3 POSITION_OF_HIDDEN_WEAPON_MODEL = new Vector3(0, -1000, 0);

    [SerializeField] private Slider _healthBarSlider;
    private TextMeshProUGUI _currentRoundText;

    private GameObject _playScreen;
    private GameObject _deathScreen;

    private TextMeshProUGUI _interactText;

    private MovePointToPoint _weaponModelHolder;
    private Transform _weaponModelHolderTransform;

    [Header("Shaking on damage taken")]
    private ShakableUIElement[] _shakableUIElements;
    [SerializeField] private float shakingLength;
    [SerializeField] private float shakingStrength;

    // Start is called before the first frame update
    void Awake()
    {
        _playScreen = transform.Find("PlayScreen").gameObject;
        _deathScreen = transform.Find("DeathScreen").gameObject;
        _deathScreen.SetActive(false);

        _interactText = _playScreen.transform.Find("InteractionText").GetComponent<TextMeshProUGUI>();
        _interactText.text = "";

        _weaponModelHolder = _playScreen.transform.Find("Ammo/WeaponModelHolder").GetComponent<MovePointToPoint>();
        _weaponModelHolderTransform = _weaponModelHolder.transform;

        _shakableUIElements = _playScreen.GetComponentsInChildren<ShakableUIElement>();

        foreach(ShakableUIElement shakable in _shakableUIElements)
        {
            shakable.SetLengthAndStrength(shakingLength, shakingStrength);
        }
    }

    public void TakeDamage()
    {
        foreach(ShakableUIElement shakable in _shakableUIElements)
        {
            shakable.Shake();
        }
    }

    public void Die()
    {
        _deathScreen.SetActive(true);
        _playScreen.SetActive(false);

        _deathScreen.transform.Find("TimeScore/score").GetComponent<TextMeshProUGUI>().text = PlayerScore.GetTime().ToString("N2");
        _deathScreen.transform.Find("KillScore/score").GetComponent<TextMeshProUGUI>().text = PlayerScore.GetKills().ToString();
    }

    //----------------------------------------Setters
    public void SetInteractionText(string newText)
    {
        _interactText.text = newText;
    }

    public void SetRoundText(int round)
    {
        _currentRoundText.text = round.ToString();
    }

    /*public void SetUIWeaponModel(Transform modelTransform)
    {
        if (_weaponModelHolder.transform.childCount == 1)
        {
            Transform currentModel = _weaponModelHolder.transform.GetChild(0);
            currentModel.parent = null;
            currentModel.position = POSITION_OF_HIDDEN_WEAPON_MODEL;
        }

        //_weaponModelHolder.Point1to2();
        modelTransform.parent = _weaponModelHolder.transform;
        modelTransform.localPosition = Vector3.zero;
        Debug.Log(_weaponModelHolder.GetComponent<RectTransform>().localPosition);
    }*/
}
