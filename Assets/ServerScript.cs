﻿using UnityEngine;
using System.Collections;


using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Globalization;

public class ServerScript: MonoBehaviour {

	// receiving Thread
	Thread receiveThread;

	// udpclient object
	UdpClient client;

	// public
	// public string IP = "127.0.0.1"; default local
	public int port; // define > init

	// infos
	public string lastReceivedUDPPacket="";

	// Game state
	public GameScript game_object;
	public float[] x;
	public float[] y;
	public float[] rotation;
	public int[] velocity;
	public bool loaded;

	// start from unity3d
	public void Start()
	{
		game_object = GameObject.Find ("Game").GetComponent<GameScript>();
		loaded = false;

		// Init positions
		x = new float[2];
		y = new float[2];
		rotation = new float[2];
		velocity = new int[2];
		x [0] = -5;
		x [1] = 5;
		y [0] = 0;
		y [1] = 0;
		rotation [0] = 0;
		rotation [1] = 180;
		velocity [0] = 0;
		velocity [1] = 1;


		init();
	}

	//Handles the motion of the cars
	void Update(){
		if (loaded) {
			for(int i = 0; i <= 1; i++){
				game_object.setCarPosition (i, x [i], y [i]);
				game_object.setCarRotation (i, rotation [i]);
				game_object.setCarVelocity (i, velocity [i]);
			}
		}
	}

	// OnGUI
	void OnGUI()
	{
		Rect rectObj=new Rect(40,10,200,400);
		GUIStyle style = new GUIStyle();
		style.alignment = TextAnchor.UpperLeft;
		GUI.Box(rectObj,"# UDPReceive\n127.0.0.1 "+port+" #\n"
			+ "shell> nc -u 127.0.0.1 : "+port+" \n"
			+ "\nLast Packet: \n"+ lastReceivedUDPPacket
			,style);
	}

	// init
	private void init()
	{
		print("UDPSend.init()");

		port = 2122;

		receiveThread = new Thread(
			new ThreadStart(ReceiveData));
		receiveThread.IsBackground = true;
		receiveThread.Start();

	}

	// receive thread
	private  void ReceiveData()
	{
		client = new UdpClient(port);
		while (true)
		{

			try
			{
				// Bytes empfangen.
				IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
				byte[] data = client.Receive(ref anyIP);

				// Bytes mit der UTF8-Kodierung in das Textformat kodieren.
				string text = Encoding.UTF8.GetString(data);

				// Den abgerufenen Text anzeigen.
				print(">> " + text);

				// latest UDPpacket
				lastReceivedUDPPacket=text;

				// ....
				print("message:");
				print(text);
				processMessage(text);
				loaded |= true;
			}
			catch (Exception err)
			{
				print(err.ToString());
			}
		}
	}

	// getLatestUDPPacket
	// cleans up the rest
	public string getLatestUDPPacket()
	{
		return lastReceivedUDPPacket;
	}

	public float parseStringToFloat(string str) {
		print ("DEBUG");
		print(str);
		return Single.Parse (str, CultureInfo.InvariantCulture);
	}

	public void processMessage(string text) {
		var clean_text = text.Substring (1, text.Length - 2);
		var splited_text = clean_text.Split (',');

		// parse the message
		var attr = new float[4];
		for (int i = 0; i < 4; i++) {
			attr [i] = parseStringToFloat(splited_text [i]);
		}

		for(int i = 0; i <= 1; i++){
			updateRotation (i, attr[i*2], attr[i*2 + 1]);
			updateVelocity (i, attr[i*2], attr[i*2 + 1]);

			// update the position
			x[i] = attr[i*2];
			y[i] = attr[i*2 + 1];
		}
	}

	public void updateRotation(int car_id, float new_x, float new_y) {
		var new_vect = new Vector2 (new_x, new_y);
		var old_vect = new Vector2 (x [car_id], y [car_id]);
		rotation[car_id] = Vector2.Angle(old_vect, new_vect);
	}

	public void updateVelocity(int car_id, float new_x, float new_y) {
		
	}
}