/**
 * 
 * 각각의 유저를 비추는 카메라, 유저를 따라감
 *
 **/
using UnityEngine;
using Photon.Pun;


public class FollowCamera : MonoBehaviourPunCallbacks
{
    // 카메라가 유저를 따라가는 속도
    public float smoothTimeX, smoothTimeY;
    // 속도
    public Vector2 velocity;
    // 플레이어
    public GameObject player;
    // 최대 최소 좌표
    public Vector2 minPos, maxPos;
    // 바로 이동
    public bool bound;


    void FixedUpdate()
    {
        // 방에 들어왔을 때만 유저를  찾아감
        if (PhotonNetwork.InRoom && !Gamemanager.is_dead)
        {
           if(GameObject.Find("Player")) player = GameObject.Find("Player");
           else player = GameObject.Find("Player(Clone)");

            float posX = Mathf.SmoothDamp(transform.position.x, player.transform.position.x, ref velocity.x, smoothTimeX);

            // Mathf.SmoothDamp는 천천히 값을 증가시키는 메서드이다.

            float posY = Mathf.SmoothDamp(transform.position.y, player.transform.position.y, ref velocity.y, smoothTimeY);

            // 카메로 이동

            transform.position = new Vector3(posX, posY, transform.position.z);


            if (bound)
            {
                //Mathf.Clamp(현재값, 최대값, 최소값);  현재값이 최대값까지만 반환해주고 최소값보다 작으면 그 최소값까지만 반환합니다.

                transform.position = new Vector3(Mathf.Clamp(transform.position.x, minPos.x, maxPos.x),

                    Mathf.Clamp(transform.position.y, minPos.y, maxPos.y),

                    Mathf.Clamp(transform.position.z, transform.position.z, transform.position.z)
                );
            }
        }
    }
}
