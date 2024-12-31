using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Google.Protobuf;

using Unity.VisualScripting;

using UnityEditor;

using UnityEngine;

namespace Assets.Scripts.Contents
{
    public class MapManager : MonoBehaviour
    {
        private static MapManager mapManager = null;
        public static MapManager Instance
        {   get
            {
                return mapManager;
            }
        }
        
        public Dictionary<int,GameObject> _maps = new Dictionary<int, GameObject>();
        public GameObject _currentMap = null;
        public int _currentMapId = 0;
        public Grid _grid = null;
        bool[,] _collision;
        PlayerObject[,] _players;
        int xMin;
        int xMax;
        int yMin;
        int yMax;
        int Xsize;
        int Ysize;
        public void Start()
        {
            if (mapManager == null)
                mapManager = this;
            GameObject[] maps;
            maps = Resources.LoadAll<GameObject>("Prefabs/Map");
            int mapSize = maps.Length;
            for(int i = 1;i<=mapSize;i++)
            {
                GameObject map = maps.FirstOrDefault((mapTemp) => mapTemp.name == $"Map_{i.ToString("D3")}");
                _maps.Add(i, map);
            }
        }
        public void SetMap(int mapId)
        {
            if (mapId == _currentMapId)
                return;
            GameObject map = null;
            if(_maps.TryGetValue(mapId, out map))
            {
                GameObject go = Instantiate(map);
                GameObject tileCollision = go.transform.Find("TileCollision").gameObject;
                tileCollision.SetActive(false);
                _currentMapId = mapId;
                _currentMap = go;
                _grid = _currentMap.GetComponent<Grid>();

                string mapNum = $"Map_{mapId.ToString("D3")}";
                TextAsset text = Resources.Load<TextAsset>($"Map/{mapNum}");
                string[] lines = text.text.Split(new char[] { '\n' ,'\r'},StringSplitOptions.RemoveEmptyEntries);

                xMin = int.Parse(lines[0]);
                xMax = int.Parse(lines[1]);
                yMin = int.Parse(lines[2]);
                yMax = int.Parse(lines[3]);
                
                Xsize = xMax - xMin+1;
                Ysize = yMax - yMin+1;

                _collision = new bool[Ysize, Xsize];
                _players = new PlayerObject[Ysize, Xsize];

                for (int i = 4;i < lines.Length;i++)
                {
                    for(int j = 0; j < lines[i].Length;j++)
                    {
                        if (lines[i][j] == '1')
                            _collision[i-4, j] = true;
                        else _collision[i-4, j] = false;
                    }
                }
            }
        }
        public void RemoveMap()
        {
            Destroy(_currentMap);
            _currentMap = null;
        }
        public PlayerObject CheckPlayer(Pos pos)
        {
            if (CheckCellRange(pos) == false)
                return null;
            Pos arrayPos = CellPosToArrayPos(pos);
            return _players[arrayPos.Y, arrayPos.X];
        }
        public bool CanGo(Pos cellPos)
        {
            Pos arrayPos = CellPosToArrayPos(cellPos);
            if (CheckCellRange(cellPos) == false)
                return false;
            if (_collision[arrayPos.Y, arrayPos.X] || _players[arrayPos.Y, arrayPos.X])
                return false;
            return true;
        }
        public void SetObjPos(PlayerObject p)
        {
            Pos arrayPos = CellPosToArrayPos(p._pos);
            _players[arrayPos.Y, arrayPos.X] = p;
        }
        public void ClearObjPos(Pos pos)
        {
            Pos arrayPos = CellPosToArrayPos(pos);
            _players[arrayPos.Y, arrayPos.X] = null;
        }
        private bool CheckCellRange(Pos pos)
        {
            return (yMin <= pos.Y && pos.Y <= yMax && xMin <= pos.X && pos.X <= xMax);
        }
        private Pos CellPosToArrayPos(Pos CellPos)
        {
            Pos arrayPos = new Pos();
            arrayPos.X = CellPos.X - xMin;
            arrayPos.Y = yMax - CellPos.Y;
            return arrayPos;
        }
        private Pos ArrayPosToCellPos(Pos arrayPos)
        {
            Pos CellPos = new Pos();
            CellPos.X = arrayPos.X + xMin;
            CellPos.Y = yMax - arrayPos.Y;
            return CellPos;
        }
    }
}
