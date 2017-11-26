using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using MyNet;
using System;

public class MyNetUnityClient : MonoBehaviour {

    private static Packet heartbeat = new Packet(Packet.HEADER.__HEART_BEAT);

    private float heartbeat_period = (float)((Config.MAX_HEARTBEAT_PERIOD / 10000000) - 1.0f);

    static MyNetUnityClient instance = null;
    public static MyNetUnityClient Instance {
        get {
            if (instance != null)
            {
                return instance;
            }
            else {
                return null;
            }
        }
    }

    public delegate void OnAction();
    public delegate void OnMessage(Packet packet);
    public OnMessage onPacketReceive;
    public OnAction onConnect;

    Queue<Packet> sendPacketQueue = new Queue<Packet>(10);
    Queue<Packet> receivedPacketQueue = new Queue<Packet>(10);
    object receiveQueueLock = new object();
    object sendQueueLock = new object();

    Socket serverSocket;

    SocketAsyncEventArgs connectEventArgs;

    SocketAsyncEventArgs sendEventArgs;
    SocketAsyncEventArgs recvEventArgs;

    IPEndPoint endPoint;

    [SerializeField]
    string ipAddress;

    PacketResolver resolver;

    #region UNITY 이벤트 함수들

    private void Awake()
    {
        Application.runInBackground = true;
        if (instance == null)
        {
            instance = this;   
        }
        else {
            throw new Exception("InstanceAlreadyInitializeException");
        }
    }


    private void FixedUpdate()
    {

        if (receivedPacketQueue.Count > 0 && onPacketReceive != null) {
            lock (receiveQueueLock) {
                Queue<Packet>.Enumerator iter = receivedPacketQueue.GetEnumerator();
                while (iter.MoveNext()) {
                    onPacketReceive.Invoke(iter.Current.Clone() as Packet);
                }
                receivedPacketQueue.Clear();
            }
        }
    }
    #endregion


    public void Connect() {
        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, true);
        endPoint = new IPEndPoint(IPAddress.Parse(ipAddress), 2500);

        resolver = new PacketResolver();

        connectEventArgs = new SocketAsyncEventArgs();

        sendEventArgs = new SocketAsyncEventArgs();
        recvEventArgs = new SocketAsyncEventArgs();

        connectEventArgs.RemoteEndPoint = endPoint;
        connectEventArgs.Completed += new System.EventHandler<SocketAsyncEventArgs>(OnConnect);

        sendEventArgs.SetBuffer(new byte[Config.MAX_SESSION_BUFFER_SIZE], 0, Config.MAX_SESSION_BUFFER_SIZE);
        recvEventArgs.SetBuffer(new byte[Config.MAX_SESSION_BUFFER_SIZE], 0, Config.MAX_SESSION_BUFFER_SIZE);

        sendEventArgs.Completed += OnSend;
        recvEventArgs.Completed += OnReceive;

        serverSocket.ConnectAsync(connectEventArgs);

        GameObject.DontDestroyOnLoad(this.gameObject);
    }

    public void StartHeartBeat() {
        StartCoroutine(SendHeartBeat());
    }

    IEnumerator SendHeartBeat()
    {
        while (true)
        {
            Debug.Log("하트비트 보냄");
            this.Send(heartbeat);
            yield return new WaitForSecondsRealtime(heartbeat_period);
        }
    }


    public void Disconnect() {
        serverSocket.Shutdown(SocketShutdown.Both);
        serverSocket.Close();
    }

    private void OnConnect(object sender, SocketAsyncEventArgs e)
    {
        if (e.SocketError == SocketError.Success)
        {
            Debug.Log(e.SocketError.ToString());
            Debug.Log(e.RemoteEndPoint.ToString() + "에 연결되었습니다.");
            StartReceive();
        }
        else {
            Debug.Log(e.SocketError.ToString());
        }
    }

    public void Send(Packet packet) {
        if (sendPacketQueue.Count == 0)
        {
            lock (sendQueueLock)
            {
                sendPacketQueue.Enqueue(packet);
            }
            StartSend();
        }
        else
        {
            lock (sendQueueLock)
            {
                sendPacketQueue.Enqueue(packet);
            }
        }
    }

    private void StartSend() {

        lock (sendQueueLock) {
            Packet packet = sendPacketQueue.Peek();

            sendEventArgs.SetBuffer(sendEventArgs.Offset, packet.DataLength);
            Array.Copy(packet.Data, 0, sendEventArgs.Buffer, sendEventArgs.Offset, packet.DataLength);
        }

        if (!serverSocket.SendAsync(sendEventArgs))
        {
            OnSend(null, sendEventArgs);
        }
    }

    private void StartReceive() {
        if (!serverSocket.ReceiveAsync(recvEventArgs)) {
            OnReceive(null, recvEventArgs);
        }
    } 

    private void OnSend(object sender, SocketAsyncEventArgs e) {
        if (e.SocketError == SocketError.Success)
        {
            lock (sendQueueLock) {
                sendPacketQueue.Dequeue();
                if (sendPacketQueue.Count > 0) {
                    StartSend();
                }
            }
        }
        else {
            Debug.Log("패킷 전송에 문제가 생겼습니다. " + e.SocketError.ToString());
        }
    }

    private void OnReceive(object sender, SocketAsyncEventArgs e) {
        if (e.SocketError == SocketError.Success) {
            Stack<Packet> packets = resolver.Resolve(e.Buffer, e.Offset, e.BytesTransferred);
            var iter = packets.GetEnumerator();
            while (iter.MoveNext()) {
                if (iter.Current.Head == Packet.HEADER.__WRONG)
                {
                    Debug.Log("패킷의 헤더가 잘못되었습니다.");
                }
                else
                {
                    lock (receiveQueueLock)
                    {
                        receivedPacketQueue.Enqueue(iter.Current);
                    }
                    Debug.Log(iter.Current.ToString());
                }
            }
            StartReceive();
        }
        else {
            Debug.Log("패킷 수신에 문제가 생겼습니다. " + e.SocketError.ToString());
        }
    }
}
