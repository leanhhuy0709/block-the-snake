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

        public void GenerateRandomPosition()
        {
            var randomX = Random.Range(0, GridSystem.Instance.Width - 1) * GridSystem.Instance.CellSize;
            var randomY = Random.Range(0, GridSystem.Instance.Height - 1) * GridSystem.Instance.CellSize;

            transform.position = new Vector3(GridSystem.Instance.OffsetX + randomX, 
                GridSystem.Instance.OffsetY + randomY, 0);
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
