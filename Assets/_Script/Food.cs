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

        public void GenerateRandomPosition(int lim = 10)
        {
            if (lim == 0)
            {
                Debug.Log("Can't generate food");
                return;
            }

            var randomX = Random.Range(0, GridSystem.Instance.Width - 1) * GridSystem.Instance.CellSize;
            var randomY = Random.Range(0, GridSystem.Instance.Height - 1) * GridSystem.Instance.CellSize;

            var newPos = new Vector3(GridSystem.Instance.OffsetX + randomX, 
                GridSystem.Instance.OffsetY + randomY, 0);

            if (!Snake.Instance.IsValidPosition(newPos))
            {
                GenerateRandomPosition(lim - 1);
                return;
            }

            transform.position = newPos;
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
