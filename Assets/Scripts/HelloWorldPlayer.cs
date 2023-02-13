using Unity.Netcode;
using UnityEngine;

namespace HelloWorld
{
    public class HelloWorldPlayer : NetworkBehaviour
    {
        public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
        public Vector3 randomPosition;
        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                Move();
            }
        }

        public void Move()
        {
            if (NetworkManager.Singleton.IsServer)
            {
                Debug.Log("Move is Server");
                randomPosition = GetRandomPositionOnPlane();
                transform.position = randomPosition;
                Position.Value = randomPosition;
            }
            else
            {
                Debug.Log("Move is not Client");
                randomPosition = GetRandomPositionOnPlane();
                transform.position = randomPosition;
                SubmitPositionRequestServerRpc();
            }
        }

        [ServerRpc]
        void SubmitPositionRequestServerRpc(ServerRpcParams rpcParams = default)
        {
            Debug.Log("ServerRpc: " + randomPosition);
            // Position.Value = GetRandomPositionOnPlane();
            transform.position = randomPosition;
        }

        static Vector3 GetRandomPositionOnPlane()
        {
            return new Vector3(Random.Range(-3.5f, 3.5f), Random.Range(-1.5f, 2.5f), 0);
        }

        void Update()
        {
            // transform.position = Position.Value;
        }
    }
}