using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Data;

namespace MenJinService
{
    /// <summary>
    /// 数据库操作类
    /// </summary>
    class DbClass
    {
        public static string addsensorinfo(string sensorintdeviceID, string sensorloginTime, string sensorStatus)
        {
            MySQLDB.InitDb();
            string sensorid = "0";
            //从数据库中查找当前ID是否存在
            try
            {
                DataSet ds1 = new DataSet("tdevice");
                string strSQL1 = "SELECT deviceID FROM tdevice where deviceID=" + "\"" + sensorintdeviceID + "\"";
                ds1 = MySQLDB.SelectDataSet(strSQL1, null);
                if (ds1 != null)
                {
                    if (ds1.Tables[0].Rows.Count > 0)
                        // 有数据集
                    {
                        sensorid = ds1.Tables[0].Rows[0][0].ToString();

                    }
                }
            }
            catch (Exception ex)
            {
                return "fail"; //数据库异常
            }

            //************************************************************
            if (sensorid == "0") //若不存在,则添加
            {
                DataSet ds = new DataSet("tdevice");
                string strResult = "";
                MySqlParameter[] parmss = null;
                string strSQL = "";
                bool IsDelSuccess = false;
                strSQL = " insert into tdevice (deviceID, lastLoginTime, status) values" +
                         "(?sensorintdeviceID,?sensorloginTime,?sensorStatus);";

                parmss = new MySqlParameter[]
                {
                    new MySqlParameter("?sensorintdeviceID", MySqlDbType.VarChar),
                    new MySqlParameter("?sensorloginTime", MySqlDbType.VarChar),
                    new MySqlParameter("?sensorStatus", MySqlDbType.VarChar)
                };
                parmss[0].Value = sensorintdeviceID;
                parmss[1].Value = sensorloginTime;
                parmss[2].Value = sensorStatus;

                try
                {
                    IsDelSuccess = MySQLDB.ExecuteNonQry(strSQL, parmss);

                    if (IsDelSuccess != false)
                    {
                        return "ok";
                    }
                    else
                    {
                        return "fail";
                    }
                }

                catch (Exception ex)
                {
                    return "fail"; //数据库异常
                }
            }

            else //若ID存在,就更新update
            {
                DataSet ds = new DataSet("dssensorinfo");
                string strResult = "";
                MySqlParameter[] parmss = null;
                string strSQL = "";
                bool IsDelSuccess = false;
                strSQL =
                    "Update tdevice SET lastLoginTime=?sensorloginTime, status=?sensorStatus WHERE deviceID=?sensorintdeviceID";

                parmss = new MySqlParameter[]
                {
                    new MySqlParameter("?sensorintdeviceID", MySqlDbType.VarChar),
                    new MySqlParameter("?sensorloginTime", MySqlDbType.DateTime),
                    new MySqlParameter("?sensorStatus", MySqlDbType.VarChar)

                };
                parmss[0].Value = sensorintdeviceID;
                parmss[1].Value = Convert.ToDateTime(sensorloginTime);
                parmss[2].Value = sensorStatus;

                try
                {
                    IsDelSuccess = MySQLDB.ExecuteNonQry(strSQL, parmss);

                    if (IsDelSuccess != false)
                    {
                        return "ok";
                    }
                    else
                    {
                        return "fail";
                    }
                }

                catch (Exception ex)
                {
                    return "fail"; //数据库异常
                }
            }

        }

        //更新设备信息
        public static string UpdateSensorInfo(string sensorintdeviceID, string updateItem, string updateNum)
        {
            MySQLDB.InitDb();
            string strResult = "";
            MySqlParameter[] parmss = null;
            string strSQL = "";
            bool IsDelSuccess = false;
            strSQL =
                "Update tdevice SET " + updateItem + " =?sensorupdateItem WHERE deviceID=?sensorintdeviceID";
            parmss = new MySqlParameter[]
            {
                new MySqlParameter("?sensorintdeviceID", MySqlDbType.VarChar),
                new MySqlParameter("?sensorupdateItem", MySqlDbType.VarChar)
            };
            parmss[0].Value = sensorintdeviceID;
            parmss[1].Value = updateNum;

            try
            {
                IsDelSuccess = MySQLDB.ExecuteNonQry(strSQL, parmss);

                if (IsDelSuccess != false)
                {
                    return "ok";
                }
                else
                {
                    return "fail";
                }
            }

            catch (Exception ex)
            {
                return "fail";
            }
        }

        /// <summary>
        /// 读取命令,返回二维数组
        /// </summary>
        public static string[,] readCmd()
        {
            MySQLDB.InitDb();
            string[,] ret;
            //从数据库中查找当前ID是否存在
            try
            {
                DataSet ds1 = new DataSet("tcommand");
                string strSQL1 =
                    "SELECT * FROM tcommand where cmdName!='-1'";
                ds1 = MySQLDB.SelectDataSet(strSQL1, null);
                if (ds1 != null)
                {
                    // 有数据集
                    int count = ds1.Tables[0].Rows.Count;
                    if (count > 0)
                    {
                        ret = new string[count, 4];
                        for (int i = 0; i < count; i++)
                        {
                            ret[i, 0] = ds1.Tables[0].Rows[i]["deviceID"].ToString();
                            ret[i, 1] = ds1.Tables[0].Rows[i]["cmdName"].ToString();
                            ret[i, 2] = ds1.Tables[0].Rows[i]["operation"].ToString();
                            ret[i, 3] = ds1.Tables[0].Rows[i]["data"].ToString();
                        }

                        return ret;
                    }
                    else return null;
                }
                else return null;

            }
            catch (Exception ex)
            {
                return null;
            }
        }


    }
}
