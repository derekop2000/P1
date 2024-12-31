using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Assets.Scripts.Contents;
using Assets.Scripts.Contents.Object;

using Google.Protobuf;

using JetBrains.Annotations;

using Unity.VisualScripting;

using UnityEngine;
using UnityEngine.UI;

public class Interaction : MonoBehaviour
{
    private static Interaction interaction = null;
    private Coroutine _timerCor = null;
    public static Interaction Instance
    {
        get
        {
            return interaction;
        }
    }
    public GameObject _battleButton;

    public GameObject _monStore;
    public GameObject _ItemStore;

    public Text _battleResult;
    public Text _timeText;
    public int _time;

    public Text _loginNotification;
    public Text _input1;
    public Text _input2;
    public GameObject _startScreen;
    public GameObject _battleScreen;

    public GameObject _myMon;
    public GameObject _myMonHp;

    public GameObject _enemy;
    public GameObject _enemyHp;

    public Sprite _mon1;
    public Sprite _mon2;
    public Sprite _mon3;

    public GameObject _currentMon;

    public Text[] _chatLogs;
    public InputField _chatBox;
    int _chatLogSize;

    public GameObject _notificationWindow;
    Coroutine _notificationWindowCor;

    public int _opponentId;
    /// ///////////////////////////////////////
    UIflag _UIflag = UIflag.empty;
    enum UIflag
    {
        empty = 0b00000000000000000000000000000000,
        MonStore = 0b00000000000000000000000000000010,
        ItemStore = 0b00000000000000000000000000000001,
        ChatBox = 0b00000000000000000000000000000100,
    }
    public bool IsOnMonStore()
    {
        return (_UIflag & UIflag.MonStore) != 0;
    }
    public bool IsOnItemStore()
    {
        return (_UIflag & UIflag.ItemStore) != 0;
    }
    public bool IsOnChatBox()
    {
        return (_UIflag & UIflag.ChatBox) != 0;
    }
    public void OnOffChatBox()
    {
        if (IsOnChatBox())
            OffChatBox();
        else OnChatBox();
    }
    public void OnChatBox()
    {
        if (_UIflag != UIflag.empty)
            return;
        _UIflag |= UIflag.ChatBox;
        _chatBox.gameObject.SetActive(true);
        _chatBox.ActivateInputField();
        ObjectManager.Instance._myPlayer._state = ObjectState.Manual;
    }
    public void OffChatBox(bool flush = true)
    {
        _UIflag &= ~UIflag.ChatBox;

        if(flush && _chatBox.text != string.Empty)
        {
            C_Chat packet = new C_Chat();
            packet.Str = _chatBox.text;
            NetworkManager.Instance.Send(packet);
        }
        _chatBox.text = string.Empty;
        _chatBox.gameObject.SetActive(false);
        ObjectManager.Instance._myPlayer._state = ObjectState.Idle;
    }
    public void OnOffMonStore()
    {
        if(IsOnMonStore())
            OffMonStore();
        else OnMonStore();

    }
    public void OnMonStore()
    {
        if (IsOnChatBox())
            return;
        _UIflag |= UIflag.MonStore;
        _monStore.SetActive(true);

        int index = 1;
        foreach(MonsterCp cp in ObjectManager.Instance._myPlayer._mons.Values)
        {
            GameObject store = _monStore.transform.Find($"Mon{index}").gameObject;
            Text lv = store.transform.Find("LV").gameObject.GetComponent<Text>();
            lv.text = $"LV\n{cp._cp.Level}";
            Image image = store.transform.Find("Image").gameObject.GetComponent<Image>();
            switch (cp._cp.MonNum)
            {
                case 1:
                    {
                        image.sprite = _mon1;
                        break;
                    }
                case 2:
                    {
                        image.sprite = _mon2;
                        break;
                    }
                case 3:
                    {
                        image.sprite = _mon3;
                        break;
                    }
                default: break;
            }
            Slider slider = store.transform.Find("hp").gameObject.GetComponent<Slider>();
            slider.value = cp._cp.Hp / (float)cp._cp.MaxHp;

            GameObject current = store.transform.Find("Current").gameObject;
            if (cp._objId == ObjectManager.Instance._myPlayer._mainMon._objId)
            {
                _currentMon = current;
                current.SetActive(true);
            }
            else current.SetActive(false);
            index++;
        }
        ObjectManager.Instance._myPlayer._state = ObjectState.Manual;
    }
    public void OffMonStore()
    {
        _UIflag &= ~UIflag.MonStore;
        _monStore.SetActive(false);
        _currentMon.SetActive(false);
        if (_UIflag == UIflag.empty)
            ObjectManager.Instance._myPlayer._state = ObjectState.Idle;
    }
    public void OnOffItemStore()
    {
        if (IsOnItemStore())
            OffItemStore();
        else OnItemStore();
    }

