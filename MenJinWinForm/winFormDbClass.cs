using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Data;

namespace MenJinWinForm
{
    class winFormDbClass
    {
        public static string[,] readDeviceInfo()
        {
            MySQLDB.InitDb();
            string sensorid = "0";
            string[,] ret;
            //从数据库中查找当前ID是否存在
            try
            {
                DataSet ds1 = new DataSet("tdevice");
                string strSQL1 = "SELECT * FROM tdevice WHERE status = 'True'";
                ds1 = MySQLDB.SelectDataSet(strSQL1, null);
                if (ds1 != null)
                {
                    // 有数据集
                    int count = ds1.Tables[0].Rows.Count;
                    if (count > 0)
                    {
                        ret = new string[count, 3];
                        for (int i = 0; i < count; i++)
                        {
                            ret[i, 0] = ds1.Tables[0].Rows[i]["deviceID"].ToString();
                            ret[i, 1] = ds1.Tables[0].Rows[i]["lastLoginTime"].ToString();
                            ret[i, 2] = ds1.Tables[0].Rows[i]["cardID_now"].ToString();
                        }

                        //重置字段为-1,add3-5
                        string strSQL2 = "UPDATE tdevice SET cardID_now = '-1'";
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
                return null; //数据库异常
            }
        }

        //更新设备信息
        public static string UpdateCmd(string sensorintdeviceID, string updateItem, string updateItem1,
            string updateItem2, string updateNum, string updateNum1, string updateNum2)
        {
            MySQLDB.InitDb();
            string strResult = "";
            MySqlParameter[] parmss = null;
            string strSQL = "";
            bool IsDelSuccess = false;
            strSQL =
                "Update tcommand SET " + updateItem + " =?sensorupdateItem, " + updateItem1 + " =?sensorupdateItem1, " +
                updateItem2 + " =?sensorupdateItem2 WHERE deviceID=?sensorintdeviceID";
            parmss = new MySqlParameter[]
            {
                new MySqlParameter("?sensorintdeviceID", MySqlDbType.VarChar),
                new MySqlParameter("?sensorupdateItem", MySqlDbType.VarChar),
                new MySqlParameter("?sensorupdateItem1", MySqlDbType.VarChar),
                new MySqlParameter("?sensorupdateItem2", MySqlDbType.VarChar)
            };
            parmss[0].Value = sensorintdeviceID;
            parmss[1].Value = updateNum;
            parmss[2].Value = updateNum1;
            parmss[3].Value = updateNum2;

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
        public static string[,] readCmd(string id)
        {
            MySQLDB.InitDb();
            string[,] ret;
            //从数据库中查找当前ID是否存在
            try
            {
                DataSet ds1 = new DataSet("tcommand");
                string strSQL1 =
                    "SELECT cmdName, data FROM tcommand WHERE deviceID="+"\""+id+ "\"";
                ds1 = MySQLDB.SelectDataSet(strSQL1, null);
                if (ds1 != null)
                {
                    // 有数据集
                    int count = ds1.Tables[0].Rows.Count;
                    if (count > 0)
                    {
                        ret = new string[count, 2];
                        for (int i = 0; i < count; i++)
                        {
                            ret[i, 0] = ds1.Tables[0].Rows[i]["cmdName"].ToString();
                            ret[i, 1] = ds1.Tables[0].Rows[i]["data"].ToString();
                        }

                        //重置字段为-1,add3-5
                        //string strSQL2 = "UPDATE tcommand SET cmdName = '-1'";
                        //ds1 = MySQLDB.SelectDataSet(strSQL2, null);

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

        //读取刷卡记录
        public static string[,] readHistory(string id)
        {
            MySQLDB.InitDb();
            string[,] ret;
            string childName = "thistorychild" + id;
            //从数据库中查找当前ID是否存在
            try
            {
                DataSet ds1 = new DataSet("tcommand");
                string strSQL1 =
                    "SELECT * FROM "+ childName;
                ds1 = MySQLDB.SelectDataSet(strSQL1, null);
                if (ds1 != null)
                {
                    // 有数据集
                    int count = ds1.Tables[0].Rows.Count;
                    if (count > 0)
                    {
                        ret = new string[count, 3];
                        for (int i = 0; i < count; i++)
                        {
                            ret[i, 0] = ds1.Tables[0].Rows[i]["CardID"].ToString();
                            ret[i, 1] = ds1.Tables[0].Rows[i]["DataDate"].ToString();
                            ret[i, 2] = ds1.Tables[0].Rows[i]["DoorID"].ToString();
                        }

                        //重置字段为-1,add3-5
                        //string strSQL2 = "UPDATE tcommand SET cmdName = '-1'";
                        //ds1 = MySQLDB.SelectDataSet(strSQL2, null);

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



    }
}
