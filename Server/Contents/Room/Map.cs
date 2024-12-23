using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Google.Protobuf;
using Server.Contents.Object;

namespace Server.Contents.Room
{
    public class Map
    {
        bool[,] _collisions;
        GameObject[,] _objs;
        public int _mapId;
        int xMin;
        int xMax;
        int yMin;
        int yMax;
        int xSize;
        int ySize;
        public void Init(int mapId)
        {
            _mapId = mapId;
            string file = $"../../../../Common/Map/Map_{mapId.ToString("D3")}.txt";
            string[] lines = File.ReadAllLines(file);
            xMin = int.Parse(lines[0]);
            xMax = int.Parse(lines[1]);
            yMin = int.Parse(lines[2]);
            yMax = int.Parse(lines[3]);
            xSize = xMax - xMin+1;
            ySize = yMax - yMin+1;

            _collisions = new bool[ySize,xSize];
            _objs = new GameObject[ySize,xSize];
            for(int i=4;i<lines.Length; i++)
            {
                string line = lines[i];
                for(int j=0;j<xSize;j++)
                {
                    if (line[j] == '1')
                        _collisions[i - 4, j] = true;
                    else
                        _collisions[i - 4, j] = false;
                }
            }
        }
        public int CanGo(GameObject go, Pos pos) // 0 false, 1 true, 2 collision
        {
            Pos arrayPos = CellPosToArrayPos(pos);
            if (go._pos == pos)
                return 0;
            if (!(yMin <= pos.Y && pos.Y <= yMax && xMin <= pos.X && pos.X <= xMax))
                return 0;
            if (_collisions[arrayPos.Y, arrayPos.X])
                return 0;
            if (_objs[arrayPos.Y, arrayPos.X] != null)
            {
                ObjectType type = go._type;
                GameObject destGo = _objs[arrayPos.Y, arrayPos.X];
                switch (type)
                {
                    case ObjectType.Player:
                        {
                            if (destGo._type == ObjectType.Player)
                                return 0;
                            else if (destGo._type == ObjectType.Monster)
                                return 2;
                            else if (destGo._type == ObjectType.Portal)
                                return 2;
                            break;
                        }
                    case ObjectType.Monster:
                        {
                            if (destGo._type == ObjectType.Player)
                                return 2;
                            else if (destGo._type == ObjectType.Monster)
                                return 0;
                            else if (destGo._type == ObjectType.Portal)
                                return 0;
                            break;
                        }
                    default:
                        break;
                }
            }
            return 1;
        }
        public void SetObjPos(GameObject go)
        {
            Pos arrayPos = CellPosToArrayPos(go._pos);
            _objs[arrayPos.Y, arrayPos.X] = go;
        }
        public void ClearObjPos(Pos pos)
        {
            Pos arrayPos = CellPosToArrayPos(pos);
            _objs[arrayPos.Y, arrayPos.X] = null;
        }

        public GameObject GetObj(Pos pos)
        {
            Pos arrayPos = CellPosToArrayPos(pos);
            return _objs[arrayPos.Y, arrayPos.X];
        }
        public Pos GetCenterCellPos()
        {
            Pos pos = new Pos();
            pos.Y = (yMin + yMax) / 2;
            pos.X = (xMin + xMax) / 2;
            return pos;
        }
        public Pos GetRightCellPos()
        {
            Pos pos = new Pos();
            pos.Y = (yMin + yMax) / 2;
            pos.X = xMax - 1;
            return pos;
        }
        public Pos GetLeftCellPos()
        {
            Pos pos = new Pos();
            pos.Y = (yMin + yMax) / 2;
            pos.X = xMin +1 ;
            return pos;
        }
        private Pos CellPosToArrayPos(Pos cellPos)
        {
            Pos arrayPos = new Pos();
            arrayPos.Y = yMax - cellPos.Y;
            arrayPos.X = cellPos.X - xMin;
            return arrayPos;
        }

        private Pos ArrayPosToCellPos(Pos ArrayPos)
        {
            Pos cellPos = new Pos();
            cellPos.Y = yMax - ArrayPos.Y;
            cellPos.X = xMin + ArrayPos.X;
            return cellPos;
        }
    }
}
