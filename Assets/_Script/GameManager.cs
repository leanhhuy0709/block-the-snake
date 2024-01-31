using UnityEngine;

namespace _Script
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;
        public float Delay = 1;
        public bool Debug;
        private void Awake()
        {
            Instance = this;
        }
        
        void Start()
        {
        
        }

        void Update()
        {
            //
        }
    }
}
