//using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Profiling;
using System.Text;
using System.Net.Sockets;
using TcpServer;
using UnityEngine;
//using Debug = UnityEngine.Debug;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Reflection;
// using UnityEditor;
//using System.Linq;

namespace UAutoSDK
{

    public class ReflectionTool
    {
        public static PropertyInfo GetPropertyNest(Type t, String name)
        {

            PropertyInfo pi = t.GetProperty(name);

            if (pi != null)
            {
                return pi;
            }

            if (t.BaseType != null)
            {
                return GetPropertyNest(t.BaseType, name);
            }

            return null;
        }

        public static object GetComponentAttribute(GameObject target, Type t, String attributeName)
        {
            if (target == null || t == null)
                return null;

            Component component = target.GetComponent(t);

            if (component == null)
                return null;

            PropertyInfo pi = GetPropertyNest(t, attributeName);

            if (pi == null || !pi.CanRead)
            {
                return null;
            }

            return pi.GetValue(component, null);
        }

        public static bool SetComponentAttribute(GameObject obj, Type t, String attributeName, object value)
        {

            if (t == null)
            {
                return false;
            }

            Component comp = obj.GetComponent(t);

            if (comp == null)
            {
                return false;
            }

            PropertyInfo pi = GetPropertyNest(t, attributeName);


            if (pi == null || !pi.CanWrite)
            {
                return false;
            }

            pi.SetValue(comp, value, null);

            return true;
        }
    }


    public class Error
    {
        public readonly static string NotFoundMessage = "error:notFound";
        public readonly static string ExceptionMessage = "error:exceptionOccur";
    }

    public class UAutoRuner : MonoBehaviour
    {

        public static readonly string UIRootTypeName = "UIRoot, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
        public static readonly string UICameraTypeName = "UICamera, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
        public static readonly string UIPanelTypeName = "UIPanel, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
        public static readonly string UIButtonName = "UIButton, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
        public static readonly string UIToggleName = "UIToggle, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
        public static readonly string UIEventListenerName = "UIEventListener, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
        public static readonly string UIWidgetTypeName = "UIWidget, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
        public static readonly string UIRectTypeName = "UIRect, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
        public static readonly string UILabelTypeName = "UILabel, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
        public static readonly string UIInputTypeName = "UIInput, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
        public static readonly string UITextureTypeName = "UITexture, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
        public static readonly string UISpriteTypeName = "UISprite, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";

        public const int versionCode = 6;
        public int port = 13000;
        private bool mRunning;
        //private bool responseFlag = false;
        private bool requestFlag = false;
        public AsyncTcpServer server = null;
        public MsgParser m_Handlers = null;
        private MsgProtocol prot = null;
        private StringBuilder response = new StringBuilder();
        //private ConcurrentDictionary<string, TcpClientState> inbox = new ConcurrentDictionary<string, TcpClientState>();
        //private KeyValuePair<string, TcpClientState> client;
        private TcpClientState client;
        private List<string> msgs;
        private Dictionary<string, string> data = new Dictionary<string, string>();
        private List<string> dataList = new List<string>();
        private StringBuilder dataJson = new StringBuilder();
        private UnityEngine.GameObject target;
        private System.Collections.IEnumerator DebugModeEnumerator = null;
        


        private System.Reflection.MethodInfo UICameraNotify = null;

        private static Type UIRootType = null;
        private static Type UICameraType = null;
        private static Type UIPanelType = null;
        private static Type UIButtonType = null;
        private static Type UIToggleType = null;
        private static Type UIEventListenerType = null;
        private static Type UIWidgetType = null;
        private static Type UIRectType = null;

        private static Type UILabelType = null;
        private static Type UIInputType = null;
        private static Type UITextureType=null;
        private static Type UISpriteType=null;

        
        private bool pauseDebugMode = false;

        public void Init()
        {
            Application.runInBackground = true;
            DontDestroyOnLoad(this);
            prot = new MsgProtocol();
            m_Handlers = new MsgParser();
            m_Handlers.addMsgHandler("getServerVersion", GetSDKVersion);
            m_Handlers.addMsgHandler("enableLogging", enableLogging);
            m_Handlers.addMsgHandler("closeConnection", closeHandler);
            m_Handlers.addMsgHandler("findObject", findObjectHandler);
            m_Handlers.addMsgHandler("tapObject", tapObjectHandler);
            m_Handlers.addMsgHandler("findObjectsWhereNameContains", findObjectsWhereNameContainsHandler);
            m_Handlers.addMsgHandler("getText", getTextHandler);
            m_Handlers.addMsgHandler("setText", setTextHandler);
            m_Handlers.addMsgHandler("findObjectAndTap", findObjectAndTapHandler);
            m_Handlers.addMsgHandler("findObjectInRangeWhereNameContains", findObjectInRangeWhereNameContainsHandler);
            m_Handlers.addMsgHandler("objectExist", objectExistHandler);
            m_Handlers.addMsgHandler("findObjectInRangeWhereTextContains", findObjectInRangeWhereTextContainsHandler);
            m_Handlers.addMsgHandler("getScreen", getScreenHandler);
            m_Handlers.addMsgHandler("findChild", findChildHandler);
            m_Handlers.addMsgHandler("findObjectByLevel", findObjectByLevelHandler);
            m_Handlers.addMsgHandler("tapScreen", tapScreenHandler);
            m_Handlers.addMsgHandler("getRectTransformPoints", getRectTransformPointsHandler);
            m_Handlers.addMsgHandler("getValueOnComponent", getValueOnComponentHandler);
            m_Handlers.addMsgHandler("findObjectByPoint", findObjectByPointHandler);
            m_Handlers.addMsgHandler("findObjectAllChildren", findObjectAllChildrenHandler);
            m_Handlers.addMsgHandler("findAllObjectWhereTextContains", findAllObjectWhereTextContainsHandler);
            m_Handlers.addMsgHandler("getPNGScreenshot", getPNGScreenshotHandler);
            m_Handlers.addMsgHandler("dragObject", dragObjectHandler);
            m_Handlers.addMsgHandler("debugMode", debugModeHandler);
            m_Handlers.addMsgHandler("pauseDebugMode", pauseDebugModeHandler);
            m_Handlers.addMsgHandler("resumeDebugMode", resumeDebugModeHandler);
            m_Handlers.addMsgHandler("stopDebugMode", stopDebugModeHandler);
            m_Handlers.addMsgHandler("findAllText", findAllTextHandler);
            m_Handlers.addMsgHandler("objectFind", objectFindHandler);
            m_Handlers.addMsgHandler("getParent", getParentHandler);
            m_Handlers.addMsgHandler("getHierarchy", getHierarchyHandler);
            m_Handlers.addMsgHandler("getInspector", getInspectorHandler);

            m_Handlers.addMsgHandler("RecordProfile", RecordProfileHandler);

            InitNGUI();
        }

        private void InitNGUI()
        {
            UIRootType = Type.GetType(UIRootTypeName);
            UICameraType = Type.GetType(UICameraTypeName);
            UIPanelType = Type.GetType(UIPanelTypeName);
            UIButtonType = Type.GetType(UIButtonName);
            UIToggleType = Type.GetType(UIToggleName);
            UIEventListenerType = Type.GetType(UIEventListenerName);
            UIWidgetType = Type.GetType(UIWidgetTypeName);
            UIRectType = Type.GetType(UIRectTypeName);
            UILabelType = Type.GetType(UILabelTypeName);
            UIInputType = Type.GetType(UIInputTypeName);
            UITextureType = Type.GetType(UITextureTypeName);
            UISpriteType = Type.GetType(UISpriteTypeName);

            UICameraNotify = UICameraType.GetMethod("Notify");
        }

        public void Run()
        {
            mRunning = true;
            for (int i = 0; i < 5; i++)
            {
                this.server = new AsyncTcpServer(port + i);
                this.server.Encoding = Encoding.UTF8;
                this.server.ClientConnected += new EventHandler<TcpClientConnectedEventArgs>(server_ClientConnected);
                this.server.ClientDisconnected += new EventHandler<TcpClientDisconnectedEventArgs>(server_ClientDisconnected);
                this.server.DatagramReceived += new EventHandler<TcpDatagramReceivedEventArgs<byte[]>>(server_Received);
                try
                {
                    this.server.Start();
                    Debug.Log(string.Format("Tcp server started and listening at {0}", server.Port));
                    break;
                }
                catch (SocketException e)
                {
                    Debug.Log(string.Format("Tcp server bind to port {0} Failed!", server.Port));
                    Debug.Log("--- Failed Trace Begin ---");
                    Debug.LogError(e);
                    Debug.Log("--- Failed Trace End ---");
                    // try next available port
                    this.server = null;
                }
            }
            if (this.server == null)
            {
                Debug.LogError(string.Format("Unable to find an unused port from {0} to {1}", port, port + 5));
            }
        }

