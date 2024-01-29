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

        private Dictionary<(int, int), bool> _hashTable;

        private void Awake()
        {
            Instance = this;
            _hashTable = new Dictionary<(int, int), bool>();
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

        public void SetGrid(int x, int y, bool value)
        {
            _hashTable[(x, y)] = value;
        }

        public bool GetGrid(int x, int y)
        {
            if (!_hashTable.ContainsKey((x, y)))
                _hashTable[(x, y)] = false;

            return _hashTable[(x, y)];
        }

        public bool IsValidPosition(Vector3 position)
        {
            var x = (int) position.x;
            var y = (int) position.y;

            var minX = -Width / 2;
            var maxX = Width / 2;
            var minY = -Height / 2;
            var maxY = Height / 2;
            if (position.x > maxX || position.y > maxY || position.x < minX || position.y < minY)
                return false;

            return !GetGrid(x, y); //HashTable[x, y] = true => Not valid
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public Vector2 GetRandomValidPosition(int lim = 50)
        {
            if (lim == 0)
            {
                Debug.Log("Can't generate position");
                return Vector2.zero;
            }
            var minX = -Width / 2;
            var maxX = Width / 2;
            var minY = -Height / 2;
            var maxY = Height / 2;
            
            var randomX = Random.Range(minX, maxX) * CellSize;
            var randomY = Random.Range(minY, maxY) * CellSize;

            var newPos = new Vector3(randomX, randomY, 0);

            if (!IsValidPosition(newPos))
            {
                return GetRandomValidPosition(lim - 1);
            }

            return newPos;
        }

        public Dictionary<(int, int), bool> GetHashTable()
        {
            return _hashTable;
        }
    }
}
