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

        _shakableUIElements = _playScreen.GetComponentsInChildren<ShakableUIElement>();

        foreach(ShakableUIElement shakable in _shakableUIElements)
        {
            shakable.SetLengthAndStrength(shakingLength, shakingStrength);
        }
    }

    public void TakeDamage()
    {
        Debug.Log("taken");
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
}
