/**
 * 
 * 오른쪽 레이저를 담당
 *
 **/
using UnityEngine;
using Photon.Pun;

public class BattleLaserR : MonoBehaviour
{
    // 레드존
    public Transform Redzone;
    // 레이저 속도
    public static int Speed = 3;
    // 레이저 시작 여부
    public static bool start = false;
    // 현재 좌표
    public static float RightX = -10;

    // 프레임마다
    void Update()
    {
        // 오른쪽에서 > 왼쪽으로 움직이며 현재 좌표를 계속 업데이트 한다.
        transform.Translate(Vector3.down * Speed * Time.deltaTime);
        Redzone.position = transform.position - new Vector3(-2, 0, 0);
        Redzone.localScale += new Vector3(Time.deltaTime * Speed, 0, 0);

        if (Redzone.position.x < 109)
            RightX = Redzone.position.x;
    }

    // 플레이어가 레이저에 부딪히면 실행
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
            other.GetComponent<PlayerControl>().get_hit(1);
    }

    // 삭제 후 동기화
    [PunRPC]
    void DestoryRPC() => Destroy(gameObject);

}
