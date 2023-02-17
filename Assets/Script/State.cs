using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State
{
    public string name;

    public float Vs;
    public Action policy;

    public List<Action> actions;

    public int N = 0;
    public float Return = 0;

    /*private void Awake()
    {
        Actions = GetComponents<Action>();
        _a = new List<Action>();

        for (int i = 0; i < Actions.Length; i++)
        {
            _a.Add(Actions[i]);
        }
    }*/

     public State(float Vs, Action policy, string n)
     {
         this.name = n;
         this.Vs = Vs;
         this.policy = policy;

         actions = new List<Action>();
         this.N = 0;
         this.Return = 0;
     }

     public void InitializeActions(int[] map, State[] states, int x, int y)
     {
         var reward = 0;
         if (x > 0 && map[(x - 1 )+ 4 * y] != 1)
         {
             reward = GetRewardForState(map[(x - 1 )+ 4 * y]);
             actions.Add(new Action(reward, states[(x - 1 )+ 4 * y], reward, Direction.Left));
         }
         
         if (x < 3 && map[(x + 1 )+ 4 * y] != 1)
         {
             reward = GetRewardForState(map[(x + 1 )+ 4 * y]);
             actions.Add(new Action(reward, states[(x + 1 )+ 4 * y], reward, Direction.Right));
         }
         
         if (y > 0 && map[x + 4 * (y - 1)] != 1)
         {
             reward = GetRewardForState(map[x + 4 * (y - 1)]);
             actions.Add(new Action(reward, states[x + 4 * (y - 1)], reward, Direction.Up));
         }
         
         if (y < 3 && map[x + 4 * (y + 1)] != 1)
         {
             reward = GetRewardForState(map[x + 4 * (y + 1)]);
             actions.Add(new Action(reward, states[x + 4 * (y + 1)], reward, Direction.Down));
         }
     }

     int GetRewardForState(int tag)
     {
         switch (tag)
         {
             case -1:
                 return -1;
             case 3:
                 return 1; 
         }
         return 0;
     }
    
}
