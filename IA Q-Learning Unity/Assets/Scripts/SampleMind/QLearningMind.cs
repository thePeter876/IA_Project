using Assets.Scripts.DataStructures;
using System.IO;
using UnityEngine;

namespace Assets.Scripts.SampleMind
{
    public class QLearningMind : AbstractPathMind
    {
        const int numEpisodios = 200; // Número de episodios
        const int numAcciones = 4;    // Número de acciones posibles en cada estado (moverse hacia N, S, O, E)
        const float alfa = 0.5f;      // Parámetro alfa de la regla de aprendizaje Q-Learning
        const float gamma = 0.5f;     // Parámetro gamma de la regla de aprendizaje Q-Learning
        const int rMeta = 100;        // Recompensa asignada por llegar a la meta
        const int rMuro = -10;        // Recompensa asignada por llegar a un muro
        float[,,] tablaQ;             // Array tridimensional que contiene los valores de calidad. [posición x entorno, posición y entorno, acción a tomar]

        bool primeraVez = true;       // Booleano que usamos para que la tablaQ se rellene una sola vez

        void EscribirFichero(BoardInfo boardInfo, string nombreArchivo) // Función que crea un nuevo fichero y lo rellena con los datos de la tablaQ
        {
            StreamWriter fichero;
            fichero = new StreamWriter(nombreArchivo, false);
            string linea;

            for (int i = 0; i < boardInfo.NumColumns; i++)  // Recorrido del ancho y el alto del escenario
            {
                for(int j = 0; j < boardInfo.NumRows; j++)
                {
                    linea = "";

                    for(int k = 0; k < numAcciones; k++)    // Se escriben los 4 valores de calidad del estado correspondientes a cada acción, separados por espacios, en una sola línea
                    {
                        linea += tablaQ[i, j, k] + " ";
                    }

                    fichero.WriteLine(linea);
                }
            }

            fichero.Close();
        }

        void LeerFichero(BoardInfo boardInfo, string nombreArchivo) 
        {
            tablaQ = new float[boardInfo.NumColumns, boardInfo.NumRows, numAcciones];
            StreamReader fichero = File.OpenText(nombreArchivo);
            string linea;
            float[] valoresQ = new float[4];

            for (int i = 0; i < boardInfo.NumColumns; i++)
            {
                for (int j = 0; j < boardInfo.NumRows; j++)
                {
                    linea = fichero.ReadLine();

                    obtenerValoresQDeString(linea, valoresQ);

                    for (int k = 0; k < numAcciones; k++)
                    {
                        tablaQ[i, j, k] = valoresQ[k];
                    }
                }
            }
            
        }

        void obtenerValoresQDeString(string linea, float[] valoresQ) 
        {
            int index = 0;
            string numero;
            for(int i = 0; i < valoresQ.Length; i++)
            {
                numero = "";
                while (linea[index] != ' ')
                {
                    numero += linea[index];
                    index++;
                }
                index++;
                valoresQ[i] = float.Parse(numero);
            }
        }

        int ObtenerMejorAccion(int posX, int posY)
        {
            float valorMaximo = float.MinValue;

            int accion = 0;

            for (int i = 0; i < numAcciones; i++)
            {
                //valorMaximo = tablaQ[pos[0], pos[1], i] > valorMaximo ? tablaQ[pos[0], pos[1], i] : valorMaximo;

                if(valorMaximo < tablaQ[posX, posY, i])
                {
                    valorMaximo = tablaQ[posX, posY, i];
                    accion = i;
                }

            }

            return accion;

        }

        public void AlgoritmoQ(BoardInfo boardInfo, CellInfo[] goals) 
        {
            tablaQ = new float[boardInfo.NumColumns, boardInfo.NumRows, numAcciones];


            int posicionX;
            int posicionY;
            int accion;


            float ObtenerRecompensa(int posX, int posY)
            {
                float r = 0f;

                if (boardInfo.CellInfos[posX, posY] == goals[0]) r = rMeta;
                else if (!boardInfo.CellInfos[posX, posY].Walkable) r = rMuro;

                return r;
            }

            for(int epis = 0; epis < numEpisodios; epis++)
            {
                posicionX = Random.Range(0, boardInfo.NumColumns);
                posicionY = Random.Range(0, boardInfo.NumRows);

                while (boardInfo.CellInfos[ posicionX, posicionY ] != goals[0])
                {
                    int posicionXactual = posicionX;
                    int posicionYactual = posicionY;

                    accion = Random.Range(0, numAcciones);

                    switch (accion)
                    {
                        case 0: posicionY++; break; //arriba
                        case 1: posicionY--; break; //abajo
                        case 2: posicionX--; break; //izquierda
                        case 3: posicionX++; break; //derecha
                    }

                    posicionX = Mathf.Clamp(posicionX, 0, boardInfo.NumColumns - 1);
                    posicionY = Mathf.Clamp(posicionY, 0, boardInfo.NumRows - 1);

                    float maxQ = tablaQ[posicionX, posicionY, ObtenerMejorAccion(posicionX, posicionY)];
                    float recompensa = ObtenerRecompensa(posicionX, posicionY);

                    float nuevoValorQ = (1 - alfa) * tablaQ[posicionXactual, posicionYactual, accion] + alfa * (recompensa + gamma * maxQ);

                    tablaQ[posicionXactual, posicionYactual, accion] = nuevoValorQ;
                }

            }
        }

        public override void Repath()
        {

        }

        public override Locomotion.MoveDirection GetNextMove(BoardInfo boardInfo, CellInfo currentPos, CellInfo[] goals)
        {
            if (primeraVez)
            {
                primeraVez = false;

                Loader cargador = GameObject.Find("Loader").GetComponent<Loader>();
                string nombreArchivo = "./tablasQ/" + cargador.seed + ".txt";


                if (!File.Exists(nombreArchivo))
                {
                    AlgoritmoQ(boardInfo, goals);
                    EscribirFichero(boardInfo, nombreArchivo);
                }
                else
                {
                    LeerFichero(boardInfo, nombreArchivo);
                }
            }

            int val = ObtenerMejorAccion(currentPos.ColumnId, currentPos.RowId);
            if (val == 0) return Locomotion.MoveDirection.Up;
            if (val == 1) return Locomotion.MoveDirection.Down;
            if (val == 2) return Locomotion.MoveDirection.Left;
            return Locomotion.MoveDirection.Right;
        }
    }
}
