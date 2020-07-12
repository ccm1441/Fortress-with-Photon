/**
 * 
 * 총알이 날라가는 방향과 좌표를 실시간으로 보내고 받으며 충돌 여부를 동작
 *
 **/
using UnityEngine;
using Photon.Pun;

public class RocketController : MonoBehaviourPun, IPunObservable
{
    // 동기화 컴포넌트
    public PhotonView PV;

    // 최대 최소 높이 및 너비
    Transform x_max;
    Transform x_min;
    Transform y_max;
    Transform y_min;

    // 플레이어 스프라이트
    SpriteRenderer Player;
    
    // 마우스 커서 위치
    Vector3 curPos;

    // 총알 사운드
    public AudioSource sound;
    
    // 스크립트 시작시 각 변수에 컴포넌트를 불러오며 총알이 발사하는 소리 재생
    void Start()
    {
        Player = GameObject.Find("Player").GetComponent<SpriteRenderer>();
        x_max = GameObject.Find("X_Max").GetComponent<Transform>();
        x_min = GameObject.Find("X_Min").GetComponent<Transform>();
        y_max = GameObject.Find("Y_Max").GetComponent<Transform>();
        y_min = GameObject.Find("Y_Min").GetComponent<Transform>();

        sound.Play();
    }

    // 같은 방에있는 유저들에게 뿌려지는 함수
    // 총알을 날리는것을 동기화
    [PunRPC]
    void ShotRPC(Vector3 pos) => GetComponent<Rigidbody2D>().AddForce(pos * 15, ForceMode2D.Impulse);

    // 매 프레임마다 업데이트
    private void FixedUpdate()
    {
        // 최대 최소 너비 및 높이를 넘어가면 삭제
        if (transform.position.x > x_max.position.x || transform.position.y > y_max.position.y
          || transform.position.x < x_min.position.x || transform.position.y < y_min.position.y)
            PV.RPC("DestoryRPC", RpcTarget.AllBuffered);

        // 본인 클라이언트가 아닐 떄 동기화가 느릴시 딜레이를 줄이기 위함
        if (!PV.IsMine)
        {
            if ((transform.position - curPos).sqrMagnitude >= 100) transform.position = curPos;
            else transform.position = Vector3.Lerp(transform.position, curPos, Time.deltaTime * 16);
        }
    }

    // 같은 방에있는 유저들에게 총알을 지우라 명령
    [PunRPC]
    void DestoryRPC() => Destroy(gameObject);

    // 본인 클라이언트가 아니고 총알이 플에이어한테 부딪히면 체력을 깍고 삭제 함수 동작
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(!PV.IsMine && other.tag =="Player" && other.GetComponent<PhotonView>().IsMine)
        {
            other.GetComponent<PlayerControl>().get_hit(0.1f);
            PV.RPC("DestoryRPC", RpcTarget.AllBuffered);
        }

        if(other.tag == "Ground")
            PV.RPC("DestoryRPC", RpcTarget.AllBuffered);
    }

    // 실시간으로 총알의 좌표를 보내고 받음
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
            stream.SendNext(transform.position);
        else
            curPos = (Vector3)stream.ReceiveNext();
    }
}
