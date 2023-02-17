using System;using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    Up,
    Down,
    Left,
    Right
}

public class Action
{
    public int reward;
    public State nextState;
    public float Qs = 0;
    public Direction dir;

    public Action(int reward, State nextState, float Qs, Direction dir)
    {
        this.reward = reward;
        this.nextState = nextState;
        this.Qs = Qs;
        this.dir = dir;
    }
}
