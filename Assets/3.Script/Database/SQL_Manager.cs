using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MySql.Data.MySqlClient;
using LitJson;

public class JsonItem
{
    public string IP { get; private set; }
    public string TableName { get; private set; }
    public string ID { get; private set; }
    public string PW { get; private set; }
    public string Port { get; private set; }

    public JsonItem(string _ip, string _table, string _id, string _pw, string _port)
    {
        IP = _ip;
        TableName = _table;
        ID = _id;
        PW = _pw;
        Port = _port;
    }
}

public class UserInfo
{
    public string UserId { get; private set; }
    public string UserPassword { get; private set; }
    public string UserNickName { get; private set; }

    public UserInfo(string _name, string _pwd, string _nickName)
    {
        UserId = _name;
        UserPassword = _pwd;
        UserNickName = _nickName;
    }

}

public class SQL_Manager : MonoBehaviour
{
    //[SerializeField]
    private string DB_Path = string.Empty;
    private MySqlConnection con;
    private MySqlDataReader reader;

    public UserInfo info { get; private set; }

    public static SQL_Manager instance = null;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    //DB서버 연결
    private void Start()
    {
        DB_Path = Application.dataPath + "/Database";
        string serverInfo = SetServer(DB_Path);
        try
        {
            if (serverInfo.Equals(string.Empty))
            {
                Debug.Log("Json Error");
                return;
            }
            con = new MySqlConnection(serverInfo);
            con.Open();
            Debug.Log("SQL Server Connect!");
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    //Select문 결과 가지고 오기
    public MySqlDataReader ExecuteQuery(string query, params MySqlParameter[] parameters)
    {
        try
        {
            if (!Connection_Check(con))
            {
                return null;
            }

            MySqlCommand command = new MySqlCommand(query, con);
            command.Parameters.AddRange(parameters);
            return command.ExecuteReader();
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            return null;
        }
    }


    //쿼리문 실행 - Insert, Delete, Update
    public int ExecuteNonQuery(string query, params MySqlParameter[] parameters)
    {
        try
        {
            if (!Connection_Check(con))
            {
                return 0;
            }

            MySqlCommand command = new MySqlCommand(query, con);
            command.Parameters.AddRange(parameters);
            return command.ExecuteNonQuery();
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            return 0;
        }
    }


    //JSON 읽기
    private string SetServer(string path)
    {
        CreateFile(path);

        string Jsonstring = File.ReadAllText(path + "/config.json");
        JsonData itemdata = JsonMapper.ToObject(Jsonstring);

        try
        {
            string ServerInfo =
                $"Server={itemdata[0]["IP"]};" +
                $"Database={itemdata[0]["TableName"]};" +
                $"Uid={itemdata[0]["ID"]};" +
                $"Pwd={itemdata[0]["PW"]};" +
                $"Port={itemdata[0]["Port"]};" +
                "Charset=utf8;";

            return ServerInfo;
        }catch(Exception e)
        {
            Debug.Log(e.Message);
            return string.Empty;
        }
    }

    private void CreateFile(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        string jsonPath = path + "/config.json";
        if (!File.Exists(jsonPath))
        {
            List<JsonItem> items = new List<JsonItem>();
            items.Add(new JsonItem
                ("127.0.0.0", "programming", "root", "250930", "3306"));
            JsonData data = JsonMapper.ToJson(items);
            File.WriteAllText(jsonPath, data.ToString());
        }
    }

    private bool Connection_Check(MySqlConnection con)
    {
        if (con.State != System.Data.ConnectionState.Open)
        {
            con.Open();
            if (con.State != System.Data.ConnectionState.Open)
            {
                return false;
            }
        }
        return true;
    }

}
