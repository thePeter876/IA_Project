using Assets.Scripts.DataStructures;
using System.IO;
using UnityEngine;

namespace Assets.Scripts.SampleMind
{
    public class QLearningMind : AbstractPathMind
    {
        //public Loader cargador;
        const int numEpisodios = 200;
        const int numAcciones = 4;
        const float alfa = 0.5f;
        const float gamma = 0.5f;
        const int rMeta = 100;
        const int rMuro = -10;
        float[,,] tablaQ;

        bool primeraVez = true;

        void EscribirFichero(BoardInfo boardInfo, string nombreArchivo) 
        {
            StreamWriter fichero;
            fichero = new StreamWriter(/*"F:/SSD_LENOVO/Universidad/Primer Cuatrimestre/IA/REPO_IA/IA Q-Learning Unity/" + */
                nombreArchivo, false)/*File.AppendText(nombreArchivo)*/;
            string linea;

            for (int i = 0; i < boardInfo.NumColumns; i++)
            {
                for(int j = 0; j < boardInfo.NumRows; j++)
                {
                    linea = "";

                    for(int k = 0; k < numAcciones; k++)
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
                //index = 0;
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

        public void AlgoritmoQ(BoardInfo boardInfo, CellInfo currentPos, CellInfo[] goals) 
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
                //posicionX = currentPos.ColumnId;
                //posicionY = currentPos.RowId;

                while (/*boardInfo.CellInfos[ posicionX, posicionY ].Walkable &&*/ boardInfo.CellInfos[ posicionX, posicionY ] != goals[0])
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

                //string nombreArchivo = 
                Loader cargador = GameObject.Find("Loader").GetComponent<Loader>();
                string nombreArchivo = "" + cargador.seed + ".txt";


                if (!File.Exists(nombreArchivo))
                {
                    AlgoritmoQ(boardInfo, currentPos, goals);
                    EscribirFichero(boardInfo, nombreArchivo);
                }
                else
                {
                    LeerFichero(boardInfo, nombreArchivo);
                }


            }

            //var val = Random.Range(0, 4);
            int val = ObtenerMejorAccion(currentPos.ColumnId, currentPos.RowId);
            if (val == 0) return Locomotion.MoveDirection.Up;
            if (val == 1) return Locomotion.MoveDirection.Down;
            if (val == 2) return Locomotion.MoveDirection.Left;
            return Locomotion.MoveDirection.Right;


        }
    }
}
