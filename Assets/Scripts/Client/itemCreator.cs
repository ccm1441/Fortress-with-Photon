/**
 * 
 * 아이템을 생성 -> 레이저 뒤에 소환하지 않도록 구현
 *
 **/
using UnityEngine;
using Photon.Pun;

public class itemCreator : MonoBehaviourPunCallbacks
{
    // 최소 최대 너비 및 높이
    Transform x_max;
    Transform x_min;
    Transform y_max;
    Transform y_min;

    // 타임
    float timer;

    // 아이템 생성 주기
    public float item_deley = 3f;

    // 방 생성시 아이템전 체크 여부
    public static bool itemon = true;

    // 시작시 방이 아니면 실행 안함
    // 각 변수에 컴포넌트를 불러오고 타이머를 0으로 초기화
    void Start()
    {
        if (!PhotonNetwork.InRoom)
            return;

        x_max = GameObject.Find("X_Max").GetComponent<Transform>();
        x_min = GameObject.Find("X_Min").GetComponent<Transform>();
        y_max = GameObject.Find("Y_Max").GetComponent<Transform>();
        y_min = GameObject.Find("Y_Min").GetComponent<Transform>();
        timer = 0f;
    }

    // 프레임마다 아이템을 생성
    void Update()
    {
        if (itemon)
        {
            if (!PhotonNetwork.InRoom || !PhotonNetwork.IsMasterClient)
                return;

            // 타임에 시간을 계속 더해줌
            timer += Time.deltaTime * 1;

            // 지정한 주기에 도달하면 아이템 생성 및 타임 초기화
            if (timer > item_deley)
            {
                RandomLeft();
                RandomRight();
                timer = 0;
            }
        }
    }

    // 왼쪽에 아이템을 생성 함 > 레이저가 지나간 자리는 생성 x
    void RandomLeft()
    {
        float x = Random.Range(-10, BattleLaser.LeftX);
        int item = Random.Range(0, 2);

        if(item == 0)
        PhotonNetwork.Instantiate("item_heal", new Vector3(x,10,0), Quaternion.identity);
        else PhotonNetwork.Instantiate("item_atk", new Vector3(x, 10, 0), Quaternion.identity);
    }

    // 오른쪽에 아이템을 생성 함 > 레이저가 지나간 자리는 생성 x
    void RandomRight()
    {
        float x = Random.Range(10,BattleLaserR.RightX);
        int item = Random.Range(0, 2);

        if (item == 0)
            PhotonNetwork.Instantiate("item_heal", new Vector3(x, 10, 0), Quaternion.identity);
        else PhotonNetwork.Instantiate("item_atk", new Vector3(x, 10, 0), Quaternion.identity);
    }
}
