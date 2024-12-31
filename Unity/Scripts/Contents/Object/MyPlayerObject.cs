using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Contents;
using Assets.Scripts.Contents.Object;

using Google.Protobuf;

using UnityEngine;

public class MyPlayerObject : PlayerObject
{
    public int _hpPotionCnt = 0;

    public MonsterCp _mainMon;
    public Dictionary<int, MonsterCp> _mons = new Dictionary<int, MonsterCp>();

    bool _keyPressed = false;
    public static int _maxMonAmount = 3;

    public PlayerObject _frontPlayer = null;
    public PlayerObject GetFrontPlayer()
    {
        Pos front = new Pos();
        front.Y = _pos.Y;
        front.X = _pos.X;
        switch (_dir)
        {
            case ObjectDir.Down:
                {
                    front.Y--;
                    break;
                }
            case ObjectDir.Up:
                {
                    front.Y++;
                    break;
                }
            case ObjectDir.Left:
                {
                    front.X--;
                    break;
                }
            case ObjectDir.Right:
                {
                    front.X++;
                    break;
                }
            default:
                break;
        }
        return MapManager.Instance.CheckPlayer(front);
    }

    public void AlterMainMon(int id)
    {
        MonsterCp cp = null;
        if (_mons.TryGetValue(id, out cp) == false)
            return;
        _mainMon = cp;
    }
    public void AddMon(MonsterCp m)
    {
        if (_mons.Count == _maxMonAmount)
            return;
        _mons.Add(m._objId, m);
    }
    public void RemoveMon(MonsterCp m)
    {
        _mons.Remove(m._objId);
    }
    public void RemoveMonAll()
    {
        _mons.Clear();
        _mainMon = null;
    }
    public override void Init()
    {
        base.Init();
    }
    private void LateUpdate()
    {
        Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
    }

    protected override void UpdateObj()
    {
        if (_state == ObjectState.Manual)
        {
            UpdateManual();
            return;
        }
        base.UpdateObj();
    }
    private void UpdateManual()
    {
        CheckUiKey();
        int selectNum = 0;
        if (Input.GetKeyDown(KeyCode.Alpha1) && Interaction.Instance.IsOnMonStore())
        {
            selectNum = 1;
            int index = 1;
            foreach(MonsterCp m in _mons.Values)
            {
                if(index == selectNum)
                {
                    if (m._objId == _mainMon._objId)
                        return;
                    C_AlterMainMon packet = new C_AlterMainMon();
                    packet.Id = m._objId;
                    NetworkManager.Instance.Send(packet);
                    Interaction.Instance.OffAllStore();
                    return;
                }
                index++;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && Interaction.Instance.IsOnMonStore())
        {
            selectNum = 2;
            int index = 1;
            foreach (MonsterCp m in _mons.Values)
            {
                if (index == selectNum)
                {
                    if (m._objId == _mainMon._objId)
                        return;
                    C_AlterMainMon packet = new C_AlterMainMon();
                    packet.Id = m._objId;
                    NetworkManager.Instance.Send(packet);
                    Interaction.Instance.OffAllStore();
                    return;
                }
                index++;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) && Interaction.Instance.IsOnMonStore())
        {
            selectNum = 3;
            int index = 1;
            foreach (MonsterCp m in _mons.Values)
            {
                if (index == selectNum)
                {
                    if (m._objId == _mainMon._objId)
                        return;
                    C_AlterMainMon packet = new C_AlterMainMon();
                    packet.Id = m._objId;
                    NetworkManager.Instance.Send(packet);
                    Interaction.Instance.OffAllStore();
                    return;
                }
                index++;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4) && Interaction.Instance.IsOnItemStore())
        {
            if (_mainMon == null || _hpPotionCnt == 0)
                return;
            C_UsePotion packet = new C_UsePotion();
            NetworkManager.Instance.Send(packet);
            Interaction.Instance.OffAllStore();
        }
    }
    private void CheckUiKey()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            Interaction.Instance.OnOffItemStore();
        }
        else if (Input.GetKeyDown(KeyCode.O))
        {
            Interaction.Instance.OnOffMonStore();
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            Interaction.Instance.OnOffChatBox();
        }
        else;
    }
    private void CheckPressKey()
    {
        _keyPressed = true;
        if (Input.GetKey(KeyCode.W))
        {
            _dir = ObjectDir.Up;
            _state = ObjectState.Moving;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            _dir = ObjectDir.Down;
            _state = ObjectState.Moving;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            _dir = ObjectDir.Left;
            _state = ObjectState.Moving;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            _dir = ObjectDir.Right;
            _state = ObjectState.Moving;
        }
        else _keyPressed = false;
    }
    protected override void UpdateIdle()
    {
        _frontPlayer = GetFrontPlayer();
        if(_frontPlayer != null)
        {
            Interaction.Instance.OnBattleShortCut();
        }
        else
        {
            Interaction.Instance.OffBattleShortCut();
        }

        CheckPressKey();
        if (_keyPressed)
        {
            UpdatePos();
            return;
        }
        CheckUiKey();
    }
    private void UpdatePos()
    {
        Pos destPos = new Pos(_pos);
        switch (_dir)
        {
            case ObjectDir.Up:
                {
                    destPos.Y++;
                    break;
                }
            case ObjectDir.Down:
                {
                    destPos.Y--;
                    break;
                }
            case ObjectDir.Left:
                {
                    destPos.X--;
                    break;
                }
            case ObjectDir.Right:
                {
                    destPos.X++;
                    break;
                }
            default: break;
        }
        if(MapManager.Instance.CanGo(destPos) == false)
            return;
        _pos = destPos;

        C_Move packet = new C_Move();
        packet.Pos = _pos;
        NetworkManager.Instance.Send(packet);
    }
    protected override void ChangedDir()
    {
        C_Move packet = new C_Move();
        packet.Pos = _pos;
        NetworkManager.Instance.Send(packet);
    }
    protected override void ChangedState()
    {
        C_Move packet = new C_Move();
        packet.Pos = _pos;
        NetworkManager.Instance.Send(packet);
    }
    protected override void UpdateMoving()
    {
        base.UpdateMoving();
    }
    protected override void OnMovingCompleted()
    {
        CheckPressKey();
        if (!_keyPressed)
        {
            _state = ObjectState.Idle;
        }
        else
        {
            UpdatePos();
        }
    }
    
}
