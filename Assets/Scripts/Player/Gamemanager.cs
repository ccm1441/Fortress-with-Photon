/**
 * 
 * 게임 승패를 관리함
 *
 **/
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class Gamemanager : MonoBehaviourPunCallbacks
{
    // 동기화 컴포넌트
    public PhotonView PV;
    public NetworkManager networkManager;

    // 플레이어 사망 여부
    public static bool is_dead = false;
    public GameObject Player;
    
    // 탱크 움직일때 이팩트
    public GameObject moveSmoke;

    // 승패 UI
    public GameObject Lose;
    public GameObject Win;
    bool lose = false;

    // 전체 사운드 토글
    public Toggle soundToggle;

    // 어떤 클라이언트가 승리했는지 구분하기 위함
    public int win = 0;


    void Update()
    {
        // 방에 있고 안죽었으면 플레이어를 찾음 > 승리를 의미함으로 승리함수 호출
        if (PhotonNetwork.InRoom && !is_dead && !lose)
        {
            Player = GameObject.Find("Player(Clone)");

            if(win == 1) StartCoroutine("disconnect2");
        } // 죽었으면 패배 함수 호출 및 결과창을 모든 유저에게 출력하라 명령
        else if (is_dead)
        {
            is_dead = false;
            lose = true;
            PV.RPC("result", RpcTarget.Others, 1);
            Death();
        }

        // 사망후 오브젝트 껍데기 삭제
        if (GameObject.Find("Player(Clone)"))
            if (GameObject.Find("Player(Clone)").GetComponent<Transform>().childCount == 0)
                Destroy(GameObject.Find("Player(Clone)"));

        // 사운드 컨드롤
        if (soundToggle.isOn) AudioListener.volume = 0;
        else AudioListener.volume = 1;
    }

    // 사망 후 이팩트 출력 및 패배 함수 호출
    void Death()
    {
        PhotonNetwork.Instantiate("die_effect", Player.transform.position, Quaternion.identity);       
        StartCoroutine("disconnect");
    }

    // 승리한 클라이언트를 구분 // 1 == 승리
    [PunRPC]
    void result(int a)
    {
        win = a;
    }

    // 패배 함수
    IEnumerator disconnect()
    {
        Lose.SetActive(true);
        yield return new WaitForSeconds(5f);
        Lose.SetActive(false);
        init();
    }

    // 승리 함수
    IEnumerator disconnect2()
    {
        Win.SetActive(true);
        yield return new WaitForSeconds(5f);
        Win.SetActive(false);
        init();
    }

    // 게임 승패를 판별한 후 다시 변수 원위치
    void init()
    {
        win = 0;
        lose = false;
        networkManager.LeaveRoom();
    }
}
