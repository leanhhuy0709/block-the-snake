using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Script
{
    public class AStarState
    {
        public Vector2 Current;
        public Vector2 Goal;
        public List<Vector2> MoveList = new();
        private float _value;

        public void Initialization(Vector2 current, Vector2 goal, List<Vector2> moveList)
        {
            Current = current;
            Goal = goal;
            MoveList = moveList.GetRange(0, moveList.Count);
            _value = 0;
            CalculateValue();
        }

        private void CalculateValue()
        {
            var w = GridSystem.Instance.Width;
            var h = GridSystem.Instance.Height;
            var visit = new Dictionary<(int, int), int>();
            var tmpX = 0;
            var tmpY = 0;
            foreach (var pos in MoveList)
            {
                if (pos.x >= 0.9 * w + GridSystem.Instance.OffsetX
                    || pos.y >= 0.9 * h + GridSystem.Instance.OffsetY
                    || pos.x <= 0.1 * w + GridSystem.Instance.OffsetX
                    || pos.y <= 0.1 * h + GridSystem.Instance.OffsetY)
                    _value += 1.5f;
                else _value += 1f;
                visit[((int)pos.x, 0)] = 1;
                visit[(0, (int)pos.y)] = 1;
            }

            
            foreach (var (key, value) in visit)
            {
                if (key.Item1 == 0) tmpX += 1;
                else tmpY += 1;
            }

            if (tmpX >= w * 0.5f) _value *= 1.1f;
            else if (tmpX >= w * 0.75f) _value *= 1.25f;
            else if (tmpX >= w * 0.9f) _value *= 1.5f;
            
            if (tmpY >= h * 0.5f) _value *= 1.1f;
            else if (tmpY >= h * 0.75f) _value *= 1.25f;
            else if (tmpY >= h * 0.9f) _value *= 1.5f;
        }

        public float Value()
        {
            return _value;
            // var w = GridSystem.Instance.Width;
            // var h = GridSystem.Instance.Height;
            // if (Current.x >= 0.9 * w + GridSystem.Instance.OffsetX 
            //     || Current.y >= 0.9 * h + GridSystem.Instance.OffsetY 
            //     || Current.x <= 0.1 * w + GridSystem.Instance.OffsetX 
            //     || Current.y <= 0.1 * h + GridSystem.Instance.OffsetY)
            //     return (int)((MoveList.Count + Math.Abs(Goal.x - Current.x) + Math.Abs(Goal.y - Current.y)) * 0.9);
            // return (int)(MoveList.Count + Math.Abs(Goal.x - Current.x) + Math.Abs(Goal.y - Current.y));
        }   
    }

    public class AStarStateComparer : IComparer<AStarState>
    {
        public int Compare(AStarState x, AStarState y)
        {
            if (y != null && x != null && x.Value() > y.Value())
            {
                if (Snake.Instance.GoodAStar)
                    return -1;
                return 1;
            }

            if (y != null && x != null && x.Value() < y.Value())
            {
                if (Snake.Instance.GoodAStar)
                    return 1;
                return -1;
            }
            
            return 0;
        }
    }

    public class Snake : MonoBehaviour
    {
        public static Snake Instance;
        public Sprite Body;
        private List<GameObject> _snakeBody;
        private Vector2 _direction;

        private List<GameObject> _pool;
        
        private float _nextUpdate;
        private GameObject _defaultBody;

        private List<Vector2> _moveList;

        private readonly Dictionary<(int, int), bool> _visited = new();
        private readonly Dictionary<(int, int), bool> _hashTable = new();

        public bool IsSnakeOnSnake;
        public bool GoodAStar;
        
        private bool _check = false;

        private void Awake()
        {
            Instance = this;
            _snakeBody = new List<GameObject>();
            _pool = new List<GameObject>();
            _moveList = new List<Vector2>();
        }

        void Start()
        {
            InitDefaultBody();
            CreateSnake();
            
            _nextUpdate = Time.time;
        }

        void CreateSnake()
        {
            _direction = new Vector2(0, -1);
            var body = CreateNewBody(new Vector2(0, 0));
            _snakeBody.Add(body);   
            _snakeBody[0].GetComponent<SpriteRenderer>().color = Color.blue;
            _snakeBody[0].GetComponent<SpriteRenderer>().sortingOrder = 3;
            if (IsSnakeOnSnake) GridSystem.Instance.SetGrid(0, 0, true);
        }

        void Update()
        {
            // HandleInput();
            if (!(Time.time >= _nextUpdate)) return;
            // UpdateNear();
            AutoMove();
            if (_check) return;
            Move();
            
            _nextUpdate = Time.time + GameManager.Instance.Delay;
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private void AutoMove()
        {
            if (_moveList.Count == 0)
            {
                AStarSearchFood();
                var lim = 50;
                while (_moveList.Count == 0 && lim > 0)
                {
                    //Stuck
                    Debug.Log("Stuck");

                    HandleStuck();
                    if (!_check)
                    {
                        _check = true;
                        _nextUpdate = Time.time + 5;
                        return;
                    }
                    _check = false;
                    
                    
                    var pos = GridSystem.Instance.GetRandomValidPosition();
                    _hashTable.Clear();
                    foreach (var sb in _snakeBody)
                    {
                        var oldPos = sb.transform.position;
                        
                        GridSystem.Instance.SetGrid((int)oldPos.x, (int)oldPos.y, false);
                        
                        sb.transform.position = pos;
                    }

                    var tmp = _snakeBody.Count / 2 < 5 ? _snakeBody.Count/2:5;
                    tmp = 0;
                    for (var i = 0; i < tmp; i++)
                    {
                        RemoveSnakeBodyAt(_snakeBody.Count - 1);
                    }
                    
                    // UpdateNear();
                    // UpdateAll();
                    AStarSearchFood();
                    lim--;
                }

                if (_moveList.Count == 0 && lim == 0)
                {
                    Debug.Log("Can't handle stuck");
                }
            }
            _direction = _moveList[0];
            _moveList.RemoveAt(0);
        }

        private void HandleStuck()
        {
            
        }

        private void UpdateNear()
        {
            var head = GetSnakePosition();
            var x = (int)head.x;
            var y = (int)head.y;
            var isChange = false;
            for (var i = -2; i <= 2; i++)
            {
                for (var j = -2; j <= 2; j++)
                {
                    var h1 = GetGrid(x + i, y + j);
                    var h2 = GridSystem.Instance.GetGrid(x + i, y + j);
                    
                    if (h1 == h2) continue;
                    SetGrid(x + i, y + j, h2);
                    isChange = true;
                }
            }

            if (isChange)
            {
                var tmp = 3 < _moveList.Count ? 3 : _moveList.Count;
                for (var i = 0; i < tmp; i++)
                {
                    head = head + _moveList[i];
                    if (GetGrid((int)head.x, (int)head.y))
                    {
                        //Have wall on moveList
                        _moveList.Clear();
                        break;
                    }
                }
            }
        }

        private void UpdateAll()
        {
            _hashTable.Clear();
            foreach (var (key, value) in GridSystem.Instance.GetHashTable())
            {
                _hashTable[key] = value;
            }
        }
        
        // ReSharper disable Unity.PerformanceAnalysis
        void Move()
        {
            var newPos = GetSnakePosition() + _direction;
            if (!GridSystem.Instance.IsValidPosition(newPos) || Vector2.Distance(_direction, Vector2.zero) < 0.01f)
            {
                return;
            }
            
            foreach (var sb in _snakeBody)
            {
                var pos = sb.transform.position;
                GridSystem.Instance.SetGrid((int) pos.x, (int) pos.y, false);
            }
            
            var body = CreateNewBody(newPos);
            _snakeBody.Insert(1, body);

            (_snakeBody[0].transform.position, _snakeBody[1].transform.position) 
                = (_snakeBody[1].transform.position, _snakeBody[0].transform.position);

            if (IsSnakeOnSnake) GridSystem.Instance.SetGrid((int) newPos.x, (int) newPos.y, true);
            var isGenerateFood = false;
            if (IsEatFood(newPos))
            {
                isGenerateFood = true;
            }
            else
            {
                if (IsSnakeOnSnake)
                {
                    var tail = _snakeBody[^1];
                    var position = tail.transform.position;
                    GridSystem.Instance.SetGrid((int) position.x, (int) position.y, false);
                }
                RemoveSnakeBodyAt(_snakeBody.Count - 1);
            }

            foreach (var sb in _snakeBody)
            {
                var pos = sb.transform.position;
                GridSystem.Instance.SetGrid((int) pos.x, (int) pos.y, true);
            }
            
            if (isGenerateFood) Food.Instance.GenerateRandomPosition();
        }

        GameObject CreateNewBody(Vector2 position)
        {
            GameObject body;
            if (_pool.Count > 0)
            {
                body = _pool[^1];
                _pool.RemoveAt(_pool.Count - 1);
            }
            else
            {
                body = Instantiate(_defaultBody, _defaultBody.transform.position, _defaultBody.transform.rotation);
            }
            body.SetActive(true);
            body.transform.position = position;
            body.transform.SetParent(this.transform);
            
            return body;
        }

        void RemoveSnakeBodyAt(int index)
        {
            var body = _snakeBody[index];
            _snakeBody.RemoveAt(index);
            body.SetActive(false);
            _pool.Add(body);
        }
        
        private void HandleInput()
        {
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            {
                _direction = new Vector2(-1, 0);
            }
            else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            {
                _direction = new Vector2(1, 0);
            }
            else if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            {
                _direction = new Vector2(0, 1);
            }
            else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            {
                _direction = new Vector2(0, -1);
            }
        }

        private void InitDefaultBody()
        {
            var body = new GameObject("Snake");
            var spriteRenderer = body.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = Body;
            spriteRenderer.color = Color.yellow;
            body.transform.SetParent(this.transform);
            body.transform.position = new Vector3(0, 0, 0);
            body.SetActive(false);
            spriteRenderer.sortingOrder = 2;
            _defaultBody = body;
        }

        private bool IsEatFood(Vector2 head)
        {
            var foodPos = (Vector2) Food.Instance.transform.position;
            return Vector2.Distance(foodPos, head) < 0.001f;
        }

        private Vector2 GetSnakePosition()
        {
            return _snakeBody[0].transform.position;
        }

        private bool IsValidPosition(Vector2 position)
        {
            return GridSystem.Instance.IsValidPosition(position);
            var x = (int) position.x;
            var y = (int) position.y;

            var minX = -GridSystem.Instance.Width / 2;
            var maxX = GridSystem.Instance.Width / 2;
            var minY = -GridSystem.Instance.Height / 2;
            var maxY = GridSystem.Instance.Height / 2;
            if (position.x > maxX || position.y > maxY || position.x < minX || position.y < minY)
                return false;

            return !GetGrid(x, y);
        }

        private void AStarSearchFood()
        {
            _visited.Clear();

            
            
            List<AStarState> aStarStates = new();
            var tmp = new AStarState();

            var head = GetSnakePosition();

            var food = Food.Instance.transform.position;

            if (!IsValidPosition(food))
            {
                Debug.Log("Food is not valid!!!");
                return;
            }

            tmp.Initialization(head, food, new List<Vector2>());

            aStarStates.Add(tmp);

            _visited[((int) head.x,(int) head.y)] = true;

            var directionArray = new Vector2[4];
            directionArray[0] = new Vector2(0, 1);
            directionArray[1] = new Vector2(0, -1);
            directionArray[2] = new Vector2(1, 0);
            directionArray[3] = new Vector2(-1, 0);

            while (aStarStates.Count > 0)
            {
                aStarStates.Sort(new AStarStateComparer());
                AStarState currentState;

                if (Random.Range(0, 99) < 50 && aStarStates.Count > 1 && Math.Abs(aStarStates[^1].Value() - aStarStates[^2].Value()) < 0.0001f)
                {
                    currentState = aStarStates[^2];
                    aStarStates.RemoveAt(aStarStates.Count - 2);
                }
                else
                {
                    currentState = aStarStates[^1];
                    aStarStates.RemoveAt(aStarStates.Count - 1);
                }

                var currPos = currentState.Current;
                for (var i = 0; i < 4; i++)
                {
                    var newPos = currPos + directionArray[i];
                    var x = (int) newPos.x;
                    var y = (int) newPos.y;

                    if (IsValidPosition(newPos) && !_visited.ContainsKey((x, y)))
                    {
                        if (Vector2.Distance(currentState.Goal, newPos) < 0.01f) {
                            _moveList = currentState.MoveList.GetRange(0, currentState.MoveList.Count);
                            _moveList.Add(directionArray[i]);
                            return;
                        }
                        else
                        {
                            var tmpState = new AStarState();
                            tmpState.Initialization(newPos, food, currentState.MoveList);
                            tmpState.MoveList.Add(directionArray[i]);
                            aStarStates.Add(tmpState);
                            _visited[(x, y)] = true;
                        }
                    }
                }
            }
        }

        public bool AStarSearchChecked()
        {
            if (!GridSystem.Instance.IsValidPosition(Food.Instance.transform.position))
                return false;

            _visited.Clear();

            List<AStarState> aStarStates = new();
            var tmp = new AStarState();

            var head = GetSnakePosition();

            var food = Food.Instance.transform.position;

            tmp.Initialization(head, food, new List<Vector2>());

            aStarStates.Add(tmp);

            _visited[((int) head.x,(int) head.y)] = true;

            var directionArray = new Vector2[4];
            directionArray[0] = new Vector2(0, 1);
            directionArray[1] = new Vector2(0, -1);
            directionArray[2] = new Vector2(1, 0);
            directionArray[3] = new Vector2(-1, 0);

            while (aStarStates.Count > 0)
            {
                aStarStates.Sort(new AStarStateComparer());
                var currentState = aStarStates[^1];
                aStarStates.RemoveAt(aStarStates.Count - 1);

                var currPos = currentState.Current;
                for (var i = 0; i < 4; i++)
                {
                    var newPos = currPos + directionArray[i];
                    var x = (int) newPos.x;
                    var y = (int) newPos.y;

                    if (IsValidPosition(newPos) && !_visited.ContainsKey((x, y)))
                    {
                        if (currentState.Goal == newPos) {
                            return true;
                        }
                        else
                        {
                            var tmpState = new AStarState();
                            tmpState.Initialization(newPos, food, currentState.MoveList);
                            tmpState.MoveList.Add(directionArray[i]);
                            aStarStates.Add(tmpState);
                            _visited[(x, y)] = true;
                        }
                    }
                }
            }

            return false;
        }


        public List<Vector2> AStarFindFoodPosValid()
        {
            var foodList = new List<Vector2>();

            return foodList;
        }

        private void SetGrid(int x, int y, bool value)
        {
            _hashTable[(x, y)] = value;
        }

        private bool GetGrid(int x, int y)
        {
            if (!_hashTable.ContainsKey((x, y)))
                _hashTable[(x, y)] = false;

            return _hashTable[(x, y)];
        }
    }
}
