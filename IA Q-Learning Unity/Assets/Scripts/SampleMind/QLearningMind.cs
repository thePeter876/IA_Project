using Assets.Scripts.DataStructures;
using UnityEngine;

namespace Assets.Scripts.SampleMind
{
    public class QLearningMind : AbstractPathMind
    {
        const int numEpisodios = 1000;
        const int numAcciones = 4;
        const float alfa = 0.5f;
        const float gamma = 0.5f;
        const int rMeta = 100;
        const int rMuro = -10;
        float[,,] tablaQ;

        public bool ComprobadorFichero() 
        {
            return false;
        }

        public void AlgoritmoQ(BoardInfo boardInfo, CellInfo[] goals) 
        {
            tablaQ = new float[boardInfo.NumColumns, boardInfo.NumRows, numAcciones];
            int[] posicion = new int[2];
            int accion;

            void ObtenerMaxQ(int[] pos)
            {
                float maxValue = float.MinValue;
                //tablaQ[pos[0], pos[1], 0]
            }

            for(int epis = 0; epis < numEpisodios; epis++)
            {
                posicion[0] = Random.Range(0, boardInfo.NumColumns);
                posicion[1] = Random.Range(0, boardInfo.NumRows);

                while (boardInfo.CellInfos [posicion[0], posicion[1] ].Walkable && boardInfo.CellInfos[ posicion[0], posicion[1] ] != goals[0])
                {
                    accion = Random.Range(0, numAcciones);
                    switch (accion)
                    {
                        case 0: // arriba
                            posicion[1]++;
                            break;
                        case 1: // abajo
                            posicion[1]--;
                            break;
                        case 2: // izquierda
                            posicion[0]--;
                            break;
                        case 3: // derecha
                            posicion[0]++;
                            break;
                    }

                }

            }
        }

        public override void Repath()
        {

        }

        public override Locomotion.MoveDirection GetNextMove(BoardInfo boardInfo, CellInfo currentPos, CellInfo[] goals)
        {
            /*
             * METER EN AWAKE
             * 
             * if (!ComprobadorFichero()) // IMPORTANTE programar la comprobación
            {
                AlgoritmoQ(boardInfo, goals);
            }
            */

            var val = Random.Range(0, 4);
            if (val == 0) return Locomotion.MoveDirection.Up;
            if (val == 1) return Locomotion.MoveDirection.Down;
            if (val == 2) return Locomotion.MoveDirection.Left;
            return Locomotion.MoveDirection.Right;
        }
    }
}