        static void server_ClientConnected(object sender, TcpClientConnectedEventArgs e)
        {
            Debug.Log(string.Format("TCP client {0} has connected.",
                e.TcpClient.Client.RemoteEndPoint.ToString()));
        }

        static void server_ClientDisconnected(object sender, TcpClientDisconnectedEventArgs e)
        {
            Debug.Log(string.Format("TCP client {0} has disconnected.",
               e.TcpClient.Client.RemoteEndPoint.ToString()));
        }

        private void server_Received(object sender, TcpDatagramReceivedEventArgs<byte[]> e)
        {
            //Debug.Log(string.Format("Client : {0} --> {1}",
            //    e.Client.TcpClient.Client.RemoteEndPoint.ToString(), e.Datagram.Length));
            //TcpClientState internalClient = e.Client;
            //string tcpClientKey = internalClient.TcpClient.Client.RemoteEndPoint.ToString();
            //inbox.AddOrUpdate(tcpClientKey, internalClient, (n, o) =>
            //{
            //    return internalClient;
            //});
            //client = new KeyValuePair<string,TcpClientState>(tcpClientKey, internalClient);
            client = e.Client;
            requestFlag = true;
        }


        public void stopListening()
        {
            mRunning = false;
            if (server != null)
                server.Stop();
        }

        private object GetSDKVersion(string[] param)
        {
            return "1.0.11.3";
        }

        private object enableLogging(string[] args)
        {
            return "200";
        }

        private object closeHandler(string[] args)
        {
            //stopListening();
            this.server.Start();
            return null;
        }

        /// <summary>
        /// 寻找物体接口
        /// </summary>
        /// <param name="pieces">pieces[1]是路径 pieces[2]错误后是否返回层级结构 pieces[3]是否分析出正确的层级结构</param>
        /// <returns>物体id和名称</returns>
        private object findObjectHandler(string[] pieces)
        {
            try
            {

                //Dictionary<string, string> data = new Dictionary<string, string>();
                data.Clear();
                if(pieces[2] == "path")
                {
                    data.Add("name", pieces[1].Replace("//", "/"));
                    target = GameObject.Find(data["name"]);
                    if (target && target.activeInHierarchy)
                    {
                        data.Add("id", target.GetInstanceID().ToString());
                    }
                    else
                    {
                        throw new Exception(Error.NotFoundMessage);
                    }
                }
                else if(pieces[2] == "id")
                {
                    target = FindObjectFromInstanceID(int.Parse(pieces[1])) as GameObject;
                    if (target && target.activeInHierarchy)
                    {
                        data.Add("id", target.GetInstanceID().ToString());
                        data.Add("name", GetGameObjectPath(target));
                    }
                    else
                    {
                        throw new Exception(Error.NotFoundMessage);
                    }
                }
                
                //return JsonConvert.SerializeObject(data, MsgParser.settings);
                return JsonMapper.ToJson(data);
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        class Node
        {
            public string path;

            [NonSerialized]
            public GameObject obj;
        }

        /// <summary>
        /// 获取物体的全部子物体
        /// </summary>
        /// <param name="parent">父物体</param>
        /// <param name="parentPath">父路径</param>
        /// <param name="each">对于每一个子物体执行的行为</param>
        private void FindAllObjectFromParent(GameObject parent,string parentPath,Action<Node> each = null,bool isActive = false)
        {
            if (parent == null || (isActive && !parent.activeInHierarchy)) throw new Exception(Error.NotFoundMessage);
            Node root = new Node()
            {
                path = parentPath,
                obj = parent,
            };
            List<Node> nodes = new List<Node>();
            nodes.Add(root);
            int index = 0;
            Transform p;
            Node now;
            while (index != nodes.Count)
            {
                now = nodes[index];
                p = now.obj.transform;
                foreach (Transform transform in p)
                {
                    Node temp = new Node()
                    {
                        path = now.path + "/" + transform.name,
                        obj = transform.gameObject,
                    };
                    nodes.Add(temp);
                    if(each != null)
                    {
                        each(temp);
                    }
                }
                index++;
            }
        }



        private List<T> FindAllGameObject<T>()
        {
            List<T> gameObjects = new List<T>();
  
            Action<Node> action = n =>
            {
                T t = n.obj.GetComponent<T>();
                if (n.obj != null &&  t!= null)
                {
                    gameObjects.Add(t);
                }
            };
            FindAllGameObject(action);
            return gameObjects;
        }

        private void FindAllGameObject(Action<Node> action)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                foreach (GameObject obj in SceneManager.GetSceneAt(i).GetRootGameObjects())
                {
                    try
                    {
                        FindAllObjectFromParent(obj, GetGameObjectPath(obj), action);
                    }
                    catch
                    {

                    }
                }
            }
            GameObject temp = null;
            try
            {
                temp = new GameObject();
                DontDestroyOnLoad(temp);
                Scene dontDestroyOnLoad = temp.scene;
                DestroyImmediate(temp);
                temp = null;

                foreach(GameObject obj  in dontDestroyOnLoad.GetRootGameObjects())
                {
                    try
                    {
                        FindAllObjectFromParent(obj, GetGameObjectPath(obj), action);
                    }
                    catch
                    {

                    }
                }
            }
            finally
            {
                if (temp != null)
                    DestroyImmediate(temp);
            }

        }

        private object findAllTextHandler(string[] pieces)
        {
            try
            {
                bool getValue = false;
                if(pieces.Length > 2)
                {
                    getValue = true;
                }
                List<Text> texts = FindAllGameObject<Text>();
                dataJson.Clear();
                dataJson.Append("[");
                
                Action<Node> action = (Node node) => {
                    GameObject target = node.obj;
                    string text = (string)ReflectionTool.GetComponentAttribute(target, UILabelType, "text");

                    if(getValue)
                    {
                        dataJson.Append("{\"name\":\"" + GetGameObjectPath(target) + "\",\"id\":\"" + target.GetInstanceID().ToString()
                           + "\",\"value\":\"" + text
                           + "\"},");
                    }
                    else
                    {
                         dataJson.Append("{\"name\":\"" + GetGameObjectPath(target) + "\",\"id\":\"" + target.GetInstanceID().ToString()
                           + "\"},");
                    }
                };
                FindAllGameObject(action);
                
                if (dataJson.Length > 1) dataJson.Remove(dataJson.Length - 1, 1);
                dataJson.Append("]");
                return dataJson;
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }



        //private object findAllText2Handler(string[] pieces)
        //{
        //    try
        //    {
        //        List<Text> texts = GetAllSceneObjectsWithInactive<Text>();
        //        dataJson.Clear();
        //        dataJson.Append("[");
        //        texts.ForEach(t=> 
        //        {
        //            dataJson.Append("{\"name\":\"" + GetGameObjectPath(t.gameObject) + "\",\"id\":\"" + t.gameObject.GetInstanceID().ToString()
        //                   + "\",\"value\":\"" + t.text
        //                   + "\"},");
        //        });
        //        if (dataJson.Length > 1) dataJson.Remove(dataJson.Length - 1, 1);
        //        dataJson.Append("]");
        //        return dataJson;
        //    }
        //    catch (Exception e)
        //    {
        //        return e.ToString();
        //    }
        //}

        //private List<GameObject> GetAllSceneObjectsWithInactive(Func<GameObject,bool> func)
        //{
        //    var allTransforms = Resources.FindObjectsOfTypeAll(typeof(Transform));
        //    var previousSelection = UnityEditor.Selection.objects;
        //    UnityEditor.Selection.objects = allTransforms.Cast<Transform>()
        //        .Where(x => x != null)
        //        .Select(x => x.gameObject).Where(func)
        //        //如果你只想获取所有在Hierarchy中被禁用的物体，反注释下面代码
        //        //.Where(x => x != null && !x.activeInHierarchy)
        //        .Cast<UnityEngine.Object>().ToArray();

        //    var selectedTransforms = UnityEditor.Selection.GetTransforms(UnityEditor.SelectionMode.Editable | UnityEditor.SelectionMode.ExcludePrefab);
        //    UnityEditor.Selection.objects = previousSelection;

        //    return selectedTransforms.Select(tr => tr.gameObject).ToList();
        //}

        //private List<T> GetAllSceneObjectsWithInactive<T>()
        //{
        //    List<T> result = new List<T>();
        //    List<GameObject> gameObjects = GetAllSceneObjectsWithInactive(obj => obj.GetComponent<T>() != null);
        //    gameObjects.ForEach(obj => result.Add(obj.GetComponent<T>()));
        //    return result;
        //}

