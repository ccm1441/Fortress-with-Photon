/**
 * 
 * 플레이어를 총괄
 *
 **/
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerControl : MonoBehaviourPunCallbacks, IPunObservable
{
    // 네트워크 매니져 참고
    NetworkManager networkManager;

    [Header("Player")]
    // 동기화 컴포넌트
    public PhotonView PV;
    // 플레이어 스프라이트
    public SpriteRenderer SR;
    public SpriteRenderer SR2;
    // 플레이어 점프 관련
    public float Jump_Force = 20;
    public float Jump_delay = 1.5f;
    public float Jump_Timer = 0;
    // 플레이어 체력
    public Image hp;
    // 플레이어 반전
    bool flip = false;
    // 플레이어 물리 컴포넌트
    Rigidbody2D rigidbody;
    // 플레이어가 땅에 있는지 여부
    bool isGround;

    [Header("Bullet")]
    // 총알
    public GameObject Rocket;
    // 포구
    public Transform shotPos;
    // 공격 쿨타임
    bool attackCool = false;
    // 더블 샷 아이템 섭취시
    public bool doubleBullet = false;
    // 더블 샷 지속시간
    float timerdouble = 0;

    [Header("ETC")]
    // 최소 높이
    Transform min_y;
    // 아이템 사운드
    public AudioSource sound;

    [Header("Control")]
    // 마우스 포지션
    Vector3 Mouse;
    Vector3 Mouse_position;
    Vector3 Attack_postion;
    Vector3 curPos;
    float distance = 100;

    private void Start()
    {
        // 시작시 각종 컴포넌트를 가져오고 체력을 초기화
        rigidbody = gameObject.GetComponent<Rigidbody2D>();
        networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
        min_y = GameObject.Find("Y_Min").GetComponent<Transform>();
        hp.fillAmount = 1f;
    }

    void Update()
    {
        // 해당 클라이언트의 캐릭터가 나 자신이면
        if (PV.IsMine)
        {
            // 어느 방향으로 움직이는지 받아옴
            float axis = Input.GetAxisRaw("Horizontal");

            // 움직임, 점프, 캐릭터 좌우 반전, 총알쏘기
            Move(axis);
            jump();
            turning();
            shoot();

            // 점프 타이머
            Jump_Timer += Time.deltaTime * 1;
            
            // 더블샷 스킬이 발동할 시 5초동안 사용
            if (doubleBullet)
            {
                if(timerdouble > 5f)
                {
                    doubleBullet = false;
                    timerdouble = 0;
                }
                timerdouble += Time.deltaTime;
            }

            // 정해놓은 y축보다 아래로 갈시 파괴
            if (transform.position.y < min_y.position.y)
                if (networkManager.starting)
                    get_hit(1);
                else transform.position = Vector2.zero;

            // 그 방에 있는 모든 타켓에게 RPC 함수 실행, 버퍼에 남김으로써 재접속 했을 시 기존 데이터를 이어 받음     
            if (axis != 0) PV.RPC("FlipXRPC", RpcTarget.AllBuffered, flip);
        }
        else if ((transform.position - curPos).sqrMagnitude >= 100) transform.position = curPos; // 데이터가 너무 끊어지면 받아온 최근 좌표로 이동
        else transform.position = Vector3.Lerp(transform.position, curPos, Time.deltaTime * 10); // 적당하면 부드럽게 좌표 동기화
    }

   // 그 방에있는 모두가 좌우 반전을 동기화 함
    [PunRPC]
    void FlipXRPC(bool pos)
    {
        SR.flipX = pos;
        SR2.flipX = pos;
    }

    // 캐릭터 움직임
    private void Move(float axis)
    {
        if (networkManager.PlayerMove)
            transform.Translate(new Vector3(axis * Time.deltaTime * 15, 0, 0));

    }

    // 캐릭터 좌우 반전 >> 마우스의 위치에 따라 반전 시킴
    private void turning()
    {
        Mouse = Input.mousePosition;
        Mouse_position = Camera.main.ScreenToWorldPoint(Mouse);

        if (transform.position.x > Mouse_position.x) flip = true;
        else flip = false;
    }

    // 캐릭터 점프 >> 땅에 있을때만 점프 가능
    private void jump()
    {
        isGround = Physics2D.OverlapCircle((Vector2)transform.position + new Vector2(0, -1.5f), 0.07f, 1 << LayerMask.NameToLayer("Ground"));

        if (Input.GetKeyDown(KeyCode.Space) && isGround)
        {
            PV.RPC("JumpRPC", RpcTarget.All);
        }
    }

    // 그 방에있는 모두가 점프를 동기화 함
    [PunRPC]
    void JumpRPC()
    {
        rigidbody.velocity = Vector2.zero;
        rigidbody.AddForce(Vector2.up * 700);
    }

    // 총알 쏘기 >> 왼쪽 마우스를 누르면 총알을 쏘고 그방 모두에게 총알 발사를 동기화
    private void shoot()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && !attackCool)
        {
            attackCool = true;
            Mouse_position = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            PhotonNetwork.Instantiate("Rocket", SR2.transform.position + new Vector3(!flip ? 2.5f : -2.5f, 1f, 0), Quaternion.identity)
              .GetComponent<PhotonView>().RPC("ShotRPC", RpcTarget.All, Mouse_position);
            
            StartCoroutine("cool");
        }
    }

    // 총아을 쏜뒤 잠시 쿨타임을 가지도록 설정, 더블샷이면 한발 더 동기화
    IEnumerator cool()
    {
        yield return new WaitForSeconds(0.2f);

        Mouse_position = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        if (doubleBullet)
            PhotonNetwork.Instantiate("Rocket", SR2.transform.position + new Vector3(!flip ? 2.5f : -2.5f, 1f, 0), Quaternion.identity)
         .GetComponent<PhotonView>().RPC("ShotRPC", RpcTarget.All, Mouse_position);

        yield return new WaitForSeconds(0.5f);
        
        attackCool = false;
    }

    // 이방에 모두에게 데이터파괴 동기화
    [PunRPC]
    void DestoryRPC() => Destroy(gameObject);

    // 플레이어에게 데미지를 처리, 총알은 모든 타켓으로 삭제
    public void get_hit(float Damege) 
    {
        hp.fillAmount -= Damege;
        if (hp.fillAmount <= 0)
        {
            transform.parent.GetComponent<Transform>().position = transform.position;
            Gamemanager.is_dead = true;
            PV.RPC("DestoryRPC", RpcTarget.AllBuffered);
        }
    }

    // 아이템을 먹으면 사운드 호출
    public void itemsound() => sound.Play();

    // 실시간으로 체력과 위치를 동기화 하고 받음
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(hp.fillAmount);
        }
        else
        {
            curPos = (Vector3)stream.ReceiveNext();
            hp.fillAmount = (float)stream.ReceiveNext();
        }
    }
}
