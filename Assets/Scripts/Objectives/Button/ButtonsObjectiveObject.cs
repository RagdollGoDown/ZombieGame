using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class ButtonsObjectiveObject : ObjectiveObject
{   
    public void PressButton()
    {
        GetObjectEvent().Invoke();
        turnOff();
    }
}
public class ButtonsObjective : Objective
{
    private readonly int _totalButtonsToPress;
    private int _buttonsToPress;

    public ButtonsObjective(UnityEvent onComplete, ButtonsObjectiveObject[] buttons) : base(onComplete)
    {
        _totalButtonsToPress = buttons.Length;

        foreach (ButtonsObjectiveObject button in buttons)
        {
            AddObjectiveObject(button);
            button.GetObjectEvent().AddListener(PressButton);
        }
    }

    public override void Begin()
    {
        base.Begin();

        _buttonsToPress = _totalButtonsToPress;
    }

    private void PressButton()
    {
        _buttonsToPress -= 1;

        if (_buttonsToPress <= 0) { Complete(); }
    }

    public override int GetScore()
    {
        throw new System.NotImplementedException();
    }

    public override float getCompletenessRatio()
    {
        return _buttonsToPress / _totalButtonsToPress;
    }
}