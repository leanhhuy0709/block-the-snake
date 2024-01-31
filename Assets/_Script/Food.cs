using UnityEngine;

namespace _Script
{
    public class Food : MonoBehaviour
    {
        public static Food Instance;
        
        private void Awake()
        {
            Instance = this;
        }
        void Start()
        {
            GenerateRandomPosition();
        }

        public void GenerateRandomPosition(int lim = 1000)
        {
            if (lim == 0)
            {
                Debug.Log("Can't generate food");
                return;
            }

            var newPos = GridSystem.Instance.GetRandomValidPosition(100);
            transform.position = newPos;

            // if (!Snake.Instance.AStarSearchFood(false))
            // {
            //     GenerateRandomPosition(lim - 1);
            // }
            
            
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
