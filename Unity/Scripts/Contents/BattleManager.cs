using System.Collections;
using System.Collections.Generic;

using Assets.Scripts.Contents;
using Assets.Scripts.Contents.Object;

using Google.Protobuf;

using JetBrains.Annotations;

using UnityEngine;

public class BattleManager : MonoBehaviour
{
    MonsterCp _my;
    MonsterCp _enemy;
    
    private static BattleManager battleManager = null;
    public static BattleManager Instance
    {
        get
        {
            return battleManager;
        }
    }
    private void Start()
    {
        if (battleManager == null)
            battleManager = this;

    }
    public void RequestBattle()
    {
        PlayerObject p = ObjectManager.Instance._myPlayer._frontPlayer;
        if (p == null)
            return;
        C_RequestBattle packet = new C_RequestBattle();
        packet.Opponent = p._objectId;
        NetworkManager.Instance.Send(packet);
    }
    public void SetBattle(MonsterCp my, MonsterCp enemy)
    {
        _my = my;
        _enemy = enemy;
        ObjectManager.Instance._myPlayer._state = ObjectState.Fight;
        Interaction.Instance.OffAllStore();
        Interaction.Instance.OnBattleSet(my._cp.MonNum, enemy._cp.MonNum);
    }
    public void OnBattleCompleted(int result=0)
    {
        _my = null;
        _enemy = null;
        ObjectManager.Instance._myPlayer._state = ObjectState.Idle;
        Interaction.Instance.SetTimer(3);
        Interaction.Instance.OnBattleCompleted(result);
    }
    public void BattleSelect(int type)
    {
        if (NetworkManager._pending)
            return;
        NetworkManager._pending = true;

        AttackType attackType = AttackType.Nothing;
        switch (type)
        {
            case 1:
                {
                    attackType = AttackType.Normalattack;
                    break;
                }
            case 2:
                {
                    attackType = AttackType.Skillattack;
                    break;
                }
            case 3:
                {
                    attackType = AttackType.Capture;
                    break;
                }
        }

        C_AttackType packet = new C_AttackType();
        packet.AttackType = attackType;
        NetworkManager.Instance.Send(packet);
    }
    public void OnBattle(S_AttackResult packet)
    {
        int result = packet.Result;
        switch (result)
        {
            case 0:
                {
                    Interaction.Instance.SetTimer(packet.TurnTime);
                    _my._cp = packet.P;
                    _enemy._cp = packet.Enemy;
                    Interaction.Instance.SetHp(_my._cp.Hp, _my._cp.MaxHp, _enemy._cp.Hp, _enemy._cp.MaxHp);
                    break;
                }
            case 1:
                {
                    _my._cp = packet.P;
                    Interaction.Instance.SetHp(_my._cp.Hp, _my._cp.MaxHp, 0, _enemy._cp.MaxHp);
                    OnBattleCompleted(result);
                    break;
                }
            case 2:
                {
                    _my._cp = packet.P;
                    _enemy._cp = packet.Enemy;
                    Interaction.Instance.SetHp(_my._cp.Hp, _my._cp.MaxHp, _enemy._cp.Hp, _enemy._cp.MaxHp);
                    OnBattleCompleted(result);
                    break;
                }
            case 3:
                {
                    Interaction.Instance.SetHp(0, _my._cp.MaxHp, 0, _enemy._cp.MaxHp);
                    OnBattleCompleted(result);
                    break;
                }
            case 4:
                {
                    OnBattleCompleted(result);
                    break;
                }
            default: break;

        }
    }
}
