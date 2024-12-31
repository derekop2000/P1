using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Contents;
using Google.Protobuf;

using UnityEngine;

public class MovingObject : BaseObject
{
    protected float _speed = 5.0f;
    
    public MovingObject()
    {
        _state = ObjectState.Idle;
    }
    public override void Init()
    {
        base.Init();
    }
    protected override void UpdateObj()
    {
        switch (_state)
        {
            case ObjectState.Idle:
                UpdateIdle();
                break;
            case ObjectState.Moving:
                UpdateMoving();
                break;
            case ObjectState.Fight:
                break;
            default:
                break;
        }
        base.UpdateObj();
    }
    protected virtual void UpdateIdle()
    {
        ;
    }
    protected virtual void UpdateMoving()
    {
        Vector3 destPos = MapManager.Instance._grid.CellToWorld(new Vector3Int(_pos.X, _pos.Y)) + new Vector3(0.5f, 0.5f, 0);
        Vector3 dirV = destPos - transform.position;
        float distance = dirV.magnitude;
        float tickDistance = _speed * Time.deltaTime;
        if (distance <= tickDistance)
        {
            transform.position = destPos;
            OnMovingCompleted();
        }
        else
        {
            transform.position += dirV.normalized * tickDistance;
        }
    }
    protected virtual void OnMovingCompleted()
    {
        ;
    }
}
