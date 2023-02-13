using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class char_move : MonoBehaviour
{
    public Animator anim;
    public float charSpeed = 10;
    public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();

    // Update is called once per frame
    void Update()
    {
        // if (IsOwner)
        // {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            bool isMove = (horizontal != 0) || (vertical != 0);

            anim.SetBool("isMove", isMove);
            transform.position += new Vector3(horizontal, vertical, 0) * charSpeed * Time.deltaTime;
        //     if (NetworkManager.Singleton.IsClient)
        //     {
        //         if(!isMove) return;

        //         SubmitPositionRequestServerRpc(transform.position);
        //     }
        // }
        // else
        // {
        //     transform.position = Position.Value;
        // }
    }

    // [ServerRpc]
    // void SubmitPositionRequestServerRpc(Vector3 pos)
    // {
    //     Position.Value = pos;
    // }
}
