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
            // var visit = new Dictionary<(int, int), int>();
            // var tmpX = 0;
            // var tmpY = 0;
            var moveCount = 0f;
            foreach (var pos in MoveList)
            {
                if (pos.x >= 0.9 * w + GridSystem.Instance.OffsetX
                    || pos.y >= 0.9 * h + GridSystem.Instance.OffsetY
                    || pos.x <= 0.1 * w + GridSystem.Instance.OffsetX
                    || pos.y <= 0.1 * h + GridSystem.Instance.OffsetY)
                    moveCount += 1.5f;
                else moveCount += 1f;
                // visit[((int)pos.x, 0)] = 1;
                // visit[(0, (int)pos.y)] = 1;
            }

            _value = moveCount * 0.1f + Math.Abs(Goal.x - Current.x) + Math.Abs(Goal.y - Current.y);

            var directionArray = new Vector2[4];
            directionArray[0] = new Vector2(0, 1);
            directionArray[1] = new Vector2(0, -1);
            directionArray[2] = new Vector2(1, 0);
            directionArray[3] = new Vector2(-1, 0);


            var tmp = 0;
            for (var i = 0; i < 4; i++)
            {
                var pos = Current + directionArray[i];
                var value = GridSystem.Instance.GetGrid((int)pos.x, (int)pos.y);

                if (!GridSystem.Instance.IsInGridSystem(pos)) continue;
                if (value != 0 && (value == -1 || MoveList.Count <= value)) continue;
                if (MoveList.Contains(pos)) continue;
                
                tmp += 1;
            }

            switch (tmp)
            {
                case 1:
                    _value *= 1.5f; //Bad Move
                    break;
                case 0:
                    _value *= 2.25f; //Very bad move
                    break;
            }
            

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

        public bool GoodAStar;
        
        private bool _check;

        private bool _priorityLeft = true;
        
        public int SnakeLength;

        private void Awake()
        {
            Instance = this;
            _snakeBody = new List<GameObject>();
            _pool = new List<GameObject>();
            _moveList = new List<Vector2>();
            _check = false;
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
        }

        void Update()
        {
            // HandleInput();
            if (!(Time.time >= _nextUpdate)) return;
            AutoMove();
            // AutoMove2();
            if (_check) return;
            Move();

            SnakeLength = _snakeBody.Count;
            
            _nextUpdate = Time.time + GameManager.Instance.Delay;
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private void AutoMove()
        {
            if (_moveList.Count == 0)
            {
                _direction = Vector2.zero;
                AStarSearchFood();
                if (_moveList.Count == 0)
                {
                    // if (!_check)
                    // {
                    //     _check = true;
                    //     _nextUpdate = Time.time + 5;
                    //     return;
                    // }
                    // _check = false;
                    // Debug.Log("Stuck");
                    foreach (var sb in _snakeBody)
                    {
                        _moveList.Add(Vector2.zero);
                    }
                }
                
                
            }
            _direction = _moveList[0];
            _moveList.RemoveAt(0);
        }

        private void AutoMove2()
        
        {
            //Trial
            var directionArray = new Vector2[4];
            directionArray[0] = new Vector2(0, 1);
            directionArray[1] = new Vector2(0, -1);

            if (_priorityLeft)
            {
                directionArray[2] = new Vector2(-1, 0);
                directionArray[3] = new Vector2(1, 0);
            }
            else
            {
                directionArray[3] = new Vector2(-1, 0);
                directionArray[2] = new Vector2(1, 0);
                
            }
            
            

            var pos = GetSnakePosition();
            var tempDr = Vector2.zero;
            for (var i = 0; i < 4; i++)
            {
                var newPos = pos + directionArray[i];
                if (GridSystem.Instance.IsValidPosition(newPos) && Vector2.Distance(_direction, directionArray[i]) < 2)
                {
                    if ((int)newPos.y == (GridSystem.Instance.Height + 1) / 2)
                    {
                        tempDr = directionArray[i];
                        continue;
                    }
                    _direction = directionArray[i];
                    if (i == 3)
                        _priorityLeft = !_priorityLeft;
                    return;
                }
            }

            _direction = tempDr;

        }
        
        private void HandleStuck()
        {
            // Help snake move sat tuong!!!
            //
            // var pos = GetSnakePosition();
            //
            // var directionArray = new Vector2[4];
            // directionArray[0] = new Vector2(0, 1);
            // directionArray[1] = new Vector2(0, -1);
            // directionArray[2] = new Vector2(1, 0);
            // directionArray[3] = new Vector2(-1, 0);
            //
            // for (var j = 0; j < 10; j++)
            // {
            //     var a = Random.Range(0, 4);
            //     var b = Random.Range(0, 4);
            //
            //     (directionArray[a], directionArray[b]) = (directionArray[b], directionArray[a]);
            //     
            //     for (var i = 0; i < 4; i++)
            //     {
            //         var newPos = pos + directionArray[i];
            //         if (GridSystem.Instance.IsValidPosition(newPos))
            //         {
            //             pos = newPos;
            //             _moveList.Add(directionArray[i]);
            //             break;
            //         }
            //     }
            // }
            //
            
        }
        
        // ReSharper disable Unity.PerformanceAnalysis
        void Move()
        {
            var newPos = GetSnakePosition() + _direction;
            
            if (Vector2.Distance(_direction, Vector2.zero) < 0.01f)
            {
                // Do nothing
            }
            else if (!GridSystem.Instance.IsValidPosition(newPos))
            {
                Debug.Log("Can't move!");
                return;
            }

            
            
            foreach (var sb in _snakeBody)
            {
                var pos = sb.transform.position;
                GridSystem.Instance.SetGrid((int) pos.x, (int) pos.y, 0);
            }
            
            var body = CreateNewBody(newPos);
            _snakeBody.Insert(1, body);

            (_snakeBody[0].transform.position, _snakeBody[1].transform.position) 
                = (_snakeBody[1].transform.position, _snakeBody[0].transform.position);

            
            var isGenerateFood = false;
            if (IsEatFood(newPos))
            {
                isGenerateFood = true;
            }
            else
            {
                RemoveSnakeBodyAt(_snakeBody.Count - 1);
            }

            for (var sbIndex = 0; sbIndex < _snakeBody.Count; sbIndex++)
            {
                var sb = _snakeBody[sbIndex];
                var pos = sb.transform.position;
                GridSystem.Instance.SetGrid((int) pos.x, (int) pos.y, _snakeBody.Count - sbIndex);
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

        public bool AStarSearchFood(bool isUpdateMoveList = true)
        {
            _visited.Clear();
            
            List<AStarState> aStarStates = new();
            var tmp = new AStarState();

            var head = GetSnakePosition();

            var food = Food.Instance.transform.position;

            if (!GridSystem.Instance.IsValidPosition(food))
            {
                Debug.Log("Food is not valid!!!");
                return false;
            }

            tmp.Initialization(head, food, new List<Vector2>());

            aStarStates.Add(tmp);

            _visited[((int) head.x,(int) head.y)] = true;

            var directionArray = new Vector2[4];
            directionArray[0] = new Vector2(0, 1);
            directionArray[1] = new Vector2(0, -1);
            directionArray[2] = new Vector2(1, 0);
            directionArray[3] = new Vector2(-1, 0);

            var lim = GridSystem.Instance.Width * GridSystem.Instance.Height * 1000;
            while (aStarStates.Count > 0)
            {
                lim--;
                if (lim == 0)
                {
                    Debug.Log("AStar: Out of range!");
                    return false;
                }
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

                    if (_visited.ContainsKey((x, y))) continue;
                    if (!GridSystem.Instance.IsInGridSystem(newPos)) continue;
                    
                    var value = GridSystem.Instance.GetGrid(x, y);
                    if (value != 0 && (value == -1 || currentState.MoveList.Count <= value)) continue;
                    
                    
                    if (Vector2.Distance(currentState.Goal, newPos) < 0.01f)
                    {
                        if (!isUpdateMoveList) return true;
                        _moveList = currentState.MoveList.GetRange(0, currentState.MoveList.Count);
                        _moveList.Add(directionArray[i]);
                        return true;
                    }

                    var tmpState = new AStarState();
                    tmpState.Initialization(newPos, food, currentState.MoveList);
                    tmpState.MoveList.Add(directionArray[i]);
                    aStarStates.Add(tmpState);
                    _visited[(x, y)] = true;
                }
            }

            return false;
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
    }
}
