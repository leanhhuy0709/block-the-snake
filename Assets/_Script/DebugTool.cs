using UnityEngine;

namespace _Script
{
    public class DebugTool : MonoBehaviour
    {
        public static DebugTool Instance;
        public bool IsDebug;
        public Vector3 P;
        private void Awake()
        {
            Instance = this;
        }
        
        void Start()
        {
        
        }

        void Update()
        {
            if (IsDebug)
            {
                Debug.Log(GridSystem.Instance.IsValidPosition(P));
            }
        }
    }
}
