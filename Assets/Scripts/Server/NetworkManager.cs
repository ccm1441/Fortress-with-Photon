/**
 * 
 * 네트워크를 총괄하고 있음
 *
 **/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    
    [Header("Title")]
    // 타이틀 UI
    public GameObject TitlePanel;
    // 게임 시작버튼
    public Button ConnectBtn;

    [Header("Lobby")]
    // 로비 UI
    public GameObject LobbyPanel;
    // 로비 상태를 알려주는 텍스트
    public Text LobbyText;
    // 방 옵션 설정
    public GameObject RoomOption;
    // 방이름
    public Text RoomName;
    public InputField RoomName2;
    // 방 아이템 여부
    public Toggle RoomItem;
    // 방 최대 플레이어
    public GameObject maxPlayer;
    // 방 레이저 속도
    public Toggle[] FireSpeed = new Toggle[3];
    // 방 리스트
    public List<GameObject> RoomList = new List<GameObject>();
    // 게임 시작
    public Button ingame;
    // 방 아이템 여부
    string roomsub1;
    // 방 옵션
    string roomsub2;

    [Header("InGame")]
    // 인게임 UI
    public GameObject InGamePanel;
    // 플레이어 소환 위치
    public GameObject Left, Right;
    // 방 상태
    public Text RoomState;
    // 나가기 버튼
    public Button exit;
    // 레이져
    public GameObject RedZone;
    public Transform RedZonePosL;
    public Transform RedZonePosR;
    // 게임시작 브금
    public AudioSource startsound;

    [Header("ETC")]
    // 서버 연결 상태
    public Text StatusText;
    // 메인카메라
    public GameObject MainCamera;
    // 플레이어 움직일수 있나 여부
    public bool PlayerMove = true;
    // 초기화 여부
    bool init = false;
    // 게임 시작 여부
    public bool starting = false;

    // 방 리스트를 네트워크에서 받아옴
    List<RoomInfo> myList = new List<RoomInfo>();

    #region 서버연결

    // 스크립트 시작 전 화면크기 조절 후 서버 연결 시도
    void Awake()
    {
        Screen.SetResolution(960, 540, false);
        ConnectBtn.interactable = false;
        StatusText.text = "상태 : 서버 연결 시도 중...";
        PhotonNetwork.ConnectUsingSettings();
        starting = false;
    }

    // 서버가 연결 되면 게임 시작버튼 활성화 및 로비에 입장
    public override void OnConnectedToMaster()
    {
        ConnectBtn.interactable = true;

        StatusText.text = "상태 : 서버 연결 완료";

        if (!TitlePanel.activeSelf)
            PhotonNetwork.JoinLobby();
    }

    // 게임 시작을 누르면 로비로 입장
    public void Connect() => PhotonNetwork.JoinLobby();

    private void Update()
    {
        // 서버에 방리스트를 확인후 초기화
        if (myList.Count == 0)
            for (int i = 0; i < RoomList.Count; i++)
                RoomList[i].SetActive(false);

        // 플레이어가 2명이 와야 게임을 시작
        if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.PlayerCount == 2 && !init)
        {
            itemCreator.itemon = false;
            RoomState.text = "잠시후 경기가 시작됩니다!";
            StartCoroutine("StartCount");
        }
        else if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            init = false;
            exit.interactable = true;
            starting = false;
            StartCoroutine("DestoryBullet");
            RoomState.text = "상대방 접속 대기중..";
        }

        // 게임중에는 나갈수가 없도록 버튼 비활성화
        if (starting) exit.interactable = false;
        else exit.interactable = true;
    }

    // 서버에 연결 끊기
    public void Disconnect()
    {
        ConnectBtn.interactable = false;
        PhotonNetwork.ConnectUsingSettings();
    }
    #endregion

    #region 로비

    // 로비에 접속후 해당 UI 설정
    public override void OnJoinedLobby()
    {
        InGamePanel.SetActive(false);
        TitlePanel.SetActive(false);
        LobbyPanel.SetActive(true);
        LobbyText.text = "상태 : 온라인";
        myList.Clear();
    }

    // 방 만들기를 누르면 방 옵션 UI 나타넴
    public void CreateRoomVisible()
    {
        RoomName.text = "";
        RoomName2.text = "";
        RoomOption.SetActive(true);
        ingame.interactable = true;
    }

    // 방이 만들어지거나 없어지면 서버에서 방 리스트를 받아와 업데이트
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        int roomCount = roomList.Count;
        for (int i = 0; i < roomCount; i++)
        {
            if (!roomList[i].RemovedFromList)
            {
                // 방 목록에 방이 없으면 추가
                if (!myList.Contains(roomList[i]))
                {
                    myList.Add(roomList[i]);

                    int index = myList.Count - 1;

                    RoomList[index].SetActive(true);
                    RoomList[index].transform.GetChild(1).GetComponent<Text>().text = myList[index].Name;
                    RoomList[index].transform.GetChild(2).GetComponent<Button>().transform.GetChild(0).GetComponent<Text>().text = "참가\n" + myList[index].PlayerCount + " / " + myList[index].MaxPlayers;
                }
                else // 목록에 있으면 방 업데이트
                {
                    myList[myList.IndexOf(roomList[i])] = roomList[i];
                    RoomList[myList.IndexOf(roomList[i])].transform.GetChild(2).GetComponent<Button>().transform.GetChild(0).GetComponent<Text>().text = "참가\n" + myList[myList.IndexOf(roomList[i])].PlayerCount + " / " + myList[myList.IndexOf(roomList[i])].MaxPlayers;
                }
            }
            else if (myList.IndexOf(roomList[i]) != -1)
            {
                if (roomList[i].PlayerCount == 0)
                    RoomList[myList.IndexOf(roomList[i])].SetActive(false);

                myList.RemoveAt(myList.IndexOf(roomList[i]));
            }
        }
    }

    // 만들어진 방을 누르면 방 입장
    public void RoomClick(int num)
    {
        InGamePanel.SetActive(true);
        LobbyPanel.SetActive(false);
        PhotonNetwork.JoinRoom(myList[num].Name);
    }

    // 방 만들기를 실패하면 다시 방만들기 시도
    public override void OnCreateRoomFailed(short returnCode, string message) { OnCreatedRoom(); }

    // 방 입장을 실패하면 로비로 입장 >> 인원 초과로 실패하면 인원초과UI 출력
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        InGamePanel.SetActive(false);
        LobbyPanel.SetActive(true);
        maxPlayer.SetActive(true);
    }

    // 인원초과 UI 확인
    public void JoinRoomFailedOK() => maxPlayer.SetActive(false);
    #endregion
    
    #region 방 만들기

    // 방 만드는 옵션창 나가기
    public void CreateRoomExit() => RoomOption.SetActive(false);

    // 방 만드는 함수 >> 방 이름과 아이템 여부, 레이져 속도를 다른 함수로 넘긴 뒤 방을 만듬
    public override void OnCreatedRoom()
    {
        ingame.interactable = false;
        RoomOption.SetActive(false);

        roomsub1 = RoomItem.isOn ? "아이템 : Yes" : "아이템 : No";
        if (RoomItem.isOn) itemCreator.itemon = true;
        else itemCreator.itemon = false;

        for (int j = 0; j < FireSpeed.Length; j++)
        {
            if (FireSpeed[j].isOn)
            {
                roomsub2 = " , 불 속도 : " + FireSpeed[j].transform.GetChild(1).GetComponent<Text>().text;
                if (j == 0)
                {
                    BattleLaser.Speed = 8;
                    BattleLaserR.Speed = 8;
                }

                else if (j == 1)
                {
                    BattleLaser.Speed = 5;
                    BattleLaserR.Speed = 5;
                }
                else if (j == 2)
                {
                    BattleLaser.Speed = 3;
                    BattleLaserR.Speed = 3;
                }
                break;
            }
        }

        RoomName.text = RoomName.text == "" ? "방" + Random.Range(1, 101) + "\n" : RoomName.text + "\n";
        RoomName.text += roomsub1 + roomsub2;

        PhotonNetwork.CreateRoom(RoomName.text, new RoomOptions { MaxPlayers = 2 });
        

        LobbyPanel.SetActive(false);
        InGamePanel.SetActive(true);
    }
    #endregion

    #region 방(인게임)

    // 방에 들어오면 들어온 순서에따라 플레이어 스폰
    public override void OnJoinedRoom()
    {
        StartCoroutine("DestoryBullet");
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.Instantiate("Player", Left.transform.position, Quaternion.identity);
        else
            PhotonNetwork.Instantiate("Player", Right.transform.position, Quaternion.identity);
    }

    // 플레이어가 모두 접속시 게임을 시작함
    IEnumerator StartCount()
    {
        yield return new WaitForSeconds(2f);

        init = true;
        PlayerMove = false;
        starting = true;

        foreach (GameObject GO in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (GO.GetComponent<PhotonView>().IsMine)
                if (PhotonNetwork.IsMasterClient)
                    GO.transform.position = Left.transform.position;
                else GO.transform.position = Right.transform.position;
        }

        startsound.Play();
        yield return new WaitForSeconds(2f);        
        RoomState.text = "준비~~~~~~~~~~~~~~~~~~~";        
        yield return new WaitForSeconds(2f);
        RoomState.text = "경기 시작!";
        itemCreator.itemon = true;
        PlayerMove = true;
        yield return new WaitForSeconds(3f);
        RoomState.text = "잠시후 <color=red>RedZone</color>이 활성화 됩니다! ";
        yield return new WaitForSeconds(3f);
        RoomState.text = "<color=red>RedZone</color>이 활성화 되었습니다!\n줄어드는 <color=red>RedZone</color>을 피해 싸우세요!";
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Instantiate("laser", RedZonePosL.position, RedZone.transform.rotation);
            PhotonNetwork.Instantiate("laserR", RedZonePosR.position, RedZone.transform.rotation);
        }
    }

    // 나가기 버튼을 누르면 방을 나감
    public void LeaveRoom()
    {
        starting = false;
        init = false;
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.JoinLobby();
        MainCamera.transform.position = new Vector3(0, 0, MainCamera.transform.position.z);
    }

    // 방입장후 서버에 동기화로 인해 남은 찌꺼기를 모두 삭제
    IEnumerator DestoryBullet()
    {
        yield return new WaitForSeconds(0.2f);
        foreach (GameObject GO in GameObject.FindGameObjectsWithTag("Bullet"))
            GO.GetComponent<PhotonView>().RPC("DestoryRPC", RpcTarget.AllBuffered);
        foreach (GameObject GO in GameObject.FindGameObjectsWithTag("item"))
            GO.GetComponent<PhotonView>().RPC("DestoryRPC", RpcTarget.AllBuffered);

        if (GameObject.Find("laser(Clone)") && GameObject.Find("laserR(Clone)"))
        {
            GameObject.Find("laser(Clone)").GetComponent<PhotonView>().RPC("DestoryRPC", RpcTarget.AllBuffered);
            GameObject.Find("laserR(Clone)").GetComponent<PhotonView>().RPC("DestoryRPC", RpcTarget.AllBuffered);
        }
    }
    #endregion
}
