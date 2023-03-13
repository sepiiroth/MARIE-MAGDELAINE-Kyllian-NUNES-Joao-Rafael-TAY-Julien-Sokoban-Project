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
         List<Vector2Int> boxPosition) 
     {
         var reward = 0;
         State nextMove = null;
         State playerMove = null;
         Direction nextDirection = Direction.Left;
         int xBox = 0;
         int yBox = 0;
         if (boxPosition.Count > 0)
         {
             xBox = boxPosition[0].x; 
             yBox = boxPosition[0].y;
         }
        
         
         
         if (x > 0 && map[(x - 1 )+ (int)size.x * y] != 1)
         {
             //playerMove = states[(x - 1) + y * (int) size.x + ((yBox * (int) size.x +xBox) * (int) (size.x * size.y))];
             nextMove = GetNextMove(x - 1, y, xBox, yBox, playerState, boxState, map, states,Direction.Left);
             
             if (nextMove != null)
             {
                 if (GameManager.Instance().nbBox == 0)
                 {
                     reward = GetRewardForState(map[(x - 1)+ (int)size.x * y]);
                 }
                 nextDirection = Direction.Left;
                 if (GameManager.Instance().nbBox > 0 && ((x - 1 )+ (int)size.x * y) == (xBox + (int)size.x * yBox) && xBox - 1 >= 0)
                 {
                     reward = GetRewardForState(map[(xBox - 1)+ (int)size.x * yBox]);
                     nextDirection = Direction.PushLeft;
                 }
                 actions.Add(new Action(reward, nextMove, reward, nextDirection));
             }
         }
         
         reward = 0;
         if (x < (int)size.x - 1 && map[(x + 1 )+ (int)size.x * y] != 1)
         {
             //playerMove = states[(x + 1) + y * (int) size.x + ((yBox * (int) size.x +xBox) * (int) (size.x * size.y))];
             nextMove = GetNextMove(x + 1, y, xBox, yBox, playerState, boxState, map, states,Direction.Right);
             
             if (nextMove != null )
             {
                 if (GameManager.Instance().nbBox == 0)
                 {
                     reward = GetRewardForState(map[(x + 1)+ (int)size.x * y]);
                 }
                 nextDirection = Direction.Right;
                 if (GameManager.Instance().nbBox > 0 && ((x + 1 )+ (int)size.x * y) == (xBox + (int)size.x * yBox) && xBox + 1 < (int)size.x)
                 {
                     reward = GetRewardForState(map[(xBox + 1) + (int)size.x * yBox]);
                     nextDirection = Direction.PushRight;
                 }
                 actions.Add(new Action(reward, nextMove, reward, nextDirection));
             }
         }
         
         reward = 0;
         if (y > 0 && map[x + (int)size.x * (y - 1)] != 1)
         {
            // playerMove = states[x + (y - 1) * (int) size.x + ((yBox * (int) size.x +xBox) * (int) (size.x * size.y))];
             nextMove = GetNextMove(x, y - 1, xBox, yBox, playerState, boxState, map, states,Direction.Up);
             
             if (nextMove != null)
             {
                 if (GameManager.Instance().nbBox == 0)
                 {
                     reward = GetRewardForState(map[(x)+ (int)size.x * (y - 1)]);
                 }
                 nextDirection = Direction.Up;
                 if (GameManager.Instance().nbBox > 0 && (x + (int)size.x * (y - 1)) == (xBox + (int)size.x * yBox) && yBox - 1 >= 0)
                 {
                     reward = GetRewardForState(map[(xBox) + (int)size.x * (yBox - 1)]);
                     nextDirection = Direction.PushUp;
                 }
                 actions.Add(new Action(reward, nextMove, reward, nextDirection));
             }
         }
         
         reward = 0;
         if (y < (int)size.y - 1 && map[x + (int)size.x * (y + 1)] != 1)
         {
             //playerMove = states[x + (y + 1) * (int) size.x + ((yBox * (int) size.x +xBox) * (int) (size.x * size.y))];
             nextMove = GetNextMove(x , y + 1, xBox, yBox, playerState, boxState, map, states,Direction.Down);
             if (nextMove != null)
             {
                 if (GameManager.Instance().nbBox == 0)
                 {
                     reward = GetRewardForState(map[(x)+ (int)size.x * (y + 1)]);
                 }
                 nextDirection = Direction.Down;
                 if (GameManager.Instance().nbBox > 0 && (x + (int)size.x * (y + 1)) == (xBox + (int)size.x * yBox) && yBox + 1 < (int)size.y)
                 {
                     reward = GetRewardForState(map[(xBox) + (int)size.x * (yBox + 1)]);
                     nextDirection = Direction.PushDown;
                 }
                 actions.Add(new Action(reward, nextMove, reward, nextDirection));
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
         int offset = (y * (int) size.x + x) * (int)Math.Pow((int)(size.x * size.y), GameManager.Instance().nbBox);
         if (GameManager.Instance().nbBox == 0)
         {
             return states[offset];
         }
         
         State nextMove = null;
         if (playerState[x + y * (int)size.x] == boxState[0][xBox + yBox * (int)size.x])
         {
             switch (decalage)
             {
                 case Direction.Left:
                     if (xBox > 0 && map[yBox * (int) size.x + (xBox - 1)] != 1)
                     {
                         offset += ((yBox * (int) size.x + (xBox - 1)) * (int)Math.Pow((int)(size.x * size.y), 0));
                     }
                     break;
                 case Direction.Right:
                     if (xBox < (int)size.x - 1 && map[yBox * (int) size.x + (xBox + 1)] != 1)
                         offset += ((yBox * (int) size.x + (xBox + 1)) * (int)Math.Pow((int)(size.x * size.y), 0));
                     break;
                 case Direction.Up:
                     if (yBox > 0 && map[(yBox - 1) * (int) size.x + xBox] != 1)
                         offset += (((yBox - 1) * (int) size.x + xBox) * (int)Math.Pow((int)(size.x * size.y), 0));
                     break;
                 case Direction.Down:
                     if (yBox < (int)size.y - 1 && map[(yBox + 1) * (int) size.x + xBox] != 1)
                         offset += (((yBox + 1) * (int) size.x + xBox) * (int)Math.Pow((int)(size.x * size.y), 0));
                     break;
             }
             
             nextMove = states[offset];
         }
         else
         {
             offset += ((yBox * (int) size.x + xBox) * (int)Math.Pow((int)(size.x * size.y), 0));
             nextMove = states[offset];
         }
         
         return nextMove;
     }
    
}
