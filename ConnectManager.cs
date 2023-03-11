using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Actor Connection Manager
// Attach to GameManager
namespace Unity.FPS.AI
{
    public class ConnectManager : MonoBehaviour
    {
        // Start is called before the first frame update
        Dictionary<int, List<GameObject>> Agents;
        void Start()
        {
            Agents = new Dictionary<int, List<GameObject>>();
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        public void AddAgent(GameObject obj, int affiliation)
        {
            if (Agents.ContainsKey(affiliation)){
                // Add a agent in existing team
                Agents[affiliation].Add(obj);
            }else{
                // Add a agent in unknown team
                Agents.Add(affiliation, new List<GameObject>(){obj});
            }
        }

        public void RemoveAgent(GameObject obj, int affiliation)
        {
            if (Agents[affiliation].Contains(obj)){
                Agents[affiliation].Remove(obj);
                Debug.Log(string.Format("Remove GameObject from {0}", affiliation));
            }
        }
    }
}
