using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utility;
using Utility.Observable;
using Objectives;

public class PlayerUI : MonoBehaviour
{
    private readonly static float HEALTH100 = 100f;

    private PlayerController _playerController;

    private Camera playerCamera;

    private Canvas canvas;
    private CanvasScaler canvasScaler;
    private float uiScale;

    private GameObject _playScreen;
    private GameObject _deathScreen;

    private struct WeaponUI
    {
        public TextMeshProUGUI ammoTextHolder;
        public TextMeshProUGUI weaponNameTextHolder;
        public RectTransform crosshairTransform;
    }
    private WeaponUI weaponUI;

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
        public Objective currentObjective;

        public MovePointToPointNonUI talkieWalkieMover;
        public MovePointToPoint backgroundAndTextMover;
        public Slider completenessSlider;
        public TextMeshProUGUI objectiveText;
    }
    private ObjectiveUI _objectiveUI;

    [Header("Shaking on damage taken")]
    private ShakableUIElement[] _shakableUIElements;
    [SerializeField] private float shakingLength;
    [SerializeField] private float shakingStrength;

    // Start is called before the first frame update
    void Awake()
    {
        _playerController = transform.parent.parent.parent.GetComponent<PlayerController>();
        playerCamera = transform.parent.GetComponent<Camera>();

        canvas = GetComponent<Canvas>();
        canvasScaler = GetComponent<CanvasScaler>();
        uiScale = new();
        UpdateUIScale();

        _playScreen = transform.Find("PlayScreen").gameObject;
        _deathScreen = transform.Find("DeathScreen").gameObject;
        _deathScreen.SetActive(false);

        weaponUI.ammoTextHolder = _playScreen.transform.Find("Ammo/AmmoTextHolder").GetComponent<TextMeshProUGUI>();
        weaponUI.weaponNameTextHolder = _playScreen.transform.Find("Ammo/WeaponNameTextHolder").GetComponent<TextMeshProUGUI>();
        weaponUI.crosshairTransform = _playScreen.transform.Find("Crosshair").GetComponent<RectTransform>();

        _healthUI = new();
        _healthUI.slider = _playScreen.transform.Find("HealthBar").GetComponent<Slider>();
        _healthUI.tmp = _healthUI.slider.transform.Find("HealthText").GetComponent<TextMeshProUGUI>();

        _currentRoundText = _playScreen.transform.Find("RoundText/Text").GetComponent<TextMeshProUGUI>();

        _objectiveUI = new();
        _objectiveUI.talkieWalkieMover = transform.parent.Find("GunCamera/WalkieTalkie").GetComponent<MovePointToPointNonUI>();
        _objectiveUI.backgroundAndTextMover = _playScreen.transform.Find("Objective").GetComponent<MovePointToPoint>();
        _objectiveUI.completenessSlider = _objectiveUI.backgroundAndTextMover.transform.Find("Slider").GetComponent<Slider>();
        _objectiveUI.objectiveText = _objectiveUI.backgroundAndTextMover.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        _objectiveUI.objectiveText.text = string.Empty;

        _interactText = _playScreen.transform.Find("InteractionText").GetComponent<TextMeshProUGUI>();
        _interactText.text = "";

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

    private void UpdateCurrentObjective(Objective objective)
    {
        _objectiveUI.talkieWalkieMover.Point2to1();
        _objectiveUI.backgroundAndTextMover.Point2to1();
        UpdateObjectiveText(objective.GetObjectiveText().GetValue());
        objective.GetObjectiveText().onValueChange += UpdateObjectiveText;
    }

    private void UpdateObjectiveText(string text)
    {
        _objectiveUI.objectiveText.text = text;
    }

    public void UpdateUIScale()
    {
        uiScale = canvasScaler.referencePixelsPerUnit * canvasScaler.referenceResolution.x / playerCamera.fieldOfView;
    }

    public void ActivatePauseUI()
    {

    }

    public void DeactivatePauseUI()
    {

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

    public void SetMission(Mission mission)
    {
        UpdateCurrentObjective(mission.GetCurrentObjective().GetValue());
        mission.GetCurrentObjective().onValueChange += UpdateCurrentObjective;
    }

    public void SetWeaponName(string name)
    {
        weaponUI.weaponNameTextHolder.text = name;
    }

    public void SetAmmoText(string text)
    {
        weaponUI.ammoTextHolder.text = text;
    }

    public void SetCrosshairScale(float spread)
    {
        weaponUI.crosshairTransform.sizeDelta = new Vector2(spread, spread) * uiScale;
    }
}
