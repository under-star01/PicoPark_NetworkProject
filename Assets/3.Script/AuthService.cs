using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AuthService : MonoBehaviour
{
    [SerializeField] private TMP_InputField id_input;
    [SerializeField] private TMP_InputField Pwd_input;
    [SerializeField] private TMP_InputField nickname_input;

    [SerializeField] private Button login_btn;
    [SerializeField] private Button register_btn;

    [SerializeField] private TMP_Text logText;

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
        parameters.Add(id_input);
        parameters.Add(Pwd_input);
    }

    public void LoginEvent()
    {
        if (name_input.text.Equals(string.Empty) ||
            Pwd_input.text.Equals(string.Empty))
        {
            LogText_viewing("이름이나 비밀번호를 입력하세요");
            return;
        }
    }

    public void Register()
    {

    }

    public void RegisterEvent()
    {

    }

    public void LogText_viewing(string text)
    {
        logText.text = text;
    }

}
