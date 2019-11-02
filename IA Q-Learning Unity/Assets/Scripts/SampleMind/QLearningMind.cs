using Assets.Scripts.DataStructures;
using UnityEngine;

namespace Assets.Scripts.SampleMind
{
    public class QLearningMind : AbstractPathMind
    {
        public override void Repath()
        {

        }

        public override Locomotion.MoveDirection GetNextMove(BoardInfo boardInfo, CellInfo currentPos, CellInfo[] goals)
        {
            // si tabla no existe crear, si existe leer

            var val = Random.Range(0, 4);
            if (val == 0) return Locomotion.MoveDirection.Up;
            if (val == 1) return Locomotion.MoveDirection.Down;
            if (val == 2) return Locomotion.MoveDirection.Left;
            return Locomotion.MoveDirection.Right;
        }

        // método crear tabla (PONERLE NOMBRE SEMILLA, ASÍ TENEMOS EL FICHERO CORRESPONDIENTE CREADO)

        // método crear array con valores tabla, pillar ancho y alto de boardinfo

    }
}
