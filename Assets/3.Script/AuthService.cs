using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MySql.Data.MySqlClient;

public class AuthService : MonoBehaviour
{
    [SerializeField] private GameObject AuthPanel;

    [SerializeField] private TMP_InputField id_input;
    [SerializeField] private TMP_InputField Pwd_input;
    [SerializeField] private TMP_InputField nickname_input;

    [SerializeField] private Button login_btn;
    [SerializeField] private Button register_btn;

    [SerializeField] private TMP_Text logText;

    [SerializeField] private GameObject anykeyText;

    public UserInfo info { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        LogText_viewing(string.Empty);
        login_btn.onClick.AddListener(LoginEvent);
        register_btn.onClick.AddListener(RegisterEvent);
    }

    public void Login(string id_input, string Pwd_input)
    {
        MySqlParameter[] parameters = new MySqlParameter[2];
        parameters[0] = new MySqlParameter("@User_Id", id_input);
        parameters[1] = new MySqlParameter("@User_Password", Pwd_input);

        string query = "SELECT * FROM user_info WHERE User_Id=@User_Id AND User_Password=@User_Password";
        MySqlDataReader reader = SQL_Manager.instance.ExecuteQuery(query, parameters);

        if (reader.HasRows) // 행 하나라도 있나?
        {
            if (reader.Read())
            {
                string id = (reader.IsDBNull(0)) ? string.Empty : reader["User_Id"].ToString();
                string pwd = (reader.IsDBNull(1)) ? string.Empty : reader["User_Password"].ToString();
                string nickname = (reader.IsDBNull(2)) ? string.Empty : reader["User_NickName"].ToString();

                if (!id.Equals(string.Empty) || !pwd.Equals(string.Empty) || !nickname.Equals(string.Empty))
                {//정상적으로 data를 가지고 온상황...
                    info = new UserInfo(id, pwd, nickname);
                    if (!reader.IsClosed) reader.Close();
                    //LogText_viewing("로그인 성공!");
                    AuthPanel.SetActive(false);
                    anykeyText.SetActive(true);
                }
                else
                {
                    if (!reader.IsClosed) reader.Close();
                    LogText_viewing("로그인 실패!");
                }

            }
        }
        else {
            if (!reader.IsClosed) reader.Close();
            LogText_viewing("정확한 ID이나 PASSWORD를 다시 입력하세요");
        }
    }

    public void LoginEvent()
    {
        if (id_input.text.Equals(string.Empty) ||
            Pwd_input.text.Equals(string.Empty))
        {
            LogText_viewing("ID이나 PASSWORD를 입력하세요");
            return;
        }

        Login(id_input.text, Pwd_input.text);
    }

    public void ResetInputField()
    {
        id_input.text = string.Empty;
        Pwd_input.text = string.Empty;
        nickname_input.text = string.Empty;
    }

    private bool Auth_check(string id_input)
    {
        MySqlParameter[] parameters = new MySqlParameter[1];
        parameters[0] = new MySqlParameter("@User_Id", id_input);

        string query = "SELECT * FROM user_info WHERE User_Id=@User_Id";
        MySqlDataReader reader = SQL_Manager.instance.ExecuteQuery(query, parameters);

        if (reader.HasRows) // 행 하나라도 있나?
        {
            if (!reader.IsClosed) reader.Close();
            //있으면 못 만들어
            return false;
        }
        else
        {
            if (!reader.IsClosed) reader.Close();
            //없으니깐 통과
            return true;
        }

    }

    //등록
    public void Register(string id_input, string Pwd_input, string nickname_input)
    {
        MySqlParameter[] parameters = new MySqlParameter[3];
        parameters[0] = new MySqlParameter("@User_Id", id_input);
        parameters[1] = new MySqlParameter("@User_Password", Pwd_input);
        parameters[2] = new MySqlParameter("@User_NickName", nickname_input);

        string query = "INSERT INTO user_info (User_Id, User_Password, User_NickName) VALUES (@User_Id, @User_Password, @User_NickName)";
        SQL_Manager.instance.ExecuteNonQuery(query, parameters);
    }

    public void RegisterEvent()
    {
        if (id_input.text.Equals(string.Empty) ||
            Pwd_input.text.Equals(string.Empty)||
            nickname_input.text.Equals(string.Empty))
        {
            LogText_viewing("이름/비밀번호/닉네임을 입력하세요");
            return;
        }

        if (Auth_check(id_input.text))
        {
            Register(id_input.text, Pwd_input.text, nickname_input.text);
            LogText_viewing("회원가입하였습니다.");
        }
        else
        {
            LogText_viewing("해당 ID는 존재합니다.");
        }

    }

    public void LogText_viewing(string text)
    {
        logText.text = text;
    }

}
