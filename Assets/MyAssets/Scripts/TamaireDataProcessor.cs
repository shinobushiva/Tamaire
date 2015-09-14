using UnityEngine;
using System.Collections;

public class TamaireDataProcessor : MonoBehaviour {

	private XBeeSerialServerCommunicator com;
	public GameMaster master;

	private int lastSeqNum = -1;

	private int idxCounter = 0;

	private class Worker {
		private int idxCounter = 0;
		private bool headerReceived = false;

		private int seqnum = -1;
		private TamaireDataProcessor tdp;

		public Worker(TamaireDataProcessor tdp){
			this.tdp = tdp;
		}

		public void Process(byte data){

			byte masked = (byte)(data & 0x7F);
			bool hOne = (data & 0x80) > 0;


			if (data == 0x31) {
				headerReceived = true;
				idxCounter = 1;
				return;
			}

			if (!hOne) {
				headerReceived = false;
				return;
			}

			if (headerReceived) {
				if (idxCounter == 1) {
					if (masked == seqnum) {
						//duplicated data received
						headerReceived = false;
						idxCounter = 0;
						return;
					}
					seqnum = masked;
				}else if (idxCounter >= 2 && idxCounter <= 13) {
					tdp.master.Add (masked, idxCounter - 2);
				} else if (idxCounter == 14) {
					if(data != 0xFF)
						print ("Maybe error");

					idxCounter = 0;
					headerReceived = false;
					return;
				}
			}

			idxCounter = (idxCounter+1) % 15;
			if (idxCounter == 0) {
				headerReceived = false;
			}

		}
	}

	// Use this for initialization
	void Start () {

		Worker worker = new Worker (this);

		com = GetComponent<XBeeSerialServerCommunicator> ();
		com.dataReceived += (buf, from, len) => {

			for(int i=from;i<from+len;i++){
				worker.Process(buf[i]);
			}
		};
	}



	
	// Update is called once per frame
	void Update () {
	
	}
}
