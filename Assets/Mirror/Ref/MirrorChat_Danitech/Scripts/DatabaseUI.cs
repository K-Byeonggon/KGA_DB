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

    private bool _isConnectTestComplete; //�߿����� ����.

    private static MySqlConnection _dbConnection;

    private void Awake()
    {
        this.gameObject.SetActive(false);
    }

    //���� ������
    private void SendQuery(String queryStr, string tableName)
    {
        //������ Select �����Լ� ȣ��
        if (queryStr.Contains("SELECT"))
        {
            DataSet dataSet = OnSelectRequest(queryStr, tableName);
            Text_DBResult.text = DeformatResult(dataSet);
        }
        else //������ Insert �Ǵ� Update ���� ���� (�μ�Ʈ ������Ʈ�� bool�� ����)
        {
            Text_DBResult.text = OnInsertOnUpdateRequest(queryStr) ? "����" : "����";
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
            //�̻��� ���� ������ �����
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
            
            //�� ��ü ����ؼ� ������ �ܾ��.
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

    //DB�� �����ϴ� ����
    private bool ConnectTest()
    {
        //��Ÿ �ȳ��� ���� 
        string connectStr = $"Server={_ip};Database={_dbName};Uid={_uid};PWd={_pwd};";

        try
        {
            using(MySqlConnection conn = new MySqlConnection(connectStr)) 
            {
                _dbConnection = conn;
                conn.Open();
            }

            Text_Log.text = "DB ������ �����߽��ϴ�!";
            return true;
        }
        catch (Exception e) 
        {
            Debug.LogWarning($"e: {e.ToString()}");
            Text_Log.text = "DB ���� ����!";
            return false;
        }
    }


    public void OnClick_TestDBConnect()
    {
        _isConnectTestComplete = ConnectTest();
    }

    //OnSubmit ���� �����ų� �ҋ�
    public void OnSubmit_SendQuery()
    {
        if(_isConnectTestComplete == false)
        {
            Text_Log.text = "DB ������ ���� �õ����ּ���.";
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