    public void OnItemStore()
    {
        if (IsOnChatBox())
            return;
        _UIflag |= UIflag.ItemStore;
        _ItemStore.SetActive(true);
        _ItemStore.transform.Find("Item1").transform.Find("Cnt").GetComponent<Text>().text = ObjectManager.Instance._myPlayer._hpPotionCnt.ToString("d2");
        ObjectManager.Instance._myPlayer._state = ObjectState.Manual;
    }
    public void OffItemStore()
    {
        _UIflag &= ~UIflag.ItemStore;
        _ItemStore.SetActive(false);

        if (_UIflag == UIflag.empty)
            ObjectManager.Instance._myPlayer._state = ObjectState.Idle;
    }
    public void OffAllStore()
    {
        if (IsOnItemStore())
            OffItemStore();
        if (IsOnMonStore())
            OffMonStore();
        OffChatBox(false);   
    }
    /// ///////////////////////////////////////
    public void OnBattleShortCut()
    {
        _battleButton.SetActive(true);
    }
    public void OffBattleShortCut()
    {
        _battleButton.SetActive(false);
    }
    public void Start()
    {
        if (interaction == null)
            interaction = this;
        _myMon = _battleScreen.transform.Find("MyMon").gameObject;
        _myMonHp = _myMon.transform.Find("hp").gameObject;
        _enemy = _battleScreen.transform.Find("Enemy").gameObject;
        _enemyHp = _enemy.transform.Find("hp").gameObject;
        _battleScreen.SetActive(false);
        _chatLogSize = _chatLogs.Length;
    }
    public void MakeNotificationWindow(string text, int time)
    {
        if(_notificationWindowCor != null)
            StopCoroutine(_notificationWindowCor);

        _notificationWindow.SetActive(true);

        Text notifyText = _notificationWindow.transform.Find("Text").gameObject.GetComponent<Text>();
        notifyText.text = text;

        Text buttonText = _notificationWindow.transform.Find("Button").Find("Text").gameObject.GetComponent<Text>();
        _notificationWindowCor = StartCoroutine(NotificationCoroutine(buttonText, time));
    }

    IEnumerator NotificationCoroutine(Text text, int time)
    {
        for(int i = time;i>=1;i--)
        {
            text.text = $"수락({i}s)";
            yield return new WaitForSeconds(1);
        }
        _notificationWindow.SetActive(false);
        _notificationWindowCor = null;
    }
    public void ResponeBattleRequest()
    {
        if (_notificationWindowCor != null)
        {
            StopCoroutine(_notificationWindowCor);
            _notificationWindow.SetActive(false);
        }

        C_ResponeBattleRequest packet = new C_ResponeBattleRequest();
        packet.P1 = ObjectManager.Instance._myPlayer._objectId;
        packet.P2 = _opponentId;
        NetworkManager.Instance.Send(packet);
    }

