using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Google.Protobuf;


using UnityEngine;

namespace Assets.Scripts.Contents
{
    public class ObjectManager : MonoBehaviour
    {
        public static ObjectManager Instance = new ObjectManager();
        Dictionary<int,GameObject> _objects = new Dictionary<int,GameObject>();

        public MyPlayerObject _myPlayer;

        private GameObject _myPlayerPrefab;
        private GameObject _playerPrefab;
        private GameObject _portalPrefab;
        private GameObject _monster1Prefab;
        private GameObject _monster2Prefab;
        private GameObject _monster3Prefab;
        public ObjectManager()
        {
            _myPlayerPrefab = Resources.Load<GameObject>("Prefabs/Object/Creature/MyPlayer");
            _playerPrefab = Resources.Load<GameObject>("Prefabs/Object/Creature/Player");
            _portalPrefab = Resources.Load<GameObject>("Prefabs/Object/Portal");
            _monster1Prefab = Resources.Load<GameObject>("Prefabs/Object/Creature/Monster1");
            _monster2Prefab = Resources.Load<GameObject>("Prefabs/Object/Creature/Monster2");
            _monster3Prefab = Resources.Load<GameObject>("Prefabs/Object/Creature/Monster3");
        }
        public void MakeMyPlayerObj(ObjectInfo info)
        {
            GameObject clone = Instantiate(_myPlayerPrefab);
            _objects[info.ObjectId] = clone;

            MyPlayerObject player = clone.GetComponent<MyPlayerObject>();
            player._info = info;
            player.Init();

            _myPlayer = player;
        }
        public void MakeObj(ObjectInfo info)
        {
            GameObject clone = null;
            switch (info.ObjectType)
            {
                case ObjectType.Player:
                    {
                        clone = Instantiate(_playerPrefab);
                        PlayerObject p = clone.GetComponent<PlayerObject>();
                        p._info = info;
                        p.Init();
                        MapManager.Instance.SetObjPos(p);
                        break;
                    }
                case ObjectType.Portal:
                    {
                        clone = Instantiate(_portalPrefab);
                        BaseObject o = clone.GetComponent<BaseObject>();
                        o._info = info;
                        o.Init();
                        break;
                    }
                case ObjectType.Monster:
                    {
                        switch (info.MonNum)
                        {
                            case 1:
                                {
                                    clone = Instantiate(_monster1Prefab);
                                    break;
                                }
                            case 2:
                                {
                                    clone = Instantiate(_monster2Prefab);
                                    break;
                                }
                            case 3:
                                {
                                    clone = Instantiate(_monster3Prefab);
                                    break;
                                }
                            default:break;
                        }
                        MovingObject mo = clone.GetComponent<MovingObject>();
                        mo._info = info;
                        mo.Init();
                        break;
                    }
                default: break;
            }
            _objects[info.ObjectId] = clone;
        }
        public bool RemoveObj(int objId)
        {
            GameObject go = _objects[objId];
            if (go == null)
                return false;
            if(go.GetComponent<BaseObject>()._objectId == _myPlayer._objectId)
                _myPlayer = null;
            GameObject.Destroy(go);
            return true;
        }
        public void RemoveAll()
        {
            foreach (GameObject go in _objects.Values)
            {
                GameObject.Destroy(go);
            }
            _objects.Clear();
            _myPlayer = null;
        }
        public GameObject GetObj(int objId)
        {
            GameObject go = null;
            if (_objects.TryGetValue(objId, out go) == false)
                return null;
            return go;
        }
    }
}
