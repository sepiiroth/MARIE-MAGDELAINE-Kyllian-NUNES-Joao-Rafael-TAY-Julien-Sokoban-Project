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
    
    Vector2 size = GameManager.Instance().size;

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

     public void InitializeActions(int[] map, State[] states, int[] playerState, List<int[]> boxState, int x, int y,
         int xBox, int yBox) 
     {
         var reward = 0;
         State nextMove = null;
         State playerMove = null;
         if (x > 0 && map[(x - 1 )+ 4 * y] != 1)
         {
             //playerMove = states[(x - 1) + y * (int) size.x + ((yBox * (int) size.x +xBox) * (int) (size.x * size.y))];
             nextMove = GetNextMove(x - 1, y, xBox, yBox, playerState, boxState, map, states,Direction.Left);
             if (nextMove != null)
             {
                 if(xBox - 1 > 0 && map[(xBox - 1) + 4 * (yBox)] != 1)
                     reward = GetRewardForState(map[(xBox - 1 )+ 4 * yBox]);
                 actions.Add(new Action(reward, nextMove, reward, Direction.Left));
             }
         }
         
         if (x < 3 && map[(x + 1 )+ 4 * y] != 1)
         {
             //playerMove = states[(x + 1) + y * (int) size.x + ((yBox * (int) size.x +xBox) * (int) (size.x * size.y))];
             nextMove = GetNextMove(x + 1, y, xBox, yBox, playerState, boxState, map, states,Direction.Right);
             if (nextMove != null )
             {
                 if(xBox + 1 < 3 && map[(xBox + 1) + 4 * yBox] != 1)
                    reward = GetRewardForState(map[(xBox + 1) + 4 * yBox]);
                 actions.Add(new Action(reward, nextMove, reward, Direction.Right));
             }
         }
         
         if (y > 0 && map[x + 4 * (y - 1)] != 1)
         {
            // playerMove = states[x + (y - 1) * (int) size.x + ((yBox * (int) size.x +xBox) * (int) (size.x * size.y))];
             nextMove = GetNextMove(x, y - 1, xBox, yBox, playerState, boxState, map, states,Direction.Up);
             if (nextMove != null)
             {
                 if(yBox - 1 > 0 && map[(xBox) + 4 * (yBox - 1)] != 1)
                    reward = GetRewardForState(map[(xBox) + 4 * (yBox - 1)]);
                 actions.Add(new Action(reward, nextMove, reward, Direction.Up));
             }
         }
         
         if (y < 3 && map[x + 4 * (y + 1)] != 1)
         {
             //playerMove = states[x + (y + 1) * (int) size.x + ((yBox * (int) size.x +xBox) * (int) (size.x * size.y))];
             nextMove = GetNextMove(x , y + 1, xBox, yBox, playerState, boxState, map, states,Direction.Down);
             if (nextMove != null)
             {
                 if (yBox + 1 < 3 && map[(xBox) + 4 * (yBox + 1)] != 1)
                     reward = GetRewardForState(map[(xBox) + 4 * (yBox + 1)]);
                 actions.Add(new Action(reward, nextMove, reward, Direction.Down));
             }
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

     State GetNextMove(int x, int y, int xBox, int yBox, int[] playerState, List<int[]> boxState, int[] map, State[] states, Direction decalage)
     {
         
         State nextMove = null;
         if (playerState[x + y * (int)size.x] == boxState[0][xBox + yBox * (int)size.x])
         {
             switch (decalage)
             {
                 case Direction.Left:
                     if (x > 0 && map[yBox * (int) size.x + (xBox - 1)] != 1)
                         nextMove = states[x + y * (int) size.x + ((yBox * (int) size.x + (xBox - 1)) * (int) (size.x * size.y))];
                     break;
                 case Direction.Right:
                     if (x < 3 && map[yBox * (int) size.x + (xBox + 1)] != 1)
                        nextMove = states[x + y * (int) size.x + ((yBox * (int) size.x + (xBox + 1)) * (int) (size.x * size.y))];
                     break;
                 case Direction.Up:
                     if (y > 0 && map[(yBox - 1) * (int) size.x + xBox] != 1)
                        nextMove = states[x + y * (int) size.x + (((yBox - 1) * (int) size.x + xBox) * (int) (size.x * size.y))];
                     break;
                 case Direction.Down:
                     if (y < 3 && map[(yBox + 1) * (int) size.x + xBox] != 1)
                        nextMove = states[x + y * (int) size.x + (((yBox + 1) * (int) size.x + xBox) * (int) (size.x * size.y))];
                     break;
             }
         }
         else
         {
             nextMove = states[x + y * (int) size.x + ((yBox * (int) size.x +xBox) * (int) (size.x * size.y))];
         }
         
         
         return nextMove;
     }
    
}
