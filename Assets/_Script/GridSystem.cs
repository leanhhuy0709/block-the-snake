using System;
using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;
using Random = UnityEngine.Random;

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

        private Dictionary<(int, int), int> _hashTable;//-1 -> infinity

        private void Awake()
        {
            Instance = this;
            _hashTable = new Dictionary<(int, int), int>();
            Init();
        }

        void Start()
        {
            
        }

        void Init()
        {
            OffsetX = -((Width + 1)/2)*CellSize;
            OffsetY = -((Height + 1)/2)*CellSize;
            for (var i = 0; i <= Width; i++)
            {
                for (var j = 0; j <= Height; j++)
                {
                    var square = new GameObject("Grid");
                    var spriteRenderer = square.AddComponent<SpriteRenderer>();
                    spriteRenderer.sprite = Square;
                    spriteRenderer.color = (i + j) % 2 == 0 ? new Color(255,155,0,0.1f) : new Color(0,255,0,0.1f);
                    square.transform.position = new Vector3(OffsetX + i * CellSize,OffsetY + j * CellSize,0);
                    square.transform.SetParent(this.transform);
                    spriteRenderer.sortingOrder = 1;
                }
            }
        }

        public void SetGrid(int x, int y, int value)
        {
            _hashTable[(x, y)] = value;
        }

        public int GetGrid(int x, int y)
        {
            if (!_hashTable.ContainsKey((x, y)))
                return 0;
            
            return _hashTable[(x, y)];
        }

        public bool IsValidPosition(Vector3 position)
        {
            var x = (int) Math.Round(position.x);
            var y = (int) Math.Round(position.y);

            var minX = -(Width + 1) / 2;
            var maxX = (Width + 1) / 2;
            var minY = -(Height + 1) / 2;
            var maxY = (Height + 1) / 2;
            if (position.x > maxX || position.y > maxY || position.x < minX || position.y < minY)
                return false;

            return GetGrid(x, y) == 0; //HashTable[x, y] != 0 => Not valid
        }

        public bool IsInGridSystem(Vector3 position)
        {
            var x = (int) Math.Round(position.x);
            var y = (int) Math.Round(position.y);

            var minX = -(Width + 1) / 2;
            var maxX = (Width + 1) / 2;
            var minY = -(Height + 1) / 2;
            var maxY = (Height + 1) / 2;
            if (position.x > maxX || position.y > maxY || position.x < minX || position.y < minY)
                return false;

            return true;
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public Vector2 GetRandomValidPosition(int lim = 500)
        {
            var newPos = Vector2.zero;
            while (lim > 0)
            {
                var minX = -(Width + 1) / 2;
                var maxX = (Width + 1) / 2;
                var minY = -(Height + 1) / 2;
                var maxY = (Height + 1) / 2;

                var randomX = Random.Range(minX + 1, maxX);
                var randomY = Random.Range(minY + 1, maxY);

                newPos = new Vector2(randomX, randomY);
    
                if (!IsValidPosition(newPos))
                {
                    lim -= 1;
                    continue;
                }

                return newPos;
            }
            Debug.Log("Can't generate position");
            return newPos;
        }

        public Dictionary<(int, int), int> GetHashTable()
        {
            return _hashTable;
        }
    }
}
