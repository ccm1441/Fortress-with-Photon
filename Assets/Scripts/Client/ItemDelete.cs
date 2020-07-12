/**
 * 
 * 아이템의 능력을 관리하며 삭제를 담당
 *
 **/
using UnityEngine;
using Photon.Pun;

public class ItemDelete : MonoBehaviourPunCallbacks
{
    // 동기화 변수
    public PhotonView PV;

    // 삭제 함수 >> 동기화 함으로써 양 클라이언트에서 삭제
    [PunRPC]
    void DestoryRPC() => Destroy(gameObject);

    // 플레이어가 아이템에 부딪히면 실행
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            other.GetComponent<PlayerControl>().itemsound();

            if (gameObject.name == "item_heal(Clone)")
                other.GetComponent<PlayerControl>().get_hit(-0.3f);
            else if (gameObject.name == "item_atk(Clone)")
                other.GetComponent<PlayerControl>().doubleBullet = true;

            PV.RPC("DestoryRPC", RpcTarget.AllBuffered);
        }

        if(other.tag =="Laser")
            PV.RPC("DestoryRPC", RpcTarget.AllBuffered);
    }
}
