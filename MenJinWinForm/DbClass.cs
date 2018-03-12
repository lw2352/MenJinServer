using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Data;

namespace MenJinWinForm
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
                Console.WriteLine(ex);
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
                    Console.WriteLine(ex);
                    return "fail"; //数据库异常
                }
            }

        }

        //更新所有设备在线信息为false
        public static string UpdateSensorInfo()
        {
            MySQLDB.InitDb();
            string strResult = "";
            MySqlParameter[] parmss = null;
            string strSQL = "";
            bool IsDelSuccess = false;
            strSQL =
                "Update tdevice SET status = 'False'";
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
                Console.WriteLine(ex);
                return "fail";
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
                Console.WriteLine(ex);
                return "fail";
            }
        }

        /// <summary>
        /// 读取命令,返回二维数组,并重置字段为-1
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
                    "SELECT * FROM tcommand where (cmdName!='-1' AND cmdName!='ok' AND cmdName!='fail')";
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

                        //重置字段为-1,add3-5
                        string strSQL2 ="UPDATE tcommand SET cmdName = '-1'";
                        ds1 = MySQLDB.SelectDataSet(strSQL2, null);

                        return ret;
                    }
                    else return null;
                }
                else return null;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        //创建历史记录子表
        public static string creatHistoryChildtable(string sensorintdeviceID)
        {
            MySQLDB.InitDb();
            string sensorid = "0";
            //从数据库中查找当前ID是否存在
            try
            {
                DataSet ds1 = new DataSet("thistory");
                string strSQL1 = "  SELECT deviceID FROM thistory WHERE deviceID=" + "\"" + sensorintdeviceID + "\"";
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
                Console.WriteLine(ex);
                return "fail"; //数据库异常
            }

            //************************************************************
            if (sensorid == "0") //若不存在,则添加,创建子表并新增数据
            {
                //DataSet ds = new DataSet("dssensorad");
                //string strResult = "";
                MySqlParameter[] parmss = null;
                string strSQL = "";
                //string strSQL2 = "";
                bool IsDelSuccess = false;
                //先在母表中插入ID和字表名
                string childName = "thistorychild" + sensorintdeviceID;
                strSQL =
                    "insert into thistory (deviceID, ChildTable) values (?sensorintdeviceID , ?sensorChildTable);";
                parmss = new MySqlParameter[]
                {
                    new MySqlParameter("?sensorintdeviceID", MySqlDbType.VarChar),
                    new MySqlParameter("?sensorChildTable", MySqlDbType.VarChar)
                };
                parmss[0].Value = sensorintdeviceID;
                parmss[1].Value = childName;

                try
                {
                    IsDelSuccess = MySQLDB.ExecuteNonQry(strSQL, parmss);

                    if (IsDelSuccess != false)
                    {
                        creatNewTable(childName); //创建子表
                        return "ok";
                    }
                    else
                    {
                        return "fail";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return "fail";
                }
            }
            else return "ok";
        }

        //创建新表
        private static string creatNewTable(string childName)
        {
            MySQLDB.InitDb();
            MySqlParameter[] parmss = null;
            string strSQL = "";
            bool IsDelSuccess = false;
            strSQL = "CREATE TABLE " + childName +
                     "(id INT AUTO_INCREMENT, CardID VARCHAR(45), DataDate VARCHAR(45), DoorID VARCHAR(45), PRIMARY KEY (`id`));"; //建立新表
            /*parmss = new MySqlParameter[]
                                     {
                                         new MySqlParameter("?sensorChildTable", MySqlDbType.VarChar)
                                     };
            parmss[0].Value = childName;*/
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
                Console.WriteLine(ex);
                return "fail";
            }
        }

        //清除ID对应的记录
        public static string deleteHistory(string strID)
        {
            MySQLDB.InitDb();
            MySqlParameter[] parmss = null;
            string strSQL = "";
            bool IsDelSuccess = false;
            string childName = "thistorychild" + strID;
            strSQL = "DELETE FROM "+ childName; //删除全部记录

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
                Console.WriteLine(ex);
                return "fail";
            }
        }

        //批量插入记录
        public static string insertHistory(string strID, string[,] dataStrings, int dataNum)
        {
            MySQLDB.InitDb();
            string strResult = "";
            MySqlParameter[] parmss = null;
            string strSQL = "";
            bool IsDelSuccess = false;
            string childName = "thistorychild" + strID;
            strSQL = "INSERT INTO "+ childName+"(CardID, DataDate, DoorID) VALUES ";
            for (int i = 0; i < dataNum; i++)
            {
                strSQL += "(" + "'" + dataStrings[i, 0] + "'" + "," + "'" + dataStrings[i, 1] + "'" + "," + "'" + dataStrings[i, 2] + "'" + ")";
                if (i < dataNum - 1)
                {
                    strSQL += ",";
                }
            }
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
                Console.WriteLine(ex);
                return "fail";
            }
        }

        //更新设备信息
        public static string UpdateCmd(string sensorintdeviceID, string updateItem, string updateNum)
        {
            MySQLDB.InitDb();
            string strResult = "";
            MySqlParameter[] parmss = null;
            string strSQL = "";
            bool IsDelSuccess = false;
            strSQL =
                "Update tcommand SET " + updateItem + " =?sensorupdateItem WHERE deviceID=?sensorintdeviceID";
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
                Console.WriteLine(ex);
                return "fail";
            }
        }


    }
}
