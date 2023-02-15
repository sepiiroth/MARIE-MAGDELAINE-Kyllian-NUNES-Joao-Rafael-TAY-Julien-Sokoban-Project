using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State : MonoBehaviour
{
    public string name;
    public Action[] Actions;

    public float Vs;
    public Action policy;

    public List<Action> _a;

    private void Awake()
    {
        Actions = GetComponents<Action>();
        _a = new List<Action>();

        for (int i = 0; i < Actions.Length; i++)
        {
            _a.Add(Actions[i]);
        }
    }
}
