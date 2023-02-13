using System.Collections;
using System;
using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;
using PlayFab;

public class Server : MonoBehaviour
{
    public NetworkDriver m_Driver;
    private NativeList<NetworkConnection> m_Connections;
    private Vector3 starPos;
    IEnumerator ReadyForPlayers()
    {
        yield return new WaitForSeconds(.5f);
        PlayFabMultiplayerAgentAPI.ReadyForPlayers();
    }

    private void OnServerActive()
    {
        StartServer();
    }

    private void OnAgentError(string error)
    {
        Debug.Log(error);
    }

    private void OnShutdown()
    {
        Debug.Log("Server is shutting down");
        m_Driver.Dispose();
        m_Connections.Dispose();
        StartCoroutine(Shutdown());
    }

    IEnumerator Shutdown()
    {
        yield return new WaitForSeconds(5f);
        Application.Quit();
    }

    private void OnMaintenance(DateTime? NextScheduledMaintenanceUtc)
    {
        Debug.LogFormat("Maintenance scheduled for: {0}", NextScheduledMaintenanceUtc.Value.ToLongDateString());
    }

    void StartPlayFabAPI()
    {
        PlayFabMultiplayerAgentAPI.Start();
        PlayFabMultiplayerAgentAPI.OnMaintenanceCallback += OnMaintenance;
        PlayFabMultiplayerAgentAPI.OnShutDownCallback += OnShutdown;
        PlayFabMultiplayerAgentAPI.OnServerActiveCallback += OnServerActive;
        PlayFabMultiplayerAgentAPI.OnAgentErrorCallback += OnAgentError;


        StartCoroutine(ReadyForPlayers());
    }

    void StartServer()
    {
        m_Driver = NetworkDriver.Create();
        var endpoint = NetworkEndPoint.AnyIpv4;
        // endpoint.Port = 9000;

        var connectionInfo = PlayFabMultiplayerAgentAPI.GetGameServerConnectionInfo();
        if (connectionInfo != null)
        {
            // Set the server to the first available port
            foreach (var port in connectionInfo.GamePortsConfiguration)
            {
                endpoint.Port = (ushort)port.ServerListeningPort;
                break;
            }
        }
        if (m_Driver.Bind(endpoint) != 0)
            Debug.Log("Failed to bind to port " + endpoint.Port);
        else
            m_Driver.Listen();

        m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);

        starPos = new Vector3(-3.7f, -3f, 0);
    }

    void Start()
    {
        StartServer();
    }

    void Update()
    {
        m_Driver.ScheduleUpdate().Complete();

        // Clean up connections
        for (int i = 0; i < m_Connections.Length; i++)
        {
            if (!m_Connections[i].IsCreated)
            {
                m_Connections.RemoveAtSwapBack(i);
                --i;
            }
        }

        // Accept new connections
        NetworkConnection c;
        while ((c = m_Driver.Accept()) != default(NetworkConnection))
        {
            m_Connections.Add(c);
            Debug.Log("Accepted a connection");
        }

        DataStreamReader stream;
        for (int i = 0; i < m_Connections.Length; i++)
        {
            if (!m_Connections[i].IsCreated)
                continue;

            NetworkEvent.Type cmd;
            while ((cmd = m_Driver.PopEventForConnection(m_Connections[i], out stream)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Data)
                {
                    var data = stream.ReadFixedString32();
                    string[] newPosStr = data.ToString().Split(',');
                    Vector3 newPos = new Vector3(float.Parse(newPosStr[0]), float.Parse(newPosStr[1]), 0);

                    starPos = newPos;
                    Debug.Log($"Character {i} collides with the star");
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log($"Client disconnected from server");
                    m_Connections[i] = default(NetworkConnection);
                }
            }

            m_Driver.BeginSend(NetworkPipeline.Null, m_Connections[i], out var writer);
            writer.WriteFixedString32($"{starPos.x},{starPos.y}");
            m_Driver.EndSend(writer);
        }
    }
}