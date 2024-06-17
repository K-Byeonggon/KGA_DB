using UnityEngine;
using UnityEngine.UI;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using Org.BouncyCastle.Asn1.Crmf;

public class DatabaseUI : MonoBehaviour
{
   [Header("UI")]
   [SerializeField] InputField Input_Query;
   [SerializeField] Text Text_DBResult;
   [SerializeField] Text Text_Log;

    [Header("ConnectionInfo")]
    [SerializeField] string _ip = "127.0.0.1";
    [SerializeField] string _dbName = "dungeon_busters";
    [SerializeField] string _uid = "root";
    [SerializeField] string _pwd = "1234";

    private bool _isConnectTestComplete; //중요하진 않음.

    private static MySqlConnection _dbConnection;

    private void Awake()
    {
        this.gameObject.SetActive(false);
    }

    //쿼리 보내기
    private void SendQuery(String queryStr, string tableName)
    {
        //있으면 Select 관련함수 호출
        if (queryStr.Contains("SELECT"))
        {
            DataSet dataSet = OnSelectRequest(queryStr, tableName);
            Text_DBResult.text = DeformatResult(dataSet);
        }
        else //없으면 Insert 또는 Update 관련 쿼리 (인서트 업데이트는 bool값 관련)
        {
            Text_DBResult.text = OnInsertOnUpdateRequest(queryStr) ? "성공" : "실패";
        }

    }

    public static bool OnInsertOnUpdateRequest(string query)
    {
        try
        {
            MySqlCommand sqlCommand = new MySqlCommand();
            sqlCommand.Connection = _dbConnection;
            sqlCommand.CommandText = query;

            _dbConnection.Open();
            sqlCommand.ExecuteNonQuery();
            _dbConnection.Close();
            return true;
        }
        catch (Exception e)
        {
            //이상한 쿼리 날리면 여기로
            return false;
        }
    }

    private string DeformatResult(DataSet dataSet)
    {
        string resultStr = string.Empty;

        foreach(DataTable table in dataSet.Tables)
        {
            foreach(DataRow row in table.Rows)
            {
                foreach(DataColumn column in table.Columns)
                {
                    resultStr += $"{column.ColumnName}: {row[column]}\n";
                }
            }
        }
        return resultStr;
    }

    public static DataSet OnSelectRequest(string query, string tableName)
    {
        try
        {
            _dbConnection.Open();
            MySqlCommand sqlCmd = new MySqlCommand();
            sqlCmd.Connection = _dbConnection;
            sqlCmd.CommandText = query;
            
            //이 객체 사용해서 데이터 긁어옴.
            MySqlDataAdapter sd = new MySqlDataAdapter(sqlCmd);
            DataSet dataSet = new DataSet();
            sd.Fill(dataSet, tableName);

            _dbConnection.Close();
            return dataSet;
        }
        catch(Exception e)
        {
            Debug.Log(e.ToString());
            return null;
        }
    }

    //DB와 연결하는 과정
    private bool ConnectTest()
    {
        //오타 안나게 주의 
        string connectStr = $"Server={_ip};Database={_dbName};Uid={_uid};PWd={_pwd};";

        try
        {
            using(MySqlConnection conn = new MySqlConnection(connectStr)) 
            {
                _dbConnection = conn;
                conn.Open();
            }

            Text_Log.text = "DB 연결을 성공했습니다!";
            return true;
        }
        catch (Exception e) 
        {
            Debug.LogWarning($"e: {e.ToString()}");
            Text_Log.text = "DB 연결 실패!";
            return false;
        }
    }


    public void OnClick_TestDBConnect()
    {
        _isConnectTestComplete = ConnectTest();
    }

    //OnSubmit 엔터 누르거나 할떄
    public void OnSubmit_SendQuery()
    {
        if(_isConnectTestComplete == false)
        {
            Text_Log.text = "DB 연결을 먼저 시도해주세요.";
            return;
        }
        Text_Log.text = string.Empty;

        string query = string.IsNullOrWhiteSpace(Input_Query.text) ? "SELECT U_Name,U_Password,U_Id FROM user_info"
            : Input_Query.text;

        SendQuery(query, "user_info");
    }

    public void OnClick_OpenDatabaseUI()
    {
        this.gameObject.SetActive(true);
    }

    public void OnClick_CloseDatabaseUI()
    {
        this.gameObject.SetActive(false);
    }

}
