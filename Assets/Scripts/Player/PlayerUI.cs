using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    //private static Vector3 POSITION_OF_HIDDEN_WEAPON_MODEL = new Vector3(0, -1000, 0);
    private readonly static float HEALTH100 = 100f;

    private PlayerController _playerController;

    private GameObject _playScreen;
    private GameObject _deathScreen;

    private struct HealthUI
    {
        public Slider slider;
        public TextMeshProUGUI tmp;
    }
    private HealthUI _healthUI;

    private TextMeshProUGUI _currentRoundText;
    private TextMeshProUGUI _interactText;

    private struct ObjectiveUI
    {
        public MovePointToPointNonUI talkieWalkieMover;
        public MovePointToPoint backgroundAndTextMover;
        public Slider completenessSlider;
        public TextMeshProUGUI tmp;
    }
    private ObjectiveUI _objectiveUI;

    private MovePointToPoint _weaponModelHolder;
    private Transform _weaponModelHolderTransform;

    [Header("Shaking on damage taken")]
    private ShakableUIElement[] _shakableUIElements;
    [SerializeField] private float shakingLength;
    [SerializeField] private float shakingStrength;

    // Start is called before the first frame update
    void Awake()
    {
        _playerController = transform.parent.parent.parent.GetComponent<PlayerController>();

        _playScreen = transform.Find("PlayScreen").gameObject;
        _deathScreen = transform.Find("DeathScreen").gameObject;
        _deathScreen.SetActive(false);

        _healthUI = new();
        _healthUI.slider = _playScreen.transform.Find("HealthBar").GetComponent<Slider>();
        _healthUI.tmp = _healthUI.slider.transform.Find("HealthText").GetComponent<TextMeshProUGUI>();

        _currentRoundText = _playScreen.transform.Find("RoundText/Text").GetComponent<TextMeshProUGUI>();

        _objectiveUI = new();
        _objectiveUI.talkieWalkieMover = transform.parent.Find("GunCamera/WalkieTalkie").GetComponent<MovePointToPointNonUI>();
        _objectiveUI.backgroundAndTextMover = _playScreen.transform.Find("Objective").GetComponent<MovePointToPoint>();
        _objectiveUI.completenessSlider = _objectiveUI.backgroundAndTextMover.transform.Find("Slider").GetComponent<Slider>();
        _objectiveUI.tmp = _objectiveUI.backgroundAndTextMover.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        _objectiveUI.tmp.text = string.Empty;

        _interactText = _playScreen.transform.Find("InteractionText").GetComponent<TextMeshProUGUI>();
        _interactText.text = "";

        //_weaponModelHolder = _playScreen.transform.Find("Ammo/WeaponModelHolder").GetComponent<MovePointToPoint>();
        //_weaponModelHolderTransform = _weaponModelHolder.transform;

        _shakableUIElements = _playScreen.GetComponentsInChildren<ShakableUIElement>();

        foreach(ShakableUIElement shakable in _shakableUIElements)
        {
            shakable.SetLengthAndStrength(shakingLength, shakingStrength);
        }
    }

    public void TakeDamage()
    {
        if (_playerController.GetPlayerState() == PlayerController.PlayerState.Dead) return;

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

    public void UpdateHealthBar(Damage damage)
    {
        if (_playerController.GetPlayerState() != PlayerController.PlayerState.Normal) return;

        float ratio = _playerController.GetPlayerHealthRatio();
        _healthUI.slider.value = 1-ratio;
        _healthUI.tmp.text = (ratio * HEALTH100).ToString();

        if (_healthUI.slider.value == 1) { Die(); }
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

    public void SetObjectiveText(Objective objective)
    {
        if (objective == null)
        {
            _objectiveUI.talkieWalkieMover.Point1to2();
            _objectiveUI.backgroundAndTextMover.Point1to2();
            _objectiveUI.tmp.text = string.Empty;
        }
        else
        {
            _objectiveUI.talkieWalkieMover.Point2to1();
            _objectiveUI.backgroundAndTextMover.Point2to1();
            _objectiveUI.tmp.text = objective.GetObjectiveText();
        }
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
