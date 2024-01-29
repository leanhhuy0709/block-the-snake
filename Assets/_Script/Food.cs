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

        public void GenerateRandomPosition(int lim = 50)
        {
            if (lim == 0)
            {
                Debug.Log("Can't generate food");
                return;
            }

            transform.position = GridSystem.Instance.GetRandomValidPosition();

            if (!Snake.Instance.AStarSearchChecked())
            {
                GenerateRandomPosition(lim - 1);
            }
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
