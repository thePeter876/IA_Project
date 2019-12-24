using Assets.Scripts.DataStructures;
using UnityEngine;

namespace Assets.Scripts.SampleMind
{
    public class RandomMind : AbstractPathMind {
        public override void Repath()
        {
            
        }

        public override Locomotion.MoveDirection GetNextMove(BoardInfo boardInfo, CellInfo currentPos, CellInfo[] goals)
        {
            Vector2Int currentPosition = new Vector2Int(currentPos.ColumnId, currentPos.RowId);

            var val = Random.Range(0, 4);
            switch (val)
            {
                case 0:
                    if (currentPosition.y < boardInfo.NumRows - 1) {
                        if (boardInfo.CellInfos[currentPosition.x, currentPosition.y + 1].Walkable)
                            return Locomotion.MoveDirection.Up; 
                    } break;

                case 1:
                    if (currentPosition.y > 1){
                        if (boardInfo.CellInfos[currentPosition.x, currentPosition.y - 1].Walkable)
                            return Locomotion.MoveDirection.Down;
                    } break;

                case 2:
                    if (currentPosition.x > 1) {
                        if (boardInfo.CellInfos[currentPosition.x - 1, currentPosition.y].Walkable)
                            return Locomotion.MoveDirection.Left;
                    } break;

                case 3:
                    if (currentPosition.x < boardInfo.NumColumns - 1){
                        if (boardInfo.CellInfos[currentPosition.x + 1, currentPosition.y].Walkable)
                            return Locomotion.MoveDirection.Right;
                    } break;
            }
            
            return Locomotion.MoveDirection.None;
        }
    }
}
