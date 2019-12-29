using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.DataStructures;
using UnityEngine;


public class Node
{
    int cost;
    int fStar;
    Node parent;
    CellInfo cell;

    public Node(CellInfo cell, Vector2Int goal, Node parent = null)
    {
        //Se usa la distancia Manhattan como heurística para calcular la distancia aproximada a la meta desde la celda del nodo
        this.cell = cell;
        this.parent = parent;

        if (parent != null) this.cost = parent.cost++;
        else this.cost = 0;

        this.fStar = (int)(Mathf.Abs(goal.x - cell.GetPosition.x) + Mathf.Abs(goal.y - cell.GetPosition.y)) + cost; //Se calcula la f* como la distancia manhattan (heurística) + el coste
    }
    public int getFStar() { return fStar; }
    public Node getParent() { return parent; }
    public CellInfo getCellInfo() { return cell; }

    public bool equals(Node n) //La función equals entre nodos solo tiene en cuenta las posiciones
    {
        Vector2Int a = new Vector2Int((int)this.cell.GetPosition.x, (int)this.cell.GetPosition.y);
        Vector2Int b = new Vector2Int((int)n.cell.GetPosition.x, (int)n.cell.GetPosition.y);

        if (a == b) return true;
        return false;
    }

}

class NodeComparer : IComparer<Node>    //Clase comparadora de nodos en base a su f*
{
    public int Compare(Node a, Node b)
    {
        if (a.getFStar() == 0 || b.getFStar() == 0) return 0;

        // CompareTo() method 
        return a.getFStar().CompareTo(b.getFStar());
    }
}
