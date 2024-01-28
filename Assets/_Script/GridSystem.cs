using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Script
{
    public class GridSystem : MonoBehaviour
    {
        public static GridSystem Instance;
        public int Width;
        public int Height;

        public int CellSize;

        public Sprite Square;

        public int OffsetX;
        public int OffsetY;

        private int[,] _map;

        private void Awake()
        {
            Instance = this;
            Init();
        }

        void Start()
        {
            
        }

        void Init()
        {
            OffsetX = -(Width/2)*CellSize;
            OffsetY = -(Height/2)*CellSize;
            for (var i = 0; i < Width; i++)
            {
                for (var j = 0; j < Height; j++)
                {
                    var square = new GameObject("Grid");
                    var spriteRenderer = square.AddComponent<SpriteRenderer>();
                    spriteRenderer.sprite = Square;
                    spriteRenderer.color = (i + j) % 2 == 0 ? new Color(255,155,0,0.1f) : new Color(0,255,0,0.1f);
                    square.transform.position = new Vector3(OffsetX + i * CellSize,OffsetY + j * CellSize,0);
                    square.transform.SetParent(this.transform);
                }
            }

            _map = new int[Width, Height];
        }
        
        // Update is called once per frame
        void Update()
        {
        
        }
        
    }
}
