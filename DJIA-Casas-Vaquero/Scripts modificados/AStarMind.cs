using Assets.Scripts.DataStructures;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Assets.Scripts.SampleMind
{
    public class AStarMind : AbstractPathMind {
        List<Node> nodes = new List<Node>();                                            //Lista abierta de los nodos
        List<Node> visitedNodes = new List<Node>();                                     //Lista de todos los nodos visitados (para comprobar ciclos generales)
        List<Node> currentPlan = new List<Node>();                                      //Lista de nodos que guarda el plan actual que debe seguir el personaje
        Node finalNode = null;                                                          //Nodo final de la búsqueda (meta o enemigo)
        NodeComparer nodeComparer = new NodeComparer();                                 //Comparador de nodos, para poder ordenar la lista abierta
        bool searchingEnemies = false;                                                  
        Vector2Int finalGoal;                                                           //En esta variable se guarda de forma global la posición de la meta final del personaje
        

        private void Start()                                                            //Al inicio de la ejecución
        {
            RandomMind rm = GetComponent<RandomMind>();                                 //Se obtiene el componente RandomMind en caso de que esté presente
            if (rm)                                                                     //Si existe el componente
            {
                Debug.Log("Eliminando el script randomMind ya que no se va a usar");
                Destroy(rm);                                                            //Se destruye el componente para que no pueda generar ningún tipo de conflicto
            }
        }

        public override void Repath()
        {

        }

        public List<Node> setPlan(BoardInfo boardInfo, CellInfo currentPos, Vector2Int goal) //Esta función crea un plan para llegar a una posición destino desde una posición origen
        {
            List<Node> plan = new List<Node>();

            //NODO INICIAL 
            Node firstNode = new Node(currentPos, goal);                            //Se crea el primer nodo de la lista, ubicado en el origen
            nodes.Add(firstNode);  
            visitedNodes.Add(firstNode);

            while(finalNode == null && nodes.Count > 0) expandNode(boardInfo, goal);//Se expande el árbol hasta que se encuentre una solución o se agoten las posibilidades

            if (nodes.Count == 0)               //Si no hay un camino posible
            {
                Debug.Log("No se ha podido encontrar un camino hacia el objetivo");
            }
            else
            {
                Node next = finalNode;
                while (next != null)            //Se recorren los padres de finalNode de forma sucesiva para crear el plan hasta llegar al origen
                {
                    plan.Add(next);
                    next = next.getParent();
                }
                plan.RemoveAt(plan.Count - 1);  //Al ser el nodo inicial la posición original del personaje, 
                                                //la descartamos, ya que no queremos que el jugador vaya a su propia posición
            }

            //Se limpian las variables auxiliares que se han usado para la creación del plan
            finalNode = null;
            nodes.Clear();
            visitedNodes.Clear();
            return plan;
        }

        public void expandNode(BoardInfo boardInfo, Vector2Int goal)
        {
            nodes.Sort(nodeComparer);   //Se ordena la lista abierta
            Node first = nodes[0];      //Se guarda una referencia al nodo a expandir, el primero de la lista abierta ordenada
            nodes.RemoveAt(0);          //Se quita dicho de nodo de la lista abierta
            Vector2Int currentPos = new Vector2Int((int)first.getCellInfo().GetPosition.x, (int)first.getCellInfo().GetPosition.y);

            if (currentPos == goal)     //Si el nodo a expandir coincide con la meta, se ha encontrado una solución y se termina
            {
                finalNode = first;
                return;
            }

            Vector2Int[] children = new Vector2Int[4];                              //Se va a usar para guardar la posición que tendrían los nodos hijos
            
            children[0] = new Vector2Int(currentPos.x - 1, currentPos.y);
            children[1] = new Vector2Int(currentPos.x + 1, currentPos.y);
            children[2] = new Vector2Int(currentPos.x,     currentPos.y + 1);
            children[3] = new Vector2Int(currentPos.x,     currentPos.y - 1);

            foreach (Vector2Int v in children) checkNode(boardInfo, v, goal, first); //Para cada nodo hijo posible se comprueba si debe añadirse a la lista abierta
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
            foreach (Node n in visitedNodes) if (n.equals(child)) return;

            //Si estamos buscando un enemigo, no podemos pasar por la meta
            if (searchingEnemies && finalGoal == nodePos) return; 

            nodes.Add(child);
            visitedNodes.Add(child);
        }

        public override Locomotion.MoveDirection GetNextMove(BoardInfo boardInfo, CellInfo currentPos, CellInfo[] goals)
        {
            List<Vector2Int> enemyPositions = new List<Vector2Int>();                           //Lista para guardar las posiciones de los enemigos
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");                  //Array para guardar los enemigos

            if (currentPlan.Count == 0)                                                         //Si no hay plan
            {
                finalGoal = new Vector2Int(goals[0].ColumnId, goals[0].RowId);
                if(enemies.Length > 0)                                                          //Si hay enemigos, deben ser perseguidos primero
                {
                    searchingEnemies = true;
                    foreach (GameObject e in enemies)                                           //Se rellena la lista de posiciones de enemigos, que se usarán como objetivos para crear los planes
                    {
                        CellInfo enemyPos = e.GetComponent<EnemyBehaviour>().CurrentPosition();
                        enemyPositions.Add(new Vector2Int(enemyPos.ColumnId, enemyPos.RowId));
                    }

                    foreach (Vector2Int g in enemyPositions)                                    //Se calculan los planes a los enemigos y se usa el plan más corto
                    {
                        List<Node> plan = setPlan(boardInfo, currentPos, g);
                        if (plan.Count < currentPlan.Count || currentPlan.Count == 0) currentPlan = plan; //Si no hay plan o el plan hasta este enemigo es más corto que el anterior, se usa como plan actual
                    }
                }
                else                                                                            //Si no hay enemigos, se crea un plan a la meta
                {
                    searchingEnemies = false;
                    currentPlan = setPlan(boardInfo, currentPos, finalGoal);
                }
            }

            if (currentPlan.Count != 0) //Si existe un plan
            {
                CellInfo nextMove = currentPlan[currentPlan.Count-1].getCellInfo(); //Se obtiene la celda a la que deberá dirigirse el jugador
                currentPlan.RemoveAt(currentPlan.Count - 1);                        //Se saca esta acción del plan

                int diffX = (int)(nextMove.GetPosition.x - currentPos.GetPosition.x); //Se calcula la diferencia en X de la posición actual a la destino
                int diffY = (int)(nextMove.GetPosition.y - currentPos.GetPosition.y); //Se calcula la diferencia en Y de la posición actual a la destino

                if (searchingEnemies) currentPlan.Clear(); //Si hay enemigos se destruye el plan ya que se tendrá que reconstruir en la siguiente iteración al no ser los enemigos estáticos

                //En base a las diferencias entre la posición actual y la destino se devuelve la dirección correspondiente
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

            Debug.Log("El personaje se detiene porque no está definido el siguiente movimiento");
            return Locomotion.MoveDirection.None;
        }
    }
}