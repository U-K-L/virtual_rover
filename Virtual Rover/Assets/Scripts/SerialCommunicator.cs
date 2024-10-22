﻿/* Created by Noah Williams on 9/8/2019.
 * 
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System.Threading;
using System;
using System.Text.RegularExpressions;
using System.Linq;

public class SerialCommunicator : MonoBehaviour
{
    private SerialPort stream;
    private Thread thread;
    private Queue inputQueue; //Input queue for thread. 
    private Queue outputQueue; //Output queue for thread communication.
    private float last_dist_echo = 0;
    private Vector3 target;
    public GameObject targetObj;

    public Vector3 _roverPositions;
    public float position;
    public int baudrate = 9600;
    public string port = "COM4";
    public int delay = 70; //How long to wait between each message.
    public bool looping = true;
    public List<Tiles> tiles;
    public Tiles ghex;
    public int mapSize = 10;
    public float smoothSpeed = 0.125f;
    // Start is called before the first frame update
    void Start()
    {
        targetObj = Instantiate(targetObj, new Vector3(0, 0, 0), Quaternion.identity);
        createMap();
        outputQueue = Queue.Synchronized(new Queue()); //Makes it thread safe.
        inputQueue = Queue.Synchronized(new Queue());
        Open();
        thread = new Thread(ThreadLoop);
        thread.Start();
    }

    void createMap()
    {
        for(int i = 0; i < mapSize; i++)
        {
            tiles.Add(Instantiate(ghex, new Vector3(0, 0, 0), Quaternion.identity));
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, target, smoothSpeed);
        transform.position = new Vector3(smoothedPosition.x, transform.position.y, transform.position.z);
        targetObj.transform.position = new Vector3(transform.position.x, 0, 0);
        string r = ReadThread();
        if (r != null)
        {
            //Debug.Log(r);
            CommandParser(r);
            /*

            */
        }
       // Debug.Log(position);
    }

    public void CommandParser(string command)
    {
        if (command.StartsWith("d_e"))
        {
            getDistance(command);
        }

        if (command.StartsWith("m_p"))
        {
            getTiles(command);
        }

        if (command.StartsWith("reset"))
        {
            resetTiles();
        }
    }

    void resetTiles()
    {
        foreach(Tiles tile in tiles)
        {
            tile.transform.localPosition = new Vector3(0,0,0);
        }
    }

    void getTiles(string command)
    {
        command = Regex.Replace(command, @"[m_p]", "");
        string[] vectors = Regex.Split(command, @"[|]");
        for (int i = 0; i < vectors.Length-1; i++)
        {
            string[] vector = Regex.Split(vectors[i], @"[,]");
            Debug.Log(vector[0]);
            tiles[i].transform.localPosition = new Vector3(float.Parse(vector[0]), float.Parse(vector[1]), float.Parse(vector[2]));


        }
    }

    void getDistance(string command)
    {

        command = Regex.Replace(command, @"[d_e]", "");
        position = float.Parse(command);
        if((Mathf.Abs(position-last_dist_echo) > 0.2)){
            //transform.position = new Vector3(position, transform.position.y, transform.position.z);
            target = new Vector3(position, transform.position.y, transform.position.z);
        }
        

        last_dist_echo = position;
       // Debug.Log(command);
    }
    //Opens the port for serial connection.
    public void Open()
    {
        stream = new SerialPort(port, baudrate);
        stream.ReadTimeout = delay;
        stream.Open();
    }

    public void SendToPort(string command)
    {
        outputQueue.Enqueue(command);
        stream.WriteLine(command);
        stream.BaseStream.Flush();

    }

    public string ReadFromSerial(int timeout = 0)
    {
        stream.ReadTimeout = timeout;
        try
        {
            return stream.ReadLine();
        }
        catch (TimeoutException)
        {
            return null;
        }
    }

    public string ReadThread()
    {

       if (inputQueue.Count == 0)
            return null;
        return (string)inputQueue.Dequeue();
    }

    public void ThreadLoop()
    {
        //Start the loop.
        while (IsLooping())
        {

            //Send to port.
            if(outputQueue.Count != 0)
            {
                string command = (string)outputQueue.Dequeue();
                SendToPort(command);
            }
            //Read the port
            string result = ReadFromSerial(delay);
            if(result != null)
            {
                inputQueue.Enqueue(result);
            }
        }
        stream.Close();
    }

    public void StopThread()
    {
        lock (this)
        {
            looping = false;
        }
    }

    public bool IsLooping()
    {
        lock (this)
        {
            return looping;
        }
    }

    public void OnDestroy()
    {
        StopThread();
    }

}
