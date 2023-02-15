using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ESGI
{
    public class GameState : MonoBehaviour
    {
        public State[] state;

        public float gamma = 0.5f;

        private bool isStable = false;

        public Transform agent;

        public State start;

        // Start is called before the first frame update
        void Start()
        {
            for (int i = 0; i < state.Length; i++)
            {
                //state[i].Vs = 0;
                if (state[i].Actions.Length > 0)
                {
                    state[i].policy = state[i].Actions[Random.Range(0, state[i].Actions.Length)];
                }
                
            }

            while (!isStable)
            {
                PolicyEvaluation(gamma);
                isStable = PolicyImprovement(gamma);
            }

            StartCoroutine(Move());
        }

        IEnumerator Move()
        {
            var current = start;
            while (current.policy != null)
            {
                yield return new WaitForSeconds(1);
                //Debug.Log(current);
                agent.Translate(current.policy.nextState.transform.position - current.transform.position);
                current = current.policy.nextState;
            }
        }

        void PolicyEvaluation(float gamma)
        {
            float delta = 0;

            do
            {
                delta = 0;
                foreach (var s in state)
                {
                    if (s.policy == null)
                    {
                        continue;
                    }
                    float temp = s.Vs;
                    s.Vs = s.policy.reward + gamma * s.policy.nextState.Vs;
                    delta = Mathf.Max(delta, Mathf.Abs(temp - s.Vs));

                }
            } while (delta > 0.01);
        }

        bool PolicyImprovement(float gamma)
        {
            
            bool policyStable = true;
            foreach (var s in state)
            {
                if (s.policy == null)
                {
                    continue;
                }
                Action temp = s.policy;

                Action action = s._a[0];
                float res = action.reward + gamma * action.nextState.Vs;
                for (int i = 1; i < s._a.Count; i++)
                {
                    if (s._a[i].reward + gamma * s._a[i].nextState.Vs > res)
                    {
                        action = s._a[i];
                        res = s._a[i].reward + gamma * s._a[i].nextState.Vs;
                    }
                }
                s.policy = action;
                if (temp != s.policy)
                {
                    policyStable = false;
                }
            }

            return policyStable;

        }
    }
}


