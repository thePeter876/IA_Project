using Assets.Scripts.DataStructures;
using System.IO;
using UnityEngine;

namespace Assets.Scripts.SampleMind
{
    public class QLearningMind : AbstractPathMind
    {
        [Header("Controles del algoritmo")]
        [SerializeField] int numEpisodios = 200;            // Número de episodios
        [SerializeField] float alfa = 0.5f;                 // Parámetro alfa de la regla de aprendizaje Q-Learning
        [SerializeField] float gamma = 0.5f;                // Parámetro gamma de la regla de aprendizaje Q-Learning
        [SerializeField] int recompensaMeta = 100;         // Recompensa asignada por llegar a la meta
        [SerializeField] int recompensaMuro = -10;         // Recompensa asignada por llegar a un muro

        const int numAcciones = 4;      // Número de acciones posibles en cada estado (moverse hacia N, S, O, E)
        float[,,] tablaQ;               // Array tridimensional que contiene los valores de calidad. [posición x entorno, posición y entorno, acción a tomar]
        bool generadaTablaQ = false;    // Booleano que usamos para que la tablaQ se rellene una sola vez

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

                    fichero.WriteLine(linea);               // Se escribe la línea en el fichero
                }
            }

            fichero.Close();                                // Por último, se cierra el fichero
        }

        void LeerFichero(BoardInfo boardInfo, string nombreArchivo) // Función que lee un fichero y rellena el array tablaQ con los datos del fichero
        {
            tablaQ = new float[boardInfo.NumColumns, boardInfo.NumRows, numAcciones];
            StreamReader fichero = File.OpenText(nombreArchivo);
            string linea;
            float[] valoresQ = new float[4];

            for (int i = 0; i < boardInfo.NumColumns; i++)      // Se recorre el fichero leyendo cada línea
            {
                for (int j = 0; j < boardInfo.NumRows; j++)
                {
                    linea = fichero.ReadLine();

                    obtenerValoresQDeString(linea, valoresQ);   // Se obtienen los valores Q de la línea

                    for (int k = 0; k < numAcciones; k++)
                    {
                        tablaQ[i, j, k] = valoresQ[k];          // Se insertan los valores de calidad leídos en el array tablaQ
                    }
                }
            }
            fichero.Close();                                    // Por último, se cierra el fichero
        }

        void obtenerValoresQDeString(string linea, float[] valoresQ) // Recibe una string de números decimales separados por espacios y el array donde debe guardarlos
        {
            int index = 0;
            string numero;
            for(int i = 0; i < valoresQ.Length; i++)        // Por cada valor Q que hay que leer
            {
                numero = "";                            
                while (linea[index] != ' ')                 // Se va leyendo la línea hasta encontrar un espacio
                {
                    numero += linea[index];
                    index++;
                }
                index++;
                valoresQ[i] = float.Parse(numero);          // Se castea la string resultante que contiene el número a float y se guarda en el array
            }
        }

        int ObtenerMejorAccion(int posX, int posY)          // Recibe una posición y devuelve la mejor acción para ese estado
        {
            float valorMaximo = float.MinValue;

            int accion = 0;

            for (int i = 0; i < numAcciones; i++)           // Se recorren los valores de calidad del estado hasta encontrar el mayor
            {

                if(valorMaximo < tablaQ[posX, posY, i])
                {
                    valorMaximo = tablaQ[posX, posY, i];
                    accion = i;
                }

            }

            return accion;                                  // Se devuelve la acción correspondiente al mayor valor de calidad encontrado

        }

        public void AlgoritmoQ(BoardInfo boardInfo, CellInfo[] goals) // Se calculan los valores de la tabla Q de acuerdo al algoritmo de aprendizaje Q-Learning
        {
            tablaQ = new float[boardInfo.NumColumns, boardInfo.NumRows, numAcciones];

            int posicionX;
            int posicionY;
            int accion;

            float ObtenerRecompensa(int posX, int posY)                                                                                         // Esta función recibe unas coordenadas, y en base a la información del tablero, se devuelve la recompensa correspondiente de la casilla
            {
                float r = 0f;

                if (boardInfo.CellInfos[posX, posY] == goals[0]) r = recompensaMeta;
                else if (!boardInfo.CellInfos[posX, posY].Walkable) r = recompensaMuro;

                return r;
            }

            for(int epis = 0; epis < numEpisodios; epis++)                                                                                      // Para cada episodio se genera una posición aleatoria
            {
                posicionX = Random.Range(0, boardInfo.NumColumns);
                posicionY = Random.Range(0, boardInfo.NumRows);

                while (boardInfo.CellInfos[ posicionX, posicionY ] != goals[0])                                                                 // Se parte de la posición aleatoria, tomando decisiones al azar hasta llegar a la casilla meta
                {
                    int posicionXactual = posicionX;
                    int posicionYactual = posicionY;

                    accion = Random.Range(0, numAcciones);

                    switch (accion)
                    {
                        case 0: posicionY++; break; // arriba
                        case 1: posicionY--; break; // abajo
                        case 2: posicionX--; break; // izquierda
                        case 3: posicionX++; break; // derecha
                    }

                    posicionX = Mathf.Clamp(posicionX, 0, boardInfo.NumColumns - 1);                                                            // Clampeamos la posición entre los límites para no salir del dominio
                    posicionY = Mathf.Clamp(posicionY, 0, boardInfo.NumRows - 1);

                    float maxQ = tablaQ[posicionX, posicionY, ObtenerMejorAccion(posicionX, posicionY)];                                        // Máximo valor de calidad del estado siguiente
                    float recompensa = ObtenerRecompensa(posicionX, posicionY);                                                                 // Recompensa al tomar la acción escogida

                    float nuevoValorQ = (1 - alfa) * tablaQ[posicionXactual, posicionYactual, accion] + alfa * (recompensa + gamma * maxQ);     // Cálculo del valor Q según la regla de aprendizaje

                    tablaQ[posicionXactual, posicionYactual, accion] = nuevoValorQ;                                                             // Actualización del valor en el array tablaQ
                }

            }
        }

        void comprobarPosicion(BoardInfo boardInfo, CellInfo currentPos, int accion)    //Esta función recibe la posición actual, la información del dominio y la acción y comprueba si existe algún conflicto
        {
            if (!currentPos.Walkable)                       //Si el personaje se encuentra dentro de una pared, se muestra un mensaje de posible error
                Debug.Log("La posición actual no es andable, es posible que el personaje no pueda continuar, pero se intentará de todos modos");


            int nextPosX = currentPos.ColumnId;             //Se calcula la próxima posición
            int nextPosY = currentPos.RowId;

            switch (accion)
            {
                case 0: nextPosY++; break; // arriba
                case 1: nextPosY--; break; // abajo
                case 2: nextPosX--; break; // izquierda
                case 3: nextPosX++; break; // derecha
            }

            if (!boardInfo.CellInfos[nextPosX, nextPosY].Walkable)
                Debug.Log("La posición a la que se dirige el personaje no es andable, es posible que no sea posible llegar a la meta. Pruebe con otra semilla.");
        }

        public override void Repath()
        {
            
        }

        public override Locomotion.MoveDirection GetNextMove(BoardInfo boardInfo, CellInfo currentPos, CellInfo[] goals)
        {
            if (!generadaTablaQ)                                                        // Si la tablaQ no ha sido generada aún
            {
                
                generadaTablaQ = true;

                Loader cargador = GameObject.Find("Loader").GetComponent<Loader>();     // Encontramos el cargador para poder acceder a la semilla
                string nombreArchivo = cargador.seed + ".txt";           // Definimos el nombre del fichero en el cual esta la tabla Q como la ruta, el número de la semilla y el tipo de archivo


                if (!File.Exists(nombreArchivo))                                        // Si no existe, se crea la tabla Q y el fichero y se escriben en él los valores de la tabla Q
                {
                    AlgoritmoQ(boardInfo, goals);
                    EscribirFichero(boardInfo, nombreArchivo);
                }
                else                                                                    // Si ya existe el fichero, se leen sus datos y se guardan en el array de la tabla Q
                {
                    LeerFichero(boardInfo, nombreArchivo);
                }
            }

            
            int val = ObtenerMejorAccion(currentPos.ColumnId, currentPos.RowId);        // Se decide la siguiente acción en función del mejor valor de calidad para el estado actual y se devuelve el movimiento correspondiente
            comprobarPosicion(boardInfo, currentPos, val);                              // Se comprueba si existe algún conflicto
            if (val == 0) return Locomotion.MoveDirection.Up;
            if (val == 1) return Locomotion.MoveDirection.Down;
            if (val == 2) return Locomotion.MoveDirection.Left;
            return Locomotion.MoveDirection.Right;
        }
    }
}
