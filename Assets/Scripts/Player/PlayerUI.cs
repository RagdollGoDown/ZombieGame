using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utility;
using Objectives;
using Objectives.Button;
using System.Collections.Generic;
namespace Player
{
    public class PlayerUI : MonoBehaviour
    {
        private readonly static float HEALTH100 = 100f;
        private readonly static int TIME_BETWEEN_OBJECTIVE_ARROW_UPDATES_MS = 50;

        private PlayerController _playerController;

        private Camera playerCamera;

        private Canvas canvas;
        private CanvasScaler canvasScaler;
        private float uiScale;

        private GameObject playScreen;

        public enum Menu
        {
            Pause,
            Death
        }

        private Menu currentMenu;

        private GameObject deathScreen;
        private GameObject pauseScreen;

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

        private TextMeshProUGUI TimeText;
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
            public RectTransform objectiveArrow;
            public CancellationTokenSource arrowUpdateCancellationTokenSource;
            public CancellationToken arrowUpdateCancellationToken;
        }
        private ObjectiveUI _objectiveUI;

        [Header("Shaking on damage taken")]
        private ShakableUIElement[] _shakableUIElements;
        [SerializeField] private float shakingLength;
        [SerializeField] private float shakingStrength;

        //----------------------------------------Unity events
        private void Awake()
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

            weaponUI.ammoTextHolder = playScreen.transform.Find("Ammo/AmmoTextHolder").GetComponent<TextMeshProUGUI>();
            weaponUI.weaponNameTextHolder = playScreen.transform.Find("Ammo/WeaponNameTextHolder").GetComponent<TextMeshProUGUI>();
            weaponUI.crosshairTransform = playScreen.transform.Find("Crosshair").GetComponent<RectTransform>();

            _healthUI = new();
            _healthUI.slider = playScreen.transform.Find("HealthBar").GetComponent<Slider>();
            _healthUI.tmp = _healthUI.slider.transform.Find("HealthText").GetComponent<TextMeshProUGUI>();

            slowMoUI.slowMoChargeSlider = playScreen.transform.Find("SlowMoChargeSlider").GetComponent<Slider>();

            TimeText = playScreen.transform.Find("TimeText/Text").GetComponent<TextMeshProUGUI>();

            _objectiveUI = new();
            _objectiveUI.talkieWalkieMover = transform.parent.Find("GunCamera/WalkieTalkie").GetComponent<MovePointToPointNonUI>();
            _objectiveUI.backgroundAndTextMover = playScreen.transform.Find("Objective").GetComponent<MovePointToPoint>();
            _objectiveUI.completenessSlider = _objectiveUI.backgroundAndTextMover.transform.Find("Slider").GetComponent<Slider>();
            _objectiveUI.objectiveText = _objectiveUI.backgroundAndTextMover.transform.Find("Text").GetComponent<TextMeshProUGUI>();
            _objectiveUI.objectiveText.text = string.Empty;
            _objectiveUI.objectiveArrow = _objectiveUI.backgroundAndTextMover.transform.Find("Arrow").GetComponent<RectTransform>();
            _objectiveUI.arrowUpdateCancellationTokenSource = new CancellationTokenSource();
            _objectiveUI.arrowUpdateCancellationToken = _objectiveUI.arrowUpdateCancellationTokenSource.Token;

            UpdateObjectiveArrow();

            _interactText = playScreen.transform.Find("InteractionText").GetComponent<TextMeshProUGUI>();
            _interactText.text = "";

            _shakableUIElements = playScreen.GetComponentsInChildren<ShakableUIElement>();

            foreach(ShakableUIElement shakable in _shakableUIElements)
            {
                shakable.Prepare();
                shakable.SetLengthAndStrength(shakingLength, shakingStrength);
            }
        }

        private void OnDisable()
        {
            _objectiveUI.arrowUpdateCancellationTokenSource.Cancel();
        }

        //----------------------------------------general methods

        public void TakeDamage()
        {
            if (_playerController.GetPlayerState() != PlayerController.PlayerState.Normal) return;

            foreach(ShakableUIElement shakable in _shakableUIElements)
            {
                shakable.Shake();
            }
        }

        public void UpdateHealthBar(DamageableObject damageableObject)
        {
            if (_playerController.GetPlayerState() != PlayerController.PlayerState.Normal) return;

            float ratio = _playerController.GetPlayerHealthRatio();
            _healthUI.slider.value = 1-ratio;
            _healthUI.tmp.text = (ratio * HEALTH100).ToString();
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

        private async void UpdateObjectiveArrow()
        {
            RectTransform objectiveArrow = _objectiveUI.objectiveArrow;

            while (true)
            {
                await Task.Delay(TIME_BETWEEN_OBJECTIVE_ARROW_UPDATES_MS);

                Objective objective = _objectiveUI.currentObjective;

                if (_objectiveUI.arrowUpdateCancellationToken.IsCancellationRequested) return;

                if (objective == null || objective.GetObjectiveObjects().Count == 0) {
                    objectiveArrow.gameObject.SetActive(false);
                    continue;
                } else {
                    objectiveArrow.gameObject.SetActive(true);
                }

                Vector3 playerDirection = _playerController.transform.forward;
                Vector3 objectiveDirection;

                List<ObjectiveObject> objectiveObjects = objective.GetObjectiveObjects();
                
                if (objective is ButtonObjective buttonObjective)
                {
                    ObjectiveObject closestButton = objectiveObjects[0];
                    float closestDistance = Vector3.Distance(closestButton.transform.position, _playerController.transform.position);

                    for(int i = 1; i < objectiveObjects.Count; i++)
                    {
                        float distance = Vector3.Distance(objectiveObjects[i].transform.position, _playerController.transform.position);

                        if (distance < closestDistance)
                        {
                            closestButton = objectiveObjects[i];
                            closestDistance = distance;
                        }
                    }

                    objectiveDirection = closestButton.transform.position - _playerController.transform.position;
                }
                else
                {
                    objectiveDirection = objectiveObjects[0].transform.position - _playerController.transform.position;
                }   

                objectiveArrow.rotation = Quaternion.Euler(0, 0, Vector3.SignedAngle(objectiveDirection, playerDirection, Vector3.up));
            }
        }

        public void UpdateUIScale()
        {
            if (canvasScaler == null || playerCamera == null) return;

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
                case Menu.Death:
                    deathScreen.SetActive(true);
                    deathScreen.transform.Find("TimeScore/score").GetComponent<TextMeshProUGUI>().text = _playerController.GetPlayerScore().GetTime().ToString("N2");
                    deathScreen.transform.Find("KillScore/score").GetComponent<TextMeshProUGUI>().text = _playerController.GetPlayerScore().GetKills().ToString();
                    break;
                case Menu.Pause:
                    pauseScreen.SetActive(true);
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
                case Menu.Death:
                    deathScreen.SetActive(false);
                    break;
                case Menu.Pause:
                    pauseScreen.SetActive(false);
                    break;
                default:
                    Debug.LogError("Menu not found");
                    break;
            }
        
        }

        public void UpdateTimeSurvived(float timeSurvived)
        {
            TimeText.text = timeSurvived.ToString("N2");
        }

        //----------------------------------------Setters
        public void SetInteractionText(string newText)
        {
            _interactText.text = newText;
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
