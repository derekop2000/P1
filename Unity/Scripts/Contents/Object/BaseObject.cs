using System;
using System.Collections;
using System.Collections.Generic;

using Assets.Scripts.Contents;

using Google.Protobuf;

using Unity.VisualScripting.FullSerializer;

using UnityEngine;

public class BaseObject : MonoBehaviour
{
    public ObjectInfo _info = new ObjectInfo();
    public ObjectState _state
    {
        get { return _info.ObjectState; }
        set
        {
            if (_info.ObjectState != value)
            {
                _info.ObjectState = value;
                UpdateAni();
                //ChangedState();
            }
        }
    }
    public int _objectId { get { return _info.ObjectId; } set { _info.ObjectId = value; } }
    public ObjectType _type { get { return _info.ObjectType; } set { _info.ObjectType = value; } }
    public Pos _pos { get { return _info.ObjectPos; } set { _info.ObjectPos.X = value.X; _info.ObjectPos.Y = value.Y; _dir = value.Dir; } }
    protected ObjectDir _dir
    {
        get{ return _pos.Dir; }
        set
        {
            if (_pos.Dir != value)
            {
                _pos.Dir = value;
                UpdateAni();
                ChangedDir();
            }
        }
    }
    protected virtual void ChangedDir()
    {
        ;
    }
    protected virtual void ChangedState()
    {
        ;
    }
    public virtual void Init()
    {
        transform.position = MapManager.Instance._grid.CellToWorld(new Vector3Int(_pos.X, _pos.Y, 0)) + new Vector3(0.5f, 0.5f, 0f);
        if (_type == ObjectType.Portal)
            transform.position += Vector3.down;
    }
    void Start()
    {

    }

    void Update()
    {
        UpdateObj();
    }
    protected virtual void UpdateObj()
    {
        ;
    }
    protected virtual void UpdateAni()
    {
        ;
    }
}
