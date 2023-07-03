using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private Slider _healthBarSlider;
    private TextMeshProUGUI _currentRoundText;

    private GameObject _playScreen;
    private GameObject _deathScreen;

    private TextMeshProUGUI _interactText;

    // Start is called before the first frame update
    void Awake()
    {
        _playScreen = transform.Find("PlayScreen").gameObject;
        _deathScreen = transform.Find("DeathScreen").gameObject;
        _deathScreen.SetActive(false);

        _interactText = _playScreen.transform.Find("InteractionText").GetComponent<TextMeshProUGUI>();
        _interactText.text = "";
    }

    public void SetInteractionText(string newText)
    {
        _interactText.text = newText;
    }

    public void Die()
    {
        _deathScreen.SetActive(true);
        _playScreen.SetActive(false);

        _deathScreen.transform.Find("TimeScore/score").GetComponent<TextMeshProUGUI>().text = PlayerScore.GetTime().ToString("N2");
        _deathScreen.transform.Find("KillScore/score").GetComponent<TextMeshProUGUI>().text = PlayerScore.GetKills().ToString();
    }

    public void SetRoundText(int round)
    {
        _currentRoundText.text = round.ToString();
    }
}