        /// <summary>
        /// 获取物体的全部子孙
        /// </summary>
        /// <param name="pieces"></param>
        /// <returns></returns>
        private object findObjectAllChildrenHandler(string[] pieces)
        {
            try
            {
                string path = pieces[1].Replace("//", "/");
                target = GameObject.Find(path);
                if(target == null) throw new Exception(Error.NotFoundMessage);
                dataJson.Clear();
                dataJson.Append("[");
                //不包含本身
                //dataJson.Append("{\"name\":\"" + target.name + "\",\"id\":\"" + target.GetInstanceID().ToString() + "\"},");
                FindAllObjectFromParent(target,path,(Node temp) =>
                {
                    // RectTransform rect = temp.obj.GetComponent<RectTransform>();
                    // if(rect)
                    // {
                    //     Vector3[] vector3s = GetScreenCoordinates(rect);
                    //     dataJson.Append("{\"name\":\"" + temp.path + "\",\"id\":\"" + temp.obj.GetInstanceID().ToString() 
                    //         + "\",\"center\":\"" + ((vector3s[0] + vector3s[2]) / 2).ToString()
                    //         + "\",\"enabled\":\"" + (temp.obj.activeInHierarchy).ToString()
                    //         + "\"},");
                    // }
                    // else
                    // {
                    dataJson.Append("{\"name\":\"" + temp.path + "\",\"id\":\"" + temp.obj.GetInstanceID().ToString() 
                        + "\",\"position\":\"" + (temp.obj.transform.position).ToString() 
                        + "\",\"enabled\":\"" + (temp.obj.activeInHierarchy).ToString()
                        + "\"},");
                    // }
                },true);
                if (dataJson.Length > 1) dataJson.Remove(dataJson.Length - 1, 1);
                dataJson.Append("]");
                return dataJson;
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        /// <summary>
        /// 寻找所有内容包含特定字符串的文本
        /// </summary>
        /// <param name="args">args[1]目标字符串</param>
        /// <returns></returns>
        private object findAllObjectWhereTextContainsHandler(string[] args)
        {
            try
            {
                Type selfType = GetType();
                // MethodInfo mi = selfType.GetMethod("FindAllGameObject").MakeGenericMethod(UILabelType);
                
                // string text = args[1];
                List<Transform> obj = FindAllGameObject<Transform>();
                // List<InputField> inputFields = mi.Invoke(this, new object[]{});
                
                
                dataJson.Clear();
                dataJson.Append("[");

                foreach(var i in obj)
                {
                    string text = getText(i.gameObject);
                    if(i.gameObject.activeInHierarchy && text != null && text.Contains(args[1]))
                    {
                        
                        string path = GetGameObjectPath(i.gameObject);
                        dataJson.Append("{\"path\":\"" + path + "\",\"id\":\"" + i.gameObject.GetInstanceID().ToString() + "\"},");
                    }
                }

                // mi = selfType.GetMethod("FindAllGameObject").MakeGenericMethod(UIInputType);
                
                // foreach(var i in (List)(mi.Invoke(this, new object[]{})))
                // {
                //     if(i.gameObject.activeInHierarchy && i.text.Contains(text))
                //     {
                //         string path = GetGameObjectPath(i.gameObject);
                //         dataJson.Append("{\"path\":\"" + path + "\",\"id\":\"" + i.gameObject.GetInstanceID().ToString() + "\"},");
                //     }
                // }

                if (dataJson.Length > 1) dataJson.Remove(dataJson.Length - 1, 1);
                dataJson.Append("]");

                return dataJson.ToString();
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }        

        /*/// <summary>
        /// 获取文本包含目标字符串的全部物体
        /// </summary>
        /// <param name="pieces"></param>
        /// <returns></returns>
        private object findAllObjectWhereTextContainsHandler(string[] pieces)
        {
            try
            {
                string path = pieces[1].Replace("//", "/");
                target = GameObject.Find(path);
                if (target == null) throw new Exception(Error.NotFoundMessage);
                dataJson.Clear();
                dataJson.Append("[");
                //不包含本身
                //dataJson.Append("{\"name\":\"" + target.name + "\",\"id\":\"" + target.GetInstanceID().ToString() + "\"},");
                FindAllObjectFromParent(target, path, (Node temp) =>
                {

                    string text = getText(temp.obj);

                    if (text != null && text.Contains(pieces[2]))
                    {
                        dataJson.Append("{\"name\":\"" + temp.path + "\",\"id\":\"" + temp.obj.GetInstanceID().ToString()
                            + "\",\"enabled\":\"" + (temp.obj.activeInHierarchy).ToString()
                            + "\"},");
                    }
                },true);
                if(dataJson.Length > 1) dataJson.Remove(dataJson.Length - 1, 1);
                dataJson.Append("]");
                return dataJson;
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }*/

        /// <summary>
        /// 根据路径名称一层一层得寻找对应物体，该方法为了解决物体名称出现“\”,而导致了寻找物体失败的问题
        /// </summary>
        /// <param name="pieces">pieces[1]是路径</param>
        /// <returns>物体id和名称</returns>
        private object findObjectByLevelHandler(string[] pieces)
        {
            try
            {
                //Dictionary<string, string> data = new Dictionary<string, string>();
                //target = GameObject.Find(data["name"]);
                string[] objNames = pieces[1].Split(new string[] { "//" }, StringSplitOptions.None);
                if (objNames == null) throw new Exception(Error.NotFoundMessage);
                target = GameObject.Find(objNames[1]);
                for (int i = 2; i < objNames.Length; i++)
                {
                    if (target && target.activeInHierarchy)
                    {
                        if (objNames[i].Contains("/"))
                        {
                            foreach (Transform temp in target.transform)
                            {
                                if (temp.name == objNames[i])
                                {
                                    target = temp.gameObject;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            target = target.transform.Find(objNames[i]).gameObject;
                        }
                    }
                    else
                    {
                        throw new Exception(Error.NotFoundMessage);
                    }
                }
                data.Clear();
                data.Add("name", pieces[1]);
                data.Add("id", target.GetInstanceID().ToString());
                //return JsonConvert.SerializeObject(data, MsgParser.settings);
                return JsonMapper.ToJson(data);
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }
        /// <summary>
        /// 寻找物体的子物体
        /// </summary>
        /// <param name="pieces"></param>
        /// <returns>子物体的列表</returns>
        private object findChildHandler(string[] pieces)
        {
            try
            {
                target = GameObject.Find(pieces[1].Replace("//", "/"));
                if (target == null || !target.activeInHierarchy) throw new Exception(Error.NotFoundMessage);
                //List<Dictionary<string, string>> data = new List<Dictionary<string, string>>();
                //dataList.Clear();
                dataJson.Clear();
                dataJson.Append("[");
                for (int i = 0; i < target.transform.childCount; i++)
                {
                    Transform iterTransform = target.transform.GetChild(i);
                    if (iterTransform.gameObject.activeInHierarchy)
                    {
                        //Dictionary<string, string> dictionary = new Dictionary<string, string>()
                        //{
                        //    { "name", iterTransform.name},
                        //    { "id",iterTransform.gameObject.GetInstanceID().ToString()}
                        //};
                        //dataJson.Append("{");
                        //dataJson.Append("\"" + "name" + "\"" + ":" + "\"" + iterTransform.name + "\"");
                        //dataJson.Append(",");
                        //dataJson.Append("\"" + "id" + "\"" + ":" + "\"" + iterTransform.gameObject.GetInstanceID().ToString() + "\"");
                        //dataJson.Append("}");
                        //dataJson.Append(",");
                        dataJson.Append("{\"name\":\"" + iterTransform.name + "\",\"id\":\"" + iterTransform.gameObject.GetInstanceID().ToString() + "\"},");
                        //dataList.Add(dataJson.ToString());
                    }
                }
                if (dataJson.Length > 1) dataJson.Remove(dataJson.Length - 1, 1);
                dataJson.Append("]");
                return dataJson.ToString();
                //return JsonConvert.SerializeObject(data, MsgParser.settings);
                //return JsonMapper.ToJson(data);
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }
        /// <summary>
        /// 根据路径名称寻找物体并点击该物体
        /// </summary>
        /// <param name="pieces"></param>
        /// <returns>物体id和名称</returns>
        private object findObjectAndTapHandler(string[] pieces)
        {
            try
            {
                //Dictionary<string, string> data = new Dictionary<string, string>();
                data.Clear();
                data.Add("name", pieces[1].Replace("//", "/"));
                target = GameObject.Find(data["name"]);
                if (target && target.activeInHierarchy)
                {
                    data.Add("id", target.GetInstanceID().ToString());
                    
                    UICameraNotify.Invoke(null, new object[] { target, "OnClick", null });


                    //return JsonConvert.SerializeObject(data, MsgParser.settings);
                    return JsonMapper.ToJson(data);
                }
                else
                {
                    throw new Exception(Error.NotFoundMessage);
                }
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }
        /// <summary>
        /// 寻找全部名称包含目标字符串的物体
        /// </summary>
        /// <param name="pieces">pieces[1]是目标字符串</param>
        /// <returns>符合条件的物体列表</returns>
        private object findObjectsWhereNameContainsHandler(string[] pieces)
        {
            try
            {
                string partName = pieces[1];
                if (partName == "") throw new Exception(Error.NotFoundMessage);
                //List<Dictionary<string, string>> datas = new List<Dictionary<string, string>>();
                //dataList.Clear();
                dataJson.Clear();
                dataJson.Append("[");
                foreach (GameObject iterObj in FindObjectsOfType<GameObject>())
                {
                    if (iterObj.name.Contains(partName) && iterObj.activeInHierarchy)
                    {
                        //Dictionary<string, string> dictionary = new Dictionary<string, string>() 
                        //{
                        //    { "name", iterObj.name },
                        //    { "id", iterObj.GetInstanceID().ToString() }
                        //};
                        //dataJson.Append("{");
                        //dataJson.Append("\"" + "name" + "\""+ ":" + "\"" +iterObj.name + "\"");
                        //dataJson.Append(",");
                        //dataJson.Append("\"" + "id" + "\"" + ":" + "\"" + iterObj.GetInstanceID().ToString() + "\"");
                        //dataJson.Append("}");
                        //dataJson.Append(",");
                        dataJson.Append("{\"name\":\"" + iterObj.name + "\",\"id\":\"" + iterObj.GetInstanceID().ToString() + "\"},");
                        //dataJson.Append("{'name':'" + iterObj.name + "','id':'" + iterObj.GetInstanceID().ToString() + "'},");
                        //dataList.Add(dataJson.ToString());
                    }
                }
                if (dataJson.Length > 1) dataJson.Remove(dataJson.Length - 1, 1);
                dataJson.Append("]");
                return dataJson.ToString();
                //return JsonMapper.ToJson(dataList);
                //return JsonConvert.SerializeObject(data, MsgParser.settings);
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }
        /// <summary>
        /// 在物体的子物体列表中，寻找名称包含目标字符的物体
        /// </summary>
        /// <param name="pieces"></param>
        /// <returns>符合条件的物体列表</returns>
        private object findObjectInRangeWhereNameContainsHandler(string[] pieces)
        {
            try
            {
                string partName = pieces[1];
                string rangePathName = pieces[2];
                target = GameObject.Find(pieces[2].Replace("//", "/"));
                if (target == null || !target.activeInHierarchy) throw new Exception(Error.NotFoundMessage);
                //List<Dictionary<string, string>> data = new List<Dictionary<string, string>>();
                //dataList.Clear();
                dataJson.Clear();
                dataJson.Append("[");
                for (int i = 0; i < target.transform.childCount; i++)
                {
                    Transform iterTransform = target.transform.GetChild(i);
                    if (iterTransform.name.Contains(partName) && iterTransform.gameObject.activeInHierarchy)
                    {
                        //Dictionary<string, string> dictionary = new Dictionary<string, string>()
                        //{
                        //    { "name", iterTransform.name},
                        //    { "id",iterTransform.gameObject.GetInstanceID().ToString()}
                        //};
                        //dataJson.Append("{");
                        //dataJson.Append("\"" + "name" + "\"" + ":" + "\"" + iterTransform.name + "\"");
                        //dataJson.Append(",");
                        //dataJson.Append("\"" + "id" + "\"" + ":" + "\"" + iterTransform.gameObject.GetInstanceID().ToString() + "\"");
                        //dataJson.Append("}");
                        //dataJson.Append(",");
                        dataJson.Append("{\"name\":\"" + iterTransform.name + "\",\"id\":\"" + iterTransform.gameObject.GetInstanceID().ToString() + "\"},");
                        //dataList.Add(dataJson.ToString());
                    }
                }
                if (dataJson.Length > 1) dataJson.Remove(dataJson.Length - 1, 1);
                dataJson.Append("]");
                return dataJson.ToString();
                //return JsonConvert.SerializeObject(data, MsgParser.settings);
                //return JsonMapper.ToJson(data);
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        private string getText(GameObject obj)
        {
            // UILabel
            string text = (string)ReflectionTool.GetComponentAttribute(obj, UILabelType, "text");
            if (text != null)
                return text;
            
            // UIInput
            text = (string)ReflectionTool.GetComponentAttribute(obj, UIInputType, "value");
            if (text != null)
                return text;

            //NGUI 3.0
            text = (string)ReflectionTool.GetComponentAttribute(obj, UIInputType, "text");
            if (text != null)
                return text;
            
            return null;
        }

        /// <summary>
        /// 获取物体上的Text组件或者InputField组件上的text
        /// </summary>
        /// <param name="args">arg[1]物体路径</param>
        /// <returns>组件上的text属性</returns>
        private object getTextHandler(string[] args)
        {
            try
            {
                //var targetInfo = JsonConvert.DeserializeObject<Dictionary<string, string>>(args[1]);
                data = JsonMapper.ToObject<Dictionary<string, string>>(args[1]);

                //UnityEngine.GameObject target = FindObjectFromInstanceID(int.Parse(data["id"])) as GameObject;
                target = FindObjectFromInstanceID(int.Parse(data["id"])) as GameObject;
                //Debug.LogFormat("target: " + target);

                if (target != null && target.activeInHierarchy)
                {
                    return getText(target);
                }
                throw new Exception(Error.NotFoundMessage);
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }
        /// <summary>
        /// 设置物体组件上的InputField的text
        /// </summary>
        /// <param name="args">arg[1]物体路径</param>
        /// <returns>物体的id和名称</returns>
        private object setTextHandler(string[] args)
        {
            try
            {

                //var targetInfo = JsonConvert.DeserializeObject<Dictionary<string, string>>(args[2]);
                data = JsonMapper.ToObject<Dictionary<string, string>>(args[1]);

                target = FindObjectFromInstanceID(int.Parse(data["id"])) as GameObject;
                //Debug.LogFormat("target: " + target);

                if (target != null && target.activeInHierarchy)
                {

                    if (ReflectionTool.SetComponentAttribute(target, UIInputType, "value", args[2]))
                        return JsonMapper.ToJson(data);

                    //NGUI 3.0
                    if (ReflectionTool.SetComponentAttribute(target, UIInputType, "text", args[2]))
                        return JsonMapper.ToJson(data);

                }
                throw new Exception(Error.NotFoundMessage);
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        public static UnityEngine.Object FindObjectFromInstanceID(int iid)
        {
            return (UnityEngine.Object)typeof(UnityEngine.Object)
                    .GetMethod("FindObjectFromInstanceID", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                    .Invoke(null, new object[] { iid });

        }
        /// <summary>
        /// 点击物体
        /// </summary>
        /// <param name="args">arg[1]物体路径</param>
        /// <returns>物体名称和id</returns>
        private object tapObjectHandler(string[] args)
        {
            try
            {
                //var targetInfo = JsonConvert.DeserializeObject<Dictionary<string, string>>(args[1]);
                data = JsonMapper.ToObject<Dictionary<string, string>>(args[1]);
                target = FindObjectFromInstanceID(int.Parse(data["id"])) as GameObject;
                if (target != null && target.activeInHierarchy)
                {
                    UICameraNotify.Invoke(null, new object[] { target, "OnClick", null });

                    //return JsonConvert.SerializeObject(targetInfo, MsgParser.settings);
                    return JsonMapper.ToJson(data);
                }
                throw new Exception(Error.NotFoundMessage);
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        /// <summary>
        /// 判断物体是否存在
        /// </summary>
        /// <param name="pieces">arg[1]物体路径</param>
        /// <returns>存在返回1，不存在返回0</returns>
        private object objectExistHandler(string[] pieces)
        {
            try
            {
                //Dictionary<string, string> data = new Dictionary<string, string>();
                target = GameObject.Find(pieces[1].Replace("//", "/"));
                if (target && target.activeInHierarchy)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
                //return JsonConvert.SerializeObject(data, MsgParser.settings);

            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }
        /// <summary>
        /// 获取屏幕宽高
        /// </summary>
        /// <param name="pieces"></param>
        /// <returns>屏幕的宽高</returns>
        private object getScreenHandler(string[] pieces)
        {
            try
            {
                data.Clear();
                data.Add("height", Screen.height.ToString());
                data.Add("width", Screen.width.ToString());
                return JsonMapper.ToJson(data);
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        /// <summary>
        /// 获取RectTransform的四个点的屏幕坐标
        /// </summary>
        /// <param name="args">arg[1]物体路径</param>
        /// <returns>物体的中心点和四个顶点的屏幕坐标</returns>
        private object getRectTransformPointsHandler(string[] args)
        {
            try
            {
                string path = args[1].Replace("//", "/");
                target = GameObject.Find(path);
                if(target != null && target.activeInHierarchy)
                {
                    RectTransform rectTransform = target.GetComponent<RectTransform>();
                    if(rectTransform)
                    {
                        data.Clear();
                        Vector3[] targetPoint = GetScreenCoordinates(rectTransform);
                        data.Add("Center", ((targetPoint[0] + targetPoint[2])/2).ToString());
                        data.Add("BottomLeft", targetPoint[0].ToString());
                        data.Add("TopLeft", targetPoint[1].ToString());
                        data.Add("TopRight", targetPoint[2].ToString());
                        data.Add("BottomRight", targetPoint[3].ToString());
                    }
                    else
                    {
                        data.Add("Center", target.transform.position.ToString());
                    }
                    return JsonMapper.ToJson(data);
                }
                throw new Exception(Error.NotFoundMessage);
            }
            catch(Exception e)
            {
                return e.ToString();
            }
        }

        /// <summary>
        /// 获取RectTransform屏幕空间的下的四个点,顺序：左下、左上、右上、右下 
        /// </summary>
        /// <param name="uiElement">目标RectTransform</param>
        /// <returns></returns>
        private Vector3[] GetScreenCoordinates(RectTransform uiElement)
        {
            var worldCorners = new Vector3[4];
            var screenCorners = new Vector3[4];
            uiElement.GetWorldCorners(worldCorners);
            Canvas canvas = uiElement.GetComponentInParent<Canvas>();
            if (canvas == null) canvas = uiElement.GetComponent<Canvas>();
            if (canvas == null) return worldCorners;
            Camera camera;
            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                camera = canvas.worldCamera;
            else
                camera = Camera.main;
            if (camera != null && camera != Camera.main)
            {
                for (int i = 0; i < 4; i++)
                {
                    screenCorners[i] = RectTransformUtility.WorldToScreenPoint(camera, worldCorners[i]);
                }
                return screenCorners;
            }
            return worldCorners;
        }

        /// <summary>
        /// 根据物体的组件名称和值的名称返回值（Json形式）
        /// </summary>
        /// <param name="args">args[1]代表物体路径，args[2]代表组件类型名称，arg[3]代表值名称</param>
        /// <returns></returns>
        private object getValueOnComponentHandler(string[] args)
        {
            try
            {
                string path = args[1].Replace("//", "/");
                string typeName = args[2];
                string valueName = args[3];
                object value = null;
                target = GameObject.Find(path);
                if(target != null && target.activeInHierarchy)
                {
                    value = ReflectionTool.GetComponentAttribute(target, Type.GetType(typeName), valueName);
                    return value.ToString();

                }
                throw new Exception(Error.NotFoundMessage);
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }
        /// <summary>
        /// 寻找物体的孙级列表中Text组件的text属性包含目标字符串的物体
        /// </summary>
        /// <param name="args">args[1]孙级物体名称，arg[2]目标字符串，arg[3]物体路径</param>
        /// <returns></returns>
        private object findObjectInRangeWhereTextContainsHandler(string[] args)
        {
            try
            {

                string partPath = args[1].Replace("//", "/");
                string text = args[2];
                string rangePath = args[3].Replace("//", "/");

                target = GameObject.Find(rangePath);
                if (target != null && target.activeInHierarchy)
                {
                    data.Clear();
                    for (int i = 0; i < target.transform.childCount; ++i)
                    {
                        Transform child = target.transform.GetChild(i);
                        if (!child.gameObject.activeInHierarchy) continue;
                        Transform result = null;
                        if (partPath != "")
                        {
                            result = child.Find(partPath);
                        }
                        else
                        {
                            result = child;
                        }
                        if (result != null && result.gameObject.activeInHierarchy)
                        {

                            string labelText = (string)ReflectionTool.GetComponentAttribute(target, UILabelType, "text");
                            if (labelText != null && labelText.Contains(text))
                            {

                                data.Add("name", child.name);
                                data.Add("id", child.gameObject.GetInstanceID().ToString());
                                return JsonMapper.ToJson(data);



                                
                            }
                        }
                    }
                }
                throw new Exception(Error.NotFoundMessage);
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }
        /// <summary>
        /// 点击屏幕
        /// </summary>
        /// <param name="pieces">pieces[1]屏幕x坐标，pieces[2]屏幕y坐标</param>
        /// <returns>点击中的物体id和名称</returns>
        private object tapScreenHandler(string[] pieces)
        {
            try
            {
                MethodInfo raycast = UICameraType.GetMethod("Raycast", new Type[] {typeof(Vector3)});
                raycast.Invoke(null, new object[] {new Vector3(float.Parse(pieces[1]), float.Parse(pieces[2]))});
                FieldInfo fi = UICameraType.GetField("mRayHitObject", BindingFlags.Static | BindingFlags.NonPublic);
                GameObject gameObject = (GameObject) fi.GetValue(null);
                
                if (gameObject != null)
                {
                    UICameraNotify.Invoke(null, new object[] { gameObject, "OnClick", null });
                }
                else
                {
                    throw new Exception(Error.NotFoundMessage);
                }

                data.Clear();
                data.Add("name", GetGameObjectPath(gameObject));
                data.Add("id", gameObject.GetInstanceID().ToString());
                return JsonMapper.ToJson(data);
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        /// <summary>
        /// 通过点投射获得物体,目前只对可互动的UI有效？
        /// </summary>
        /// <param name="pieces">pieces[1]屏幕x坐标，pieces[2]屏幕y坐标</param>
        /// <returns>被中心投射的物体id和名称</returns>
        private object findObjectByPointHandler(string[] pieces)
        {
            try
            {
                MethodInfo raycast = UICameraType.GetMethod("Raycast", new Type[] {typeof(Vector3)});
                raycast.Invoke(null, new object[] {new Vector3(float.Parse(pieces[1]), float.Parse(pieces[2]))});
                FieldInfo fi = UICameraType.GetField("mRayHitObject", BindingFlags.Static | BindingFlags.NonPublic);
                GameObject gameObject = (GameObject) fi.GetValue(null);

                if (gameObject != null)
                {
                    data.Clear();
                    data.Add("name", GetGameObjectPath(gameObject));
                    data.Add("id", gameObject.GetInstanceID().ToString());
                }
                else
                {
                    throw new Exception(Error.NotFoundMessage);
                }

               
                return JsonMapper.ToJson(data);
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        private void BeginSample(string fileName)
        {
            Profiler.SetAreaEnabled(0, true);
            Profiler.SetAreaEnabled((ProfilerArea)2, true);
            Profiler.SetAreaEnabled((ProfilerArea)1, true);
            Profiler.SetAreaEnabled((ProfilerArea)3, true);
            Profiler.SetAreaEnabled((ProfilerArea)6, true);
            Profiler.SetAreaEnabled((ProfilerArea)10, true);
            Profiler.SetAreaEnabled((ProfilerArea)11, true);
            Profiler.SetAreaEnabled((ProfilerArea)12, true);
            Profiler.logFile = Application.persistentDataPath + "/" + fileName;
            Profiler.enableBinaryLog = true;
            Profiler.enabled = true;
            //标记data文件最大使用1GB储存空间
            Profiler.maxUsedMemory = 1024 * 1024 * 1024;

        }

        private void EndSample()
        {
            Profiler.enabled = false;
            Profiler.logFile = "";
            Profiler.enableBinaryLog = false;
        }

        private void ProfilerInit()
        {
            frameNum = 0;
            fileNum = 0;
            isRecording = false;
        }
        bool startSample = false, isRecording = false;
        int frameNum = 0;
        int fileNum = 0;
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
        private IEnumerator startRecordProfileIEnumerator = null;
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
                    }
                    else
                    {
                        response = "It hasn't started yet";
                    }
                }
            }
            catch (Exception e)
            {
                response = e.ToString();
            }
            return response;
        }



        /// <summary>
        /// drag object to aim pos
        /// </summary>
        /// <param name="args">args[1] object path,args[2] aim pos x,args[3] aim pos y</param>
        /// <returns></returns>
        private object dragObjectHandler(string[] args)
        {
            try
            {
                //先将物体的位置转换为屏幕空间的坐标
                //目标位置减去当前位置，也就是产生一条从当前位置指向目标位置的向量
                //创建一个PointerEventData模拟拖动事件，位置是目标位置，delta设置为当前位置指向目标位置的向量
                //执行ExecuteEvents的Execute方法，执行拖动事件
                //从当前位置拖动到目标位置（屏幕空间）
                string path = args[1].Replace("//","/");
                target = GameObject.Find(path);
                if (target != null && target.activeInHierarchy)
                {
                    RectTransform rectTransform = target.GetComponent<RectTransform>();
                    if(rectTransform)
                    {
                        Vector2 direction;
                        Vector2 aimPos;
                        Vector2 startPos;
                        if (args.Length == 4)
                        {
                            Vector3[] targetPoint = GetScreenCoordinates(rectTransform);
                            startPos = (targetPoint[0] + targetPoint[2]) / 2;
                            aimPos = new Vector2((float)Convert.ToDouble(args[2]), (float)Convert.ToDouble(args[3]));
                        }
                        else
                        {
                            startPos = new Vector2((float)Convert.ToDouble(args[2]), (float)Convert.ToDouble(args[3]));
                            aimPos = new Vector2((float)Convert.ToDouble(args[4]), (float)Convert.ToDouble(args[5]));
                        }
                        direction = aimPos - startPos;
                        var pointerEventData = MockUpPointerInputModule.GetPointerEventData(new UnityEngine.Touch() { position = aimPos });
                        pointerEventData.delta = direction;
                        ExecuteEvents.Execute(target, pointerEventData, ExecuteEvents.dragHandler);

                        data.Clear();
                        data.Add("name", path);
                        data.Add("id", target.GetInstanceID().ToString());
                        return JsonMapper.ToJson(data);
                    }
                }

                throw new Exception(Error.NotFoundMessage);
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        private object getPNGScreenshotHandler(string[] args)
        {
            try
            {
                StartCoroutine(GetPNGScreenshot());
                return "Ok";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        private System.Collections.IEnumerator GetPNGScreenshot()
        {
            yield return new WaitForEndOfFrame();
            Texture2D screenshot = UnityEngine.ScreenCapture.CaptureScreenshotAsTexture();
            byte[] bytesPNG = UnityEngine.ImageConversion.EncodeToPNG(screenshot);
            string pngAsString = Convert.ToBase64String(bytesPNG);
            server.Send(client.TcpClient, prot.pack(pngAsString));
        }

        private object objectFindHandler(string[] args)
        {
            try
            {
                data = JsonMapper.ToObject<Dictionary<string, string>>(args[1]);
                target = FindObjectFromInstanceID(int.Parse(data["id"])) as GameObject;
                data.Clear();
                string value = args[3].Replace("//", "/");

                // find by child name
                if(args[2] == "name")
                {
                    if(value[0] == '/')
                    {
                        value = value.Remove(0);
                    }
                    string[] names = value.Split('/');
                    Transform parent = target.transform;
                    foreach(string name in names)
                    {
                        bool flag = false;
                        Debug.Log(name);

                        foreach(Transform child in parent)
                        {
                            // get first found child
                            if(child.name == name && child.gameObject.activeInHierarchy)
                            {
                                parent = child;
                                Debug.Log("found");
                                flag = true;
                                break;
                            }
                        }

                        if(flag == false)
                            break;
                    }

                    if(parent.name == names[names.Length - 1])
                    {
                        // found
                        data.Add("id", parent.gameObject.GetInstanceID().ToString());
                        data.Add("name", GetGameObjectPath(parent.gameObject));
                    }
                    else
                    {
                        throw new Exception(Error.NotFoundMessage);
                    }
                }
                // find by child index
                else if(args[2] == "index")
                {
                    int index = int.Parse(value);
                    if(index < 0 || index >= target.transform.childCount)
                    {
                        throw new Exception("Index Out Of Range");
                    }

                    GameObject child = target.transform.GetChild(index).gameObject;
                    if(child.activeInHierarchy)
                    {
                        data.Add("id", child.GetInstanceID().ToString());
                        data.Add("name", GetGameObjectPath(child));
                    }
                    else
                    {
                        throw new Exception(Error.NotFoundMessage);
                    }
                }

                return JsonMapper.ToJson(data);
            }
            catch(Exception e)
            {

                return e.ToString();
            }
            

        }

        private object getParentHandler(string[] args)
        {
            try
            {
                data = JsonMapper.ToObject<Dictionary<string, string>>(args[1]);
                target = FindObjectFromInstanceID(int.Parse(data["id"])) as GameObject;
                data.Clear();
                if(target != null && target.activeInHierarchy)
                {

                    GameObject parent = target.transform.parent.gameObject;
                    data.Add("id", parent.GetInstanceID().ToString());
                    data.Add("name", GetGameObjectPath(parent));
                }
                else
                {
                    throw new Exception(Error.NotFoundMessage);
                }

                return JsonMapper.ToJson(data);
            }
            catch(Exception e)
            {
                return e.ToString();
            }
        }

        private object resumeDebugModeHandler(string[] args)
        {
            pauseDebugMode = false;
            return "200";
        }

        private object pauseDebugModeHandler(string[] args)
        {
            pauseDebugMode = true;
            return "200";
        }

        private object stopDebugModeHandler(string[] args)
        {
            try
            {
                if (DebugModeEnumerator != null)
                {
                    StopCoroutine(DebugModeEnumerator);
                    DebugModeEnumerator = null;
                }

                return "Close Debug Mode";
            }
            catch(Exception e)
            {
                return e.ToString();
            }
        }

        private object debugModeHandler(string[] args)
        {
            try
            {
                pauseDebugMode = false;
                data.Clear();

                if(DebugModeEnumerator != null)
                {
                    StopCoroutine(DebugModeEnumerator);
                    DebugModeEnumerator = null;
                }
                
                DebugModeEnumerator = DebugMode();
                StartCoroutine(DebugModeEnumerator);
                return "Open Debug Mode";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        private System.Collections.IEnumerator DebugMode()
        {
            GameObject lastSelectedGameObject = null,flagGameObject = null;
            //int quitFlag = 0;
            float quitTime = 0,maxQuitTime = 5;
            UICameraType.GetProperty("selectedObject").SetValue(null, null);
            //尚未记录操作时间
            float lastTime = Time.unscaledTime,nowTime = Time.unscaledTime;
            while (true)
            {

                if (client == null || client.TcpClient == null || !client.TcpClient.Connected)
                {
                    StopCoroutine(DebugModeEnumerator);
                    DebugModeEnumerator = null;
                    yield break;
                }
                
                // 暂停录制
                if (!pauseDebugMode)
                {
                    //按住5秒就关闭调试模式
                    if(Input.GetMouseButton(0))
                    {
                        quitTime += Time.unscaledDeltaTime;
                    }
                    else
                    {
                        quitTime = 0;
                    }

                    if (quitTime >= maxQuitTime)
                    {
                        server.Send(client.TcpClient, prot.pack("Close Debug Mode"));
                        StopCoroutine(DebugModeEnumerator);
                        DebugModeEnumerator = null;
                        yield break;
                    }

                    try
                    {

                        GameObject currentSelectedGameObject = (GameObject) UICameraType.GetProperty("selectedObject").GetValue(null);
                        // Debug.Log("selectedObject: " + GetGameObjectPath(currentSelectedGameObject));

                        if(Input.GetMouseButtonDown(0))
                        {
                            Vector2 pos = Input.mousePosition;
                            data.Add("press position", pos.ToString());
                        }

                        //当选中物体时
                        if (flagGameObject != currentSelectedGameObject)
                        {
                            lastTime = nowTime;
                            nowTime = Time.unscaledTime;
                            flagGameObject = currentSelectedGameObject;
                            if (lastSelectedGameObject == flagGameObject)
                            {
                                //重复选中物体时
                                //quitFlag++;
                            }
                            else
                            {
                                //当切换选中物体时
                                if(lastSelectedGameObject != null)
                                {


                                    // UIInput
                                    string text = (string)ReflectionTool.GetComponentAttribute(lastSelectedGameObject, UIInputType, "value");
                                    if (text != null)
                                    {
                                        // data.Clear();
                                        data.Add("name", GetGameObjectPath(lastSelectedGameObject));
                                        data.Add("value", text);
                                        data.Add("time", (nowTime - lastTime).ToString());
                                        server.Send(client.TcpClient, prot.pack(JsonMapper.ToJson(data)));
                                        data.Clear();
                                    }
                                    else
                                    {
                                        //NGUI 3.0
                                        text = (string)ReflectionTool.GetComponentAttribute(lastSelectedGameObject, UIInputType, "text");
                                        if (text != null)
                                        {
                                            // data.Clear();
                                            data.Add("name", GetGameObjectPath(lastSelectedGameObject));
                                            data.Add("value", text);
                                            data.Add("time", (nowTime - lastTime).ToString());
                                            server.Send(client.TcpClient, prot.pack(JsonMapper.ToJson(data)));
                                            data.Clear();
                                        }
                                    }

                                }
                                lastSelectedGameObject = flagGameObject;
                                //quitFlag = 0;
                            }
                            if (client.TcpClient.Connected && flagGameObject != null)
                            {
                                // data.Clear();
                                data.Add("name", GetGameObjectPath(flagGameObject));
                                data.Add("time", (nowTime - lastTime).ToString());
                                server.Send(client.TcpClient, prot.pack(JsonMapper.ToJson(data)));
                                data.Clear();
                            }

                            if (flagGameObject.GetComponent(UIInputType) == null)
                            {
                                //不将这个置为空点击相同的控件就不会发送数据，但将这个置为空后，会影响ui的使用
                                UICameraType.GetProperty("selectedObject").SetValue(null, null);
                                flagGameObject = null;
                            }
                        }


                        if(data.Count != 0)
                        {
                            server.Send(client.TcpClient, prot.pack(JsonMapper.ToJson(data)));
                            data.Clear();
                        }

                    }
                    catch(Exception e)
                    {
                        server.Send(client.TcpClient, prot.pack(e.ToString()));
                    }
                }

                yield return null;
            }
        }

        private string GetGameObjectPath(GameObject obj)
        {
            if (obj == null) return "null";
            string path = "/" + obj.name;
            Transform parentTransform = obj.transform.parent;
            while (parentTransform != null)
            {
                path = "/" + parentTransform.name + path;
                parentTransform = parentTransform.parent;
            }
            return path;
        }

        // todo: 在获取 Hierarchy 信息的时候，没有将信息保存到本地中，现版本只是简单获取 Hierarchy 信息
        private void WriteJsonData(JsonWriter jw, GameObject go)
        {
            jw.WriteObjectStart();

            jw.WritePropertyName("id");
            jw.Write(go.GetInstanceID());

            jw.WritePropertyName("name");
            jw.Write(go.name);

            GameRunTimeDataSet.AddGameObject(go);

            if(go.transform.childCount > 0)
            {
                jw.WritePropertyName("children");
                jw.WriteArrayStart();

                for(int i = 0; i < go.transform.childCount; ++i)
                {
                    GameObject child = go.transform.GetChild(i).gameObject;
                    WriteJsonData(jw, child);
                }

                jw.WriteArrayEnd();
            }

            jw.WriteObjectEnd();
        }

        // todo：每一次获取 Hierarchy 信息的时候，相当于查询一次，需要重置缓存（目前还没有缓存）
        private object getHierarchyHandler(string[] args)
        {
            try
            {
                GameRunTimeDataSet.InitDataSet();

                JsonWriter jsonWriter = new JsonWriter();
                jsonWriter.WriteObjectStart();
                jsonWriter.WritePropertyName("objs");

                jsonWriter.WriteObjectStart();

                jsonWriter.WritePropertyName("id");
                jsonWriter.Write("root");

                jsonWriter.WritePropertyName("name");
                jsonWriter.Write("root");

                jsonWriter.WritePropertyName("children");

                jsonWriter.WriteArrayStart();

                List<GameObject> _RootGameObjects = new List<GameObject>();
                Transform[] arrTransforms = Transform.FindObjectsOfType<Transform>();
                for (int i = 0; i < arrTransforms.Length; ++i)
                {
                    Transform tran = arrTransforms[i];
                    if (tran.parent == null)
                    {
                        _RootGameObjects.Add(tran.gameObject);
                    }
                }

                for (int i = 0; i < _RootGameObjects.Count; ++i)
                {
                    WriteJsonData(jsonWriter, _RootGameObjects[i]);
                }

                jsonWriter.WriteArrayEnd();

                jsonWriter.WriteObjectEnd();


                jsonWriter.WriteObjectEnd();

                return jsonWriter.ToString();
            }
            catch(Exception e)
            {
                return e.ToString();
            }
        }

        private object getInspectorHandler(string[] args)
        {
            try
            {  
                int objId = int.Parse(args[1]);

                GameObject obj = null;

                if(GameRunTimeDataSet.TryGetGameObject(objId, out obj))
                {
                    JsonWriter jw = new JsonWriter();

                    jw.WriteObjectStart();
                
                    jw.WritePropertyName("name");
                    jw.Write(obj.name);
                
                    jw.WritePropertyName("id");
                    jw.Write(obj.GetInstanceID());
                
                    jw.WritePropertyName("enabled");
                    jw.Write(obj.activeInHierarchy);
                
                    jw.WritePropertyName("tag");
                    jw.Write(obj.tag);
                
                    jw.WritePropertyName("layer");
                    jw.Write(LayerMask.LayerToName(obj.layer));
                
                    jw.WritePropertyName("components");
                
                    jw.WriteArrayStart();
                
                    Component[] components = obj.GetComponents<Component>();
                    for(int j = 0; j < components.Length; ++j)
                    {
                        WriteJsonData(jw, components[j]);
                    }
                
                    jw.WriteArrayEnd();
                
                    jw.WriteObjectEnd();

                    return jw.ToString();
                }
                else
                {
                    throw new Exception(Error.NotFoundMessage);
                }
            }
            catch(Exception e)
            {
                return e.ToString();
            }
        }

        void WriteJsonData(JsonWriter jw, Component component)
        {
            GameRunTimeDataSet.AddComponent(component);

            jw.WriteObjectStart();

            jw.WritePropertyName("id");
            jw.Write(component.GetInstanceID());

            jw.WritePropertyName("type");
            jw.Write(component.GetType().ToString());

            if (ComponentContainProperty(component, "enabled"))
            {
                jw.WritePropertyName("enabled");
                jw.Write(GetComponentValue<bool>(component, "enabled"));
            }

            jw.WritePropertyName("properties");
            jw.WriteArrayStart();

            GetComponentPropertys(component, jw);

            jw.WriteArrayEnd();

            jw.WriteObjectEnd();
        }
        
        private T GetComponentValue<T>(Component component, string propertyName)
        {
            if (component != null && !string.IsNullOrEmpty(propertyName))
            {
                PropertyInfo propertyInfo = component.GetType().GetProperty(propertyName);
                if (propertyInfo != null)
                {
                    MethodInfo mi = propertyInfo.GetGetMethod(true);
                    if (mi != null)
                    {
                        return (T)mi.Invoke(component, null);
                    }

                    //return (T)propertyInfo.GetValue(component, null);
                }
            }
            return default(T);
        }

        private void GetComponentPropertys(Component component, JsonWriter jw)
        {
            try
            {
                PropertyInfo[] propertyInfos = component.GetType().GetProperties(BindingFlags.Public |
                                                                                BindingFlags.Instance |
                                                                                BindingFlags.SetProperty |
                                                                                BindingFlags.GetProperty);

                FieldInfo[] fieldInfos = component.GetType().GetFields(BindingFlags.Public |
                                                                    BindingFlags.Instance |
                                                                    BindingFlags.SetField |
                                                                    BindingFlags.GetField);

                for (int i = 0; i < propertyInfos.Length; ++i)
                {
                    PropertyInfo pi = propertyInfos[i];

                    //Debug.LogError("Property:" + pi.Name);

                    if (pi.CanWrite && pi.CanRead)
                    {
                        // call getter with these Property name will create new object;
                        if (pi.Name == "mesh" || pi.Name == "material" || pi.Name == "materials")
                        {
                            continue;
                        }

                        System.Object obj = pi.GetValue(component, null);
                        if (obj is System.Collections.ICollection)
                        {
                            continue;
                        }

                        if (pi.GetValue(component) != null)
                        {
                            jw.WriteObjectStart();
                            
                            jw.WritePropertyName("name");
                            jw.Write(pi.Name);

                            jw.WritePropertyName("type");
                            jw.Write(pi.GetValue(component).GetType().ToString());

                            jw.WritePropertyName("value");
                            jw.Write(pi.GetValue(component).ToString());
                            
                            jw.WriteObjectEnd();
                        }
                    }
                    else
                    {

                    }
                }

                for (int i = 0; i < fieldInfos.Length; ++i)
                {
                    FieldInfo fi = fieldInfos[i];
                    //Debug.LogError("Field:" + fi.Name);
                    
                    if (fi.GetValue(component) != null)
                    {
                        jw.WriteObjectStart();

                        jw.WritePropertyName("name");
                        jw.Write(fi.Name);

                        jw.WritePropertyName("type");
                        jw.Write(fi.GetValue(component).GetType().ToString());

                        jw.WritePropertyName("value");
                        jw.Write(fi.GetValue(component).ToString());

                        jw.WriteObjectEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
            }
        }
        
        private bool ComponentContainProperty(Component component, string propertyName)
        {
            if (component != null && !string.IsNullOrEmpty(propertyName))
            {
                PropertyInfo _findedPropertyInfo = component.GetType().GetProperty(propertyName);
                return (_findedPropertyInfo != null);
            }
            return false;
        }

        void Update()
        {
            //foreach (TcpClientState client in inbox.Values)
            //{
            //    List<string> msgs = client.Prot.swap_msgs();
            //    msgs.ForEach(delegate (string msg)
            //    {                                       
            //        string response = m_Handlers.HandleMessage(msg);
            //        if (response != null)
            //        {                        
            //            byte[] bytes = prot.pack(response);                        
            //            server.Send(client.TcpClient, bytes);                        
            //            TcpClientState internalClientToBeThrowAway;
            //            string tcpClientKey = client.TcpClient.Client.RemoteEndPoint.ToString();
            //            inbox.TryRemove(tcpClientKey, out internalClientToBeThrowAway);
            //        }
            //    });
            //}
            //if(requestFlag && client.Key != null && client.Value != null && client.Value.TcpClient != null)
            if (requestFlag && client != null)
            {
                //TcpClientState tcpClientState = client.Value;
                //List<string> msgs = client.Prot.swap_msgs();
                if (client.TcpClient.Connected)
                {
                    msgs = client.Prot.swap_msgs();
                    if (msgs != null)
                    {
                        msgs.ForEach(delegate (string msg)
                        {

                            //string response = m_Handlers.HandleMessage(msg);
                            response.Clear();
                            response.Append(m_Handlers.HandleMessage(msg));
                            if (response != null)
                            {
                                //byte[] bytes = prot.pack(response);
                                server.Send(client.TcpClient, prot.pack(response.ToString()));
                                requestFlag = false;
                                //response.Clear();
                            }
                        });
                        //if (responseFlag)
                        //{
                        //    client = new KeyValuePair<string, TcpClientState>();
                        //    responseFlag = false;

                        //}
                    }
                }   
            }

        }

        void OnApplicationQuit()
        {
            // stop listening thread
            stopListening();
        }

        void OnDestroy()
        {
            // stop listening thread
            stopListening();
        }


        public static class GameRunTimeDataSet
        {
            public static void InitDataSet()
            {
                ms_gameObjectDict.Clear();
                ms_componentDict.Clear();
            }

            public static void AddGameObject(GameObject obj)
            {
                int nInstanceID = obj.GetInstanceID();
                if (!ms_gameObjectDict.ContainsKey(nInstanceID))
                {
                    ms_gameObjectDict.Add(nInstanceID, obj);
                }
            }

            public static bool TryGetGameObject(int nInstanceID, out GameObject go)
            {
                return ms_gameObjectDict.TryGetValue(nInstanceID, out go);
            }

            public static void AddComponent(Component comp)
            {
                int nInstanceID = comp.GetInstanceID();
                if (!ms_componentDict.ContainsKey(nInstanceID))
                {
                    ms_componentDict.Add(nInstanceID, comp);
                }
            }

            public static bool TryGetComponent(int nInstanceID, out UnityEngine.Component comp)
            {
                return ms_componentDict.TryGetValue(nInstanceID, out comp);
            }

            public static Dictionary<int, GameObject> ms_gameObjectDict = new Dictionary<int, GameObject>();
            public static Dictionary<int, Component> ms_componentDict = new Dictionary<int, Component>();
        }
    }


    public class MsgParser
    {
        public delegate object RpcMethod(string[] param);

        protected Dictionary<string, RpcMethod> RPCHandler = new Dictionary<string, RpcMethod>();
        //public static JsonSerializerSettings settings = new JsonSerializerSettings()
        //{
        //    StringEscapeHandling = StringEscapeHandling.EscapeNonAscii
        //};
        private string[] args;
        public string HandleMessage(string json)
        {
            args = json.Split(';');

            if (args.Length > 0)
            {
                string method = args[0];
                object result = null;
                try
                {
                    result = RPCHandler[method](args);
                }
                catch (Exception e)
                {
                    // return error response
                    //Debug.Log(e);                    
                    return string.Format("{0} {1}", Error.ExceptionMessage, e.ToString());
                }

                // return result response
                // response = formatResponse(idAction, result);
                if (result == null)
                    return null;
                return result.ToString();
            }
            else
            {
                // do not handle response
                //Debug.Log("ignore message without method");
                return null;
            }
        }
        /*
        public string HandleMessage2(string json)
        {
            Dictionary<string, object> data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json, settings);
            if (data.ContainsKey("method"))
            {
                string method = data["method"].ToString();
                List<object> param = null;
                if (data.ContainsKey("params"))
                {
                    param = ((JArray)(data["params"])).ToObject<List<object>>();
                }

                object idAction = null;
                if (data.ContainsKey("id"))
                {
                    // if it have id, it is a request
                    idAction = data["id"];
                }

                string response = null;
                object result = null;
                try
                {
                    //result = RPCHandler[method](param);
                }
                catch (Exception e)
                {
                    // return error response
                    Debug.Log(e);
                    response = formatResponseError(idAction, null, e);
                    return response;
                }

                // return result response
                response = formatResponse(idAction, result);
                return response;

            }
            else
            {
                // do not handle response
                Debug.Log("ignore message without method");
                return null;
            }
        }
        // Call a method in the server
        public string formatRequest(string method, object idAction, List<object> param = null)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data["jsonrpc"] = "2.0";
            data["method"] = method;
            if (param != null)
            {
                data["params"] = JsonConvert.SerializeObject(param, settings);
            }
            // if idAction is null, it is a notification
            if (idAction != null)
            {
                data["id"] = idAction;
            }
            return JsonConvert.SerializeObject(data, settings);
        }
        

        // Send a response from a request the server made to this client
        public string formatResponse(object idAction, object result)
        {
            Dictionary<string, object> rpc = new Dictionary<string, object>();
            rpc["jsonrpc"] = "2.0";
            rpc["id"] = idAction;
            rpc["result"] = result;
            return JsonConvert.SerializeObject(rpc, settings);
        }

        // Send a error to the server from a request it made to this client
        public string formatResponseError(object idAction, IDictionary<string, object> data, Exception e)
        {
            Dictionary<string, object> rpc = new Dictionary<string, object>();
            rpc["jsonrpc"] = "2.0";
            rpc["id"] = idAction;

            Dictionary<string, object> errorDefinition = new Dictionary<string, object>();
            errorDefinition["code"] = 1;
            errorDefinition["message"] = e.ToString();

            if (data != null)
            {
                errorDefinition["data"] = data;
            }

            rpc["error"] = errorDefinition;
            return JsonConvert.SerializeObject(rpc, settings);
        }
        */
        public void addMsgHandler(string name, RpcMethod method)
        {
            RPCHandler[name] = method;
        }
    }
}


// public static class TypeUtil
// {
//     /// <summary>
//     /// 将任意属性的值转换为字符串
//     /// </summary>
//     /// <param name="property"></param>
//     /// <returns></returns>
//     public static string GetPropertyValue(SerializedProperty property)
//     {
//         switch (property.propertyType)
//         {
//             case SerializedPropertyType.Boolean:
//                 return property.boolValue.ToString();
//             case SerializedPropertyType.Integer:
//                 return property.intValue.ToString();
//             case SerializedPropertyType.Float:
//                 return property.floatValue.ToString();
//             case SerializedPropertyType.String:
//                 return property.stringValue;
//             case SerializedPropertyType.Color:
//                 return property.colorValue.ToString("G");
//             case SerializedPropertyType.Enum:
//                 return property.enumDisplayNames[property.enumValueIndex];
//             case SerializedPropertyType.Vector2:
//                 return property.vector2Value.ToString("G");
//             case SerializedPropertyType.Vector3:
//                 return property.vector3Value.ToString("G");
//             case SerializedPropertyType.Vector4:
//                 return property.vector4Value.ToString("G");
//             case SerializedPropertyType.Quaternion:
//                 return property.quaternionValue.ToString("G");
//             // return property.quaternionValue.eulerAngles.ToString("G");
//             case SerializedPropertyType.Vector2Int:
//                 return property.vector2IntValue.ToString();
//             case SerializedPropertyType.Vector3Int:
//                 return property.vector3IntValue.ToString();
//             case SerializedPropertyType.Rect:
//                 return property.rectValue.ToString("G");
//             case SerializedPropertyType.RectInt:
//                 return property.rectIntValue.ToString();
//             case SerializedPropertyType.ObjectReference:
//                 return $"\"type\": \"{property.objectReferenceValue.GetType()}\", \"name\": \"{property.objectReferenceValue.name}\"";
//                 // return String.Format("\"type\": \"{0}\", \"name\": \"{1}\", \"id\": {2}", property.objectReferenceValue.GetType(), property.objectReferenceValue.name, property.objectReferenceValue.GetInstanceID());
//             case SerializedPropertyType.Bounds:
//                 return property.boundsValue.ToString("G");
//             case SerializedPropertyType.BoundsInt:
//                 return property.boundsIntValue.ToString();
//             case SerializedPropertyType.ExposedReference:
//                 return $"\"type\": \"{property.exposedReferenceValue.GetType()}\", \"name\": \"{property.exposedReferenceValue.name}\", \"id\": \"{property.exposedReferenceValue.GetInstanceID()}\"";
//                 // return String.Format("\"type\": \"{0}\", \"name\": \"{1}\", \"id\": {2}", property.exposedReferenceValue.GetType(), property.exposedReferenceValue.name, property.exposedReferenceValue.GetInstanceID());
//         }

//         return null;
//     }
// }