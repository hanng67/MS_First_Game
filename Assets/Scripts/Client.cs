using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;
using PlayFab.MultiplayerModels;
using PlayFab;

public class Client : MonoBehaviour
{
    static public Client instance;
    public NetworkDriver m_Driver;
    public NetworkConnection m_Connection;
    private bool m_Done;
    public GameObject starObj;

    private void RequestMultiplayerServer()
    {
        RequestMultiplayerServerRequest requestData = new RequestMultiplayerServerRequest();
        requestData.BuildId = "45e9f9d0-5d16-49a0-bfeb-a924aa4ae383"; // Build ID from the Multiplayer Dashboard
        requestData.PreferredRegions = new List<string>() { "EastUs" };
        requestData.SessionId = System.Guid.NewGuid().ToString(); // Generate a Session ID
        PlayFabMultiplayerAPI.RequestMultiplayerServer(requestData, OnRequestMultiplayerServer, OnRequestMultiplayerServerError);
    }

    private void OnRequestMultiplayerServer(RequestMultiplayerServerResponse response)
    {
        connectToServer(response.IPV4Address, (ushort)response.Ports[0].Num);
    }

    private void OnRequestMultiplayerServerError(PlayFabError error)
    {
        Debug.Log(error.ErrorMessage);
    }

    private void connectToServer(string address, ushort port)
    {
        Debug.Log("Connecting to " + address + ":" + port);
        m_Driver = NetworkDriver.Create();
        m_Connection = default(NetworkConnection);

        var endpoint = NetworkEndPoint.Parse(address, port);
        m_Connection = m_Driver.Connect(endpoint);
    }

    void Start()
    {
        instance = this;
        RequestMultiplayerServer();
        // m_Driver = NetworkDriver.Create();
        // m_Connection = default(NetworkConnection);

        // var endpoint = NetworkEndPoint.LoopbackIpv4;
        // endpoint.Port = 9000;
        // m_Connection = m_Driver.Connect(endpoint);
    }

    public void OnDestroy()
    {
        m_Driver.Dispose();
    }

    void Update()
    {
        m_Driver.ScheduleUpdate().Complete();

        if (!m_Connection.IsCreated)
        {
            if (!m_Done)
                Debug.Log("Something went wrong during connect");
            return;
        }

        DataStreamReader stream;
        NetworkEvent.Type cmd;
        while ((cmd = m_Connection.PopEvent(m_Driver, out stream)) != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {
                Debug.Log("We are now connected to the server");
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                var data = stream.ReadFixedString32();
                string[] newPosStr = data.ToString().Split(',');
                Vector3 newPos = new Vector3(float.Parse(newPosStr[0]), float.Parse(newPosStr[1]), 0);
                if (starObj.transform.position != newPos)
                {
                    starObj.transform.position = newPos;
                    Debug.Log($"Star get new position: ({newPos})");
                }
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Client got disconnected from server");
                m_Connection = default(NetworkConnection);
            }
        }
    }

    public void updatePosStar(Vector3 pos){
        m_Driver.BeginSend(m_Connection, out var writer);
        writer.WriteFixedString32($"{pos.x},{pos.y}");
        m_Driver.EndSend(writer);
    }
}