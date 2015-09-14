using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Threading;
using SpicyPixel.Threading;
using SpicyPixel;
using SpicyPixel.Threading.Tasks;

public class XBeeSerialServerCommunicator : ConcurrentBehaviour {


    public string server = "localhost:33334";

    //
    private System.Net.Sockets.TcpClient tcp;
	private System.Net.Sockets.NetworkStream ns;

	private byte[] masks;
	private byte[] buf = new byte[512];

    public bool isConnectedToServer = false;

	public delegate void SerialDataReceived(byte[] buf, int from, int len);
	public event SerialDataReceived dataReceived;


	// Use this for initialization
	void Start () {


//		dataReceived += (buf, from, len) => {
//			string s = BitConverter.ToString(buf,0, len).Replace("-", " ");
//			print (s);
//		};

		
	}

	private bool startRequested;
	private Thread m_Thread;

	public void StartCommunication(){
		ConnectToServer ();
		
		m_Thread = new Thread (WorkerThread);
		m_Thread.Start ();

	}

	public void StopCommunication(){
		
		DisconnectFromServer ();
	}


	public void WorkerThread(){
	
		while (!isConnectedToServer) {
			
			Thread.Sleep(10);
		}

		while (isConnectedToServer) {
			if(!ns.CanRead)
				break;
			
			int len = ns.Read (buf, 0, buf.Length);
			if (len > 0) {

				taskFactory.StartNew(()=>{
					dataReceived(buf, 0, len);
				});
			}
			
			Thread.Sleep(1);
		}

//		taskFactory.StartNew (Task());
	}
	
	// Update is called once per frame
	void Update () {

//		if (startRequested && !isConnectedToServer) {
//			m_Thread = new Thread (WorkerThread);
//			m_Thread.Start ();
//		}
	
	}

    void OnDestroy ()
    {
		//SaveSettings ();
		if(m_Thread != null)
			m_Thread.Abort();

        DisconnectFromServer ();
    }


    private void ConnectToServer ()
    {
        
        try {
            string[] ss = server.Split (':');
            tcp = new System.Net.Sockets.TcpClient (ss [0], int.Parse (ss [1]));
            //NetworkStreamを取得する
            ns = tcp.GetStream ();
            
//            masks = new byte[(int)(Mathf.Ceil (modules.Length / 7f))];
//            buf = new byte[modules.Length * 5];
        } catch (Exception e) {
            print (e);
            
            return;
        }
        
        isConnectedToServer = true;
    }
    
    public void DisconnectFromServer ()
    {
        print("Disconnect form Server");
        
        if (ns != null) {
            byte[] ba = new byte[]{0x65,0x6e,0x64};//System.Text.Encoding.Unicode.GetBytes ("end");
            ns.Write (ba, 0, ba.Length);
            ns.Flush();
            //閉じる
            ns.Close ();
            tcp.Close ();
            
        }
        
        isConnectedToServer = false;
    }


  

  
    

}
