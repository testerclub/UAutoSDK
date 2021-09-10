using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using TcpServer;
using UnityEngine;
using UnityEngine.Profiling;
using Debug = UnityEngine.Debug;


namespace UAutoSDK
{
    public class UAutoSdkInit : MonoBehaviour
    {
        private int fileNum = 0;

        private int frameNum = 0;

        private bool isRecording = false;
        private bool startSample = true;

        private IEnumerator startRecordProfileIEnumerator;

        private UAutoRuner runner;

        private StringBuilder dataJson = new StringBuilder();

        private string profilerDataPath;
        private string profilerDataName;
        

        private void BeginSample(string fileName)
        {
            Profiler.SetAreaEnabled(ProfilerArea.CPU, true);
            Profiler.SetAreaEnabled(ProfilerArea.Rendering, true);
            Profiler.SetAreaEnabled(ProfilerArea.Memory, true);
            Profiler.SetAreaEnabled(ProfilerArea.Physics, true);
            Profiler.SetAreaEnabled(ProfilerArea.UI, true);
            //标记data文件最大使用1GB储存空间
            Profiler.maxUsedMemory = 1024 * 1024 * 1024;

            Profiler.logFile = Application.persistentDataPath + "/" + fileName;
            Profiler.enableBinaryLog = true;
            Profiler.enabled = true;
            
            profilerDataPath = Application.persistentDataPath;
            profilerDataName = fileName;

            Debug.Log(Application.persistentDataPath + "/" + fileName);
        }

        private void EndSample()
        {
            Profiler.enabled = false;
            Profiler.logFile = "";
            Profiler.enableBinaryLog = false;

            Debug.Log("Finish");
            
            try
            {
                dataJson.Clear();
                dataJson.Append("{\"path\":\"" + profilerDataPath + "\",\"name\":\"" + profilerDataName + "\"}");
                runner.server.Send(runner.client.TcpClient, runner.prot.pack(dataJson.ToString()));
            }
            catch (Exception e)
            {
                runner.server.Send(runner.client.TcpClient, runner.prot.pack(e.ToString()));
            }
        }

        private void ProfilerInit()
        {
            frameNum = 0;
            fileNum = 0;
            isRecording = false;
        }

        IEnumerator StartRecordProfile()
        {
            ProfilerInit();
            while (startSample)
            {
                if (isRecording)
                {
                    frameNum++;
                    if (frameNum >= 300)
                    {
                        EndSample();
                        fileNum++;
                        frameNum = 0;
                        isRecording = false;
                    }
                }
                else
                {
                    BeginSample("AutoTest-" + DateTime.Now.ToString(format: "yyyy-MM-dd-HH-mm-ss") + "-" + fileNum);
                    isRecording = true;
                    frameNum++;
                }

                yield return null;
            }
        }



        void Awake()
        {
            runner = gameObject.AddComponent<UAutoRuner>();
            try
            {
                if (runner != null)
                {
                    runner.Init();

                    runner.m_Handlers.addMsgHandler("RecordProfile", RecordProfileHandler);

                    runner.Run();
                }
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("[ERROR] UAutoSDk Reg MsgHandler Error. {0}", e.ToString());
            }
        }
        
        private object RecordProfileHandler(string[] args)
        {
            string response = "";
            try
            {
                bool startArg = args.Length > 1 && (args[1] == "1");
                response = "ok";

                if (startArg)
                {
                    if (startRecordProfileIEnumerator == null)
                    {
                        startSample = true;
                        startRecordProfileIEnumerator = StartRecordProfile();
                        StartCoroutine(startRecordProfileIEnumerator);

                        response = "record profile start";
                    }
                    else
                    {
                        response = "It's already started";
                    }
                }
                else
                {
                    if (startRecordProfileIEnumerator != null)
                    {
                        if (startSample)
                        {
                            startSample = false;
                            EndSample();
                        }

                        StopCoroutine(startRecordProfileIEnumerator);
                        startRecordProfileIEnumerator = null;

                        response = "record profile stop";
                    }
                    else
                    {
                        response = "It hasn't started yet";
                    }
                }
            }
            catch (Exception e)
            {
                response = "-1";
            }
            return response;
        }
    }
}