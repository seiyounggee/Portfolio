using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButton_StateVisualChanger : MonoBehaviour
{
    Button button;

    public enum State
    { 
        None,
        Focused,
        Unfocused,
    }

    [ReadOnly] public State currentState = State.None;

    public GameObject[] focusedObj = null;
    public GameObject[] unfocusedObj = null;

    private void Awake()
    {
        button = this.GetComponent<Button>();
    }

    public void SetState(State state)
    {
        currentState = state;

        switch (currentState)
        {
            case State.None:
                {
                    foreach (var i in focusedObj)
                        i.SafeSetActive(false);

                    foreach (var i in unfocusedObj)
                        i.SafeSetActive(false);
                }
                break;
            case State.Focused:
                {
                    foreach (var i in focusedObj)
                        i.SafeSetActive(true);

                    foreach (var i in unfocusedObj)
                        i.SafeSetActive(false);
                }
                break;
            case State.Unfocused:
                {
                    foreach (var i in focusedObj)
                        i.SafeSetActive(false);

                    foreach (var i in unfocusedObj)
                        i.SafeSetActive(true);
                }
                break;
        }
    }
}
