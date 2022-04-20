using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AStar
{
    internal class Node
    {
        public bool walkbale;
        public Vector3 worldPos;

        public Node(bool walkbale, Vector3 worldPos)
        {
            this.walkbale = walkbale;
            this.worldPos = worldPos;
        }
    }
}
