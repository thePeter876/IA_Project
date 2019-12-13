using Assets.Scripts.DataStructures;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Assets.Scripts.SampleMind
{
    public class AStarMind : AbstractPathMind {

        //List<Node> nodes = new List<Node>();
        List<Node> nodes = new List<Node>();
        List<Node> currentPlan = new List<Node>();
        Node finalNode = null;
        NodeComparer nodeComparer = new NodeComparer();

        public override void Repath()
        {
            nodes.Clear();
            Debug.Log("Limpiando");
        }

        public void setPlan(BoardInfo boardInfo, CellInfo currentPos, CellInfo[] goals)
        {
            Vector2Int goal  = new Vector2Int((int)goals[0].GetPosition.x,   (int)goals[0].GetPosition.y);

            nodes.Add(new Node(currentPos, goal)); //NODO INICIAL            
            while(finalNode == null) expandNode(boardInfo, goal);

            Node next = finalNode;
            while(next != null)
            {
                currentPlan.Add(next);
                next = next.getParent();
            }
            currentPlan.RemoveAt(currentPlan.Count - 1); //Al ser el nodo inicial la posición original del personaje, 
                                                         //la descartamos, ya que no queremos que el jugador vaya a su propia posición

        }

        //EXPAND NODE NO DEBE RECIBIR CURRENT POS, SOLO USAR EL PRIMER NODO DE LA LISTA

        public void expandNode(BoardInfo boardInfo, Vector2Int goal)
        {
            //CURRENT POS NO SE RECIBE, ES LA POSICIÓN DEL PRIMER NODO DE LA LISTA TRAS ORDENAR
            
            //ORDENAR LA LISTA

            nodes.Sort(nodeComparer);
            Node first = nodes[0];      //REFERENCIA AL NODO 0
            nodes.RemoveAt(0);          //QUITAR NODO 0 DE LA LISTA
            Vector2Int currentPos = new Vector2Int((int)first.getCellInfo().GetPosition.x, (int)first.getCellInfo().GetPosition.y);

            if (currentPos == goal)
            {
                finalNode = first;
                return;
            }


            Vector2Int[] children = new Vector2Int[4];
            children[0] = new Vector2Int(currentPos.x - 1, currentPos.y);
            children[1] = new Vector2Int(currentPos.x + 1, currentPos.y);
            children[2] = new Vector2Int(currentPos.x,     currentPos.y + 1);
            children[3] = new Vector2Int(currentPos.x,     currentPos.y - 1);
            

            foreach (Vector2Int v in children) checkNode(boardInfo, v, goal, first);

            //expandNode(boardInfo, goal);
            //SE DEBE EXPANDIR EN EXPAND NODE, NO AQUÍ, YA QUE NO QUEREMOS EXPANDIR TODOS LOS HIJOS, SOLO EL QUE TENGA MENOR F*
        }

        public void checkNode(BoardInfo boardInfo, Vector2Int nodePos, Vector2Int goal, Node parent)
        {
            //Comprobar si el nodo queda fuera del entorno
            if (nodePos.x < 0 || nodePos.x > boardInfo.NumColumns - 1) return;
            if (nodePos.y < 0 || nodePos.y > boardInfo.NumRows - 1)    return;

            //Comprobar si el nodo es un obstáculo
            if (!boardInfo.CellInfos[nodePos.x, nodePos.y].Walkable) return;

            Node child = new Node(boardInfo.CellInfos[nodePos.x, nodePos.y], goal, parent);

            //Comprobar si el nodo hijo ya está en la lista (EVITAR CICLOS)
            foreach (Node n in nodes) if (n.equals(child)) return;

            nodes.Add(child);
        }

        public override Locomotion.MoveDirection GetNextMove(BoardInfo boardInfo, CellInfo currentPos, CellInfo[] goals)
        {
            //int val = 0;

            //currentPlan.Add(new Node(boardInfo.CellInfos[(int)(currentPos.GetPosition.x), (int)(currentPos.GetPosition.y+1)], goals[0]));

            if (currentPlan.Count == 0)
            {
                setPlan(boardInfo, currentPos, goals);
            }

            if (currentPlan.Count != 0)
            {
                CellInfo nextMove = currentPlan[currentPlan.Count-1].getCellInfo();
                currentPlan.RemoveAt(currentPlan.Count - 1);

                int diffX = (int)(nextMove.GetPosition.x - currentPos.GetPosition.x);
                int diffY = (int)(nextMove.GetPosition.y - currentPos.GetPosition.y);

                switch (diffY)
                {
                    case  1: return Locomotion.MoveDirection.Up;
                    case -1: return Locomotion.MoveDirection.Down;
                }

                switch (diffX)
                {
                    case -1: return Locomotion.MoveDirection.Left;
                    case  1: return Locomotion.MoveDirection.Right;
                }
            }


            //if (val == 0) return Locomotion.MoveDirection.Up;
            //if (val == 1) return Locomotion.MoveDirection.Down;
            //if (val == 2) return Locomotion.MoveDirection.Left;
            Debug.Log("Me voy a la derecha pero porque no me he metido en el switch");
            return Locomotion.MoveDirection.Right;
        }
    }
}
