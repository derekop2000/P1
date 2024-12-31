using System.Collections;
using System.Collections.Generic;

using Assets.Scripts.Contents;

using Google.Protobuf;

using UnityEngine;

public class PlayerObject : MovingObject
{
    Animator _ani;
    SpriteRenderer _sr;

    #region ChatBox
    TextMesh _chatBox;
    GameObject _chatBoxBackGround;
    float _chatBoxBaseSize = 0.4f;
    int _chatOrderLayer = 20;
    Coroutine _chatBoxCor = null;
    public void Chat(string str)
    {
        if(_chatBoxCor != null)
        {
            StopCoroutine(_chatBoxCor);
        }
        _chatBoxCor = StartCoroutine("ChatCoroutine", str);
    }
    IEnumerator ChatCoroutine(string str)
    {
        _chatBox.text = str;
        _chatBoxBackGround.transform.localScale = new Vector3(_chatBoxBaseSize * str.Length, transform.localScale.y, 1);
        yield return new WaitForSeconds(3);

        _chatBox.text = "";
        _chatBoxBackGround.transform.localScale = new Vector3(0, transform.localScale.y, 1);

        _chatBoxCor = null;
    }
    #endregion
    public override void Init()
    {
        GameObject chatBox = transform.Find("ChatBox").gameObject;
        MeshRenderer mr =chatBox.GetComponent<MeshRenderer>();
        mr.sortingOrder = _chatOrderLayer;
        _chatBox = chatBox.GetComponent<TextMesh>();
        _chatBoxBackGround = chatBox.transform.Find("BackGround").gameObject;

        _ani = GetComponent<Animator>();
        _sr = GetComponent<SpriteRenderer>();
        base.Init();
    }
    protected override void UpdateAni()
    {
        switch (_state)
        {
            case ObjectState.Idle:
                {
                    if (_dir == ObjectDir.Down)
                    {
                        _ani.Play("p_idle_front");
                    }
                    else if (_dir == ObjectDir.Up)
                    {
                        _ani.Play("p_idle_back");
                    }
                    else if (_dir == ObjectDir.Left)
                    {
                        _ani.Play("p_idle_side");
                        _sr.flipX = false;
                    }
                    else if (_dir == ObjectDir.Right)
                    {
                        _ani.Play("p_idle_side");
                        _sr.flipX = true;
                    }
                    else;
                    break;
                }
            case ObjectState.Moving:
                {
                    if (_dir == ObjectDir.Down)
                    {
                        _ani.Play("p_move_front");
                    }
                    else if (_dir == ObjectDir.Up)
                    {
                        _ani.Play("p_move_back");
                    }
                    else if (_dir == ObjectDir.Left)
                    {
                        _ani.Play("p_move_side");
                        _sr.flipX = false;
                    }
                    else if (_dir == ObjectDir.Right)
                    {
                        _ani.Play("p_move_side");
                        _sr.flipX = true;
                    }
                    else;
                    break;
                }
            default: break;
        }
    }
    protected override void UpdateObj()
    {
        base.UpdateObj();
    }
    protected override void UpdateIdle()
    {
        ;
    }
    protected override void UpdateMoving()
    {
        base.UpdateMoving();
    }
    protected override void OnMovingCompleted()
    {
        _state = ObjectState.Idle;
    }
}
