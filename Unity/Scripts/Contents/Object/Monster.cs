using System;
using System.Collections;
using System.Collections.Generic;

using Assets.Scripts.Contents;

using Google.Protobuf;

using UnityEngine;

public class Monster : MovingObject
{
    public ObjectCP _cp;

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
    protected override void UpdateIdle()
    {
        base.UpdateIdle();
    }
    protected override void UpdateMoving()
    {
        base.UpdateMoving();
    }

    protected override void OnMovingCompleted()
    {
        ;
    }
}