    public void OnRecvChat(int id, string str)
    {
        for(int i= _chatLogSize - 1;i>0;i--)
        {
            _chatLogs[i].text = _chatLogs[i - 1].text;
        }
        _chatLogs[0].text = $"{id} : {str}";
    }
    public void ChangeNotify(string text)
    {
        _loginNotification.text = text;
    }
    public void SignUpFunc()
    {
        if(NetworkManager._pending)
        {
            _loginNotification.text = "요청을 처리중...";
            return;
        }

        NetworkManager._pending = true;

        C_SignUp pkt = new C_SignUp();
        pkt.Id = _input1.text;
        pkt.Password = _input2.text;
        NetworkManager.Instance.Send(pkt);
    }
    public void SignUpCompleted(ushort state)
    {
        switch (state)
        {
            case 0:
                {
                    _loginNotification.text = "회원가입 완료";
                    break;
                }
            case 1:
                {
                    _loginNotification.text = "이미 존재하는 아이디입니다";
                    break;
                }
            case 2:
                {
                    _loginNotification.text = "잠시 후 다시 시도해주세요";
                    break;
                }
            default:
                {
                    _loginNotification.text = ".";
                    break;
                }
        }
        _input1.text = "";
        _input2.text = "";
        
    }

    public void SignInFunc()
    {
        if (NetworkManager._pending)
        {
            _loginNotification.text = "요청을 처리중...";
            return;
        }

        NetworkManager._pending = true;

        C_SignIn pkt = new C_SignIn();
        pkt.Id = _input1.text;
        pkt.Password = _input2.text;
        NetworkManager.Instance.Send(pkt);
    }
    public void SignInCompleted(ushort state)
    {
        switch(state)
        {
            case 0:
                {
                    _startScreen.active = false;
                    break;
                }
            case 1:
                {
                    _loginNotification.text = "아이디 또는 비밀번호가 일치하지 않습니다";
                    break;
                }
            default: break;
        }
        _input1.text = "";
        _input2.text = "";
        
    }
    public void OnBattleCompleted(int result = 0)
    {
        string text = "";
        if (result == 1)
            text = "Win";
        else if (result == 2)
            text = "Lose";
        else if (result == 3)
            text = "Draw";
        else if (result == 4)
            text = "Capture!";
        else if (result == 0)
            text = "Opponenet has left!";
        else text = "";

        _battleResult.text = text;
        StartCoroutine(BattleCompletedCoroutine());
    }
    public IEnumerator BattleCompletedCoroutine()
    {
        yield return new WaitForSeconds(3);
        _battleScreen.SetActive(false);
        C_BattleCompleted packet = new C_BattleCompleted();
        NetworkManager.Instance.Send(packet);
    }
    public void OnBattleSet(int myMonNum,int enemyNum)
    {
        _battleScreen.SetActive(true);
        _battleResult.text = "-";
        switch (myMonNum)
        {
            case 1:
                {
                    _myMon.GetComponent<Image>().sprite = _mon1;
                    break;
                }
            case 2:
                {
                    _myMon.GetComponent<Image>().sprite = _mon2;
                    break;
                }
            case 3:
                {
                    _myMon.GetComponent<Image>().sprite = _mon3;
                    break;
                }
            default : break;
        }

        switch (enemyNum)
        {
            case 1:
                {
                    _enemy.GetComponent<Image>().sprite = _mon1;
                    break;
                }
            case 2:
                {
                    _enemy.GetComponent<Image>().sprite = _mon2;
                    break;
                }
            case 3:
                {
                    _enemy.GetComponent<Image>().sprite = _mon3;
                    break;
                }
            default: break;
        }

    }
    public void SetHp(int friendlyCurrent, int friendlyMax, int enemyCurrent, int enemyMax)
    {
        _myMonHp.GetComponent<Slider>().value = friendlyCurrent / (float)friendlyMax;
        _enemyHp.GetComponent<Slider>().value = enemyCurrent / (float)enemyMax;
    }
    public void SetTimer(int time)
    {
        _time = time;
        if(_timerCor != null)
        {
            StopCoroutine(_timerCor);
        }
        _timerCor = StartCoroutine(TimeCoroutine());
    }
    IEnumerator TimeCoroutine()
    {
        while(_time > 0)
        {
            _timeText.text = $"Time - {_time}";
            yield return new WaitForSeconds(1f);
            _time--;
        }
        _timeText.text = $"Time Up!";
        _timerCor = null;
    }
}
