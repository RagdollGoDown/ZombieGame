using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utility;
using Objectives;
using System;
using Objectives.Button;

namespace Player
{
    public class PlayerUI : MonoBehaviour
    {
        private readonly static float HEALTH100 = 100f;

        private PlayerController _playerController;

        private Camera playerCamera;

        private Canvas canvas;
        private CanvasScaler canvasScaler;
        private float uiScale;

        private GameObject playScreen;

        public enum Menu
        {
            Pause,
            Shop
        }

        private Menu currentMenu;

        private GameObject deathScreen;
        private GameObject pauseScreen;
        private GameObject shopScreen;

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

        private TextMeshProUGUI MoneyText;
        private TextMeshProUGUI _interactText;

        private struct SlowMoUI
        {
            public Slider slowMoChargeSlider;
        }
        private SlowMoUI slowMoUI;

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

            playScreen = transform.Find("PlayScreen").gameObject;
            deathScreen = transform.Find("DeathScreen").gameObject;
            deathScreen.SetActive(false);
            pauseScreen = transform.Find("PauseScreen").gameObject;
            pauseScreen.SetActive(false);
            shopScreen = transform.Find("ShopScreen").gameObject;
            shopScreen.SetActive(false);

            weaponUI.ammoTextHolder = playScreen.transform.Find("Ammo/AmmoTextHolder").GetComponent<TextMeshProUGUI>();
            weaponUI.weaponNameTextHolder = playScreen.transform.Find("Ammo/WeaponNameTextHolder").GetComponent<TextMeshProUGUI>();
            weaponUI.crosshairTransform = playScreen.transform.Find("Crosshair").GetComponent<RectTransform>();

            _healthUI = new();
            _healthUI.slider = playScreen.transform.Find("HealthBar").GetComponent<Slider>();
            _healthUI.tmp = _healthUI.slider.transform.Find("HealthText").GetComponent<TextMeshProUGUI>();

            slowMoUI.slowMoChargeSlider = playScreen.transform.Find("SlowMoChargeSlider").GetComponent<Slider>();

            MoneyText = playScreen.transform.Find("MoneyText/Text").GetComponent<TextMeshProUGUI>();

            _objectiveUI = new();
            _objectiveUI.talkieWalkieMover = transform.parent.Find("GunCamera/WalkieTalkie").GetComponent<MovePointToPointNonUI>();
            _objectiveUI.backgroundAndTextMover = playScreen.transform.Find("Objective").GetComponent<MovePointToPoint>();
            _objectiveUI.completenessSlider = _objectiveUI.backgroundAndTextMover.transform.Find("Slider").GetComponent<Slider>();
            _objectiveUI.objectiveText = _objectiveUI.backgroundAndTextMover.transform.Find("Text").GetComponent<TextMeshProUGUI>();
            _objectiveUI.objectiveText.text = string.Empty;

            _interactText = playScreen.transform.Find("InteractionText").GetComponent<TextMeshProUGUI>();
            _interactText.text = "";

            _shakableUIElements = playScreen.GetComponentsInChildren<ShakableUIElement>();

            foreach(ShakableUIElement shakable in _shakableUIElements)
            {
                shakable.Prepare();
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
            deathScreen.SetActive(true);

            deathScreen.transform.Find("TimeScore/score").GetComponent<TextMeshProUGUI>().text = PlayerScore.GetTime().ToString("N2");
            deathScreen.transform.Find("KillScore/score").GetComponent<TextMeshProUGUI>().text = PlayerScore.GetKills().ToString();
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
            _objectiveUI.currentObjective = objective;

            if (objective is WaitObjective waitObjective)
            {
                _objectiveUI.completenessSlider.value = waitObjective.GetCompletenessRatio();
                waitObjective.ObserveCurrentTime((value) => 
                {
                    _objectiveUI.completenessSlider.value = waitObjective.GetCompletenessRatio();
                });
            }
            else if (objective is ButtonObjective buttonObjective)
            {
                _objectiveUI.completenessSlider.value = buttonObjective.GetCompletenessRatio();
                buttonObjective.ObserveOnButtonPress(() =>
                {
                    _objectiveUI.completenessSlider.value = buttonObjective.GetCompletenessRatio();
                });
            }
            else if (objective is AreaObjective areaObjective)
            {
                _objectiveUI.completenessSlider.value = areaObjective.GetCompletenessRatio();
                areaObjective.ObserveStayInArea(() =>
                {
                    _objectiveUI.completenessSlider.value = areaObjective.GetCompletenessRatio();
                });
            }
            else
            {
                _objectiveUI.completenessSlider.value = 1;
            }

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

        public void UpdateSlowMoChargeSliders(float charge)
        {
            slowMoUI.slowMoChargeSlider.value = charge;
        }

        public void OpenMenu(Menu menu)
        {
            switch (menu)
            {
                case Menu.Pause:
                    pauseScreen.SetActive(true);
                    break;
                case Menu.Shop:
                    shopScreen.SetActive(true);
                    break;
                default:
                    Debug.LogError("Menu not found");
                    break;
            }

            currentMenu = menu;
        }

        public void CloseMenu()
        {
            switch (currentMenu)
            {
                case Menu.Pause:
                    pauseScreen.SetActive(false);
                    break;
                case Menu.Shop:
                    shopScreen.SetActive(false);
                    break;
                default:
                    Debug.LogError("Menu not found");
                    break;
            }
        }

        //----------------------------------------Setters
        public void SetInteractionText(string newText)
        {
            _interactText.text = newText;
        }

        public void SetMoneyText(int money)
        {
            MoneyText.text = money.ToString();
        }

        public void SetMission(Mission mission)
        {
            UpdateCurrentObjective(mission.GetCurrentObjective().GetValue());
            mission.GetCurrentObjective().onValueChange += UpdateCurrentObjective;

            _objectiveUI.talkieWalkieMover.Point2to1();
            _objectiveUI.backgroundAndTextMover.Point2to1();
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

}
