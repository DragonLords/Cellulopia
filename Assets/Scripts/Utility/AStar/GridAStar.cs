using System;
using UnityEngine;

namespace AStar
{
    public class GridAStar : MonoBehaviour
    {
        public Vector2 gridWorldSize;
        public float nodeRadius;
        Node[,] grid;
        public LayerMask wallMask;

        float nodeDiamater;
        int gridX, gridY;

        void Start()
        {
            nodeDiamater = nodeRadius * 2;
            gridX = Mathf.RoundToInt(gridWorldSize.x / nodeDiamater);
            gridY = Mathf.RoundToInt(gridWorldSize.y / nodeDiamater);
            CreateGrid();
        }

        private void CreateGrid()
        {
            grid=new Node[gridX, gridY];    
            Vector3 bottomLeft=transform.position-Vector3.right*gridWorldSize.x/2-Vector3.up*gridWorldSize.y/2;
            for (int x = 0; x < gridX; x++)
            {
                for (int y = 0; y < gridY; y++)
                {
                    Vector3 worldPoint = bottomLeft + Vector3.right * (x * nodeDiamater + nodeRadius)+Vector3.up*(y*nodeDiamater+nodeRadius);
                    
                    bool walkable = Physics2D.OverlapCircle(worldPoint, nodeRadius,wallMask)==null;
                    grid[x,y]=new Node(walkable,worldPoint);
                }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawCube(transform.position, gridWorldSize);

            if (grid != null)
            {
                foreach(var node in grid)
                {
                    Gizmos.color = node.walkbale ? new(52,0,121) : Color.red; 
                    Gizmos.DrawCube(node.worldPos, Vector3.one*(nodeDiamater-.1f));
                }
            }
        }
    }
}