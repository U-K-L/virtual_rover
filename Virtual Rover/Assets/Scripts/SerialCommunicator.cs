/* Created by Noah Williams on 9/8/2019.
 * 
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System.Threading;
using System;

public class SerialCommunicator : MonoBehaviour
{
    private SerialPort stream;
    private Thread thread;
    private Queue inputQueue; //Input queue for thread. 
    private Queue outputQueue; //Output queue for thread communication.
    public Vector3 _roverPositions;
    public float position;
    public int baudrate = 9600;
    public string port = "COM4";
    public int delay = 70; //How long to wait between each message.
    public bool looping = true;
    // Start is called before the first frame update
    void Start()
    {

        outputQueue = Queue.Synchronized(new Queue()); //Makes it thread safe.
        inputQueue = Queue.Synchronized(new Queue());
        Open();
        thread = new Thread(ThreadLoop);
        thread.Start();
    }

    // Update is called once per frame
    void Update()
    {
        string r = ReadThread();
        if (r != null)
        {
            position = float.Parse(r);
            transform.position = new Vector3(position, transform.position.y, transform.position.z);
        }
        Debug.Log(position);
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
