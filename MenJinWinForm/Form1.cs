using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;
using System.Windows.Forms;
using System.Timers;

namespace MenJinWinForm
{
    enum State
    {
        idle,
        readCardID,
        writeCardID,
        readHistory,
        writeOpenTime,
        readOpenTime,
        writeTime,
        readTime,
        remoteOpen,
        update,
        reboot
    }
    public partial class Form1 : Form
    {
        private string strCheckedID = "";
        private System.Timers.Timer timer = new System.Timers.Timer();
        private State state;

        public Form1()
        {
            InitializeComponent();
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;//设置该属性 为false
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox_remoteOpen.Items.Add("A");
            comboBox_remoteOpen.Items.Add("B");

            textBox_update.Text = @"F:\LW_files\stm32F1\MenJin\MenJinProject\Project\MDK-ARM(uV4)\Flash\Obj\output.bin";

            UtilClass.utilInit();
            MySQLDB.m_strConn = System.Configuration.ConfigurationManager.AppSettings["ServerDB"];
            //设置定时器
            state = State.idle;
            timer.Interval = 500;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(regularRead);
            timer.Start();
        }

        //定时查询数据库
        private void regularRead(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                switch (state)
                {
                    case State.idle:
                    {
                        //查询刷卡号并置-1
                        string[,] ret = winFormDbClass.readDeviceInfo();
                        int a = ret.GetLength(1);
                        for (int i = 0; i < ret.Length / ret.GetLength(1); i++)
                        {
                            if (ret[i, 0] == strCheckedID)
                            {
                                textBox_cardNow.Text = ret[i, 2];
                            }
                        }
                    }
                        break;

                    case State.readCardID:
                        //查询命令执行结果
                        if (strCheckedID != "")
                        {
                            string[,] ret = winFormDbClass.readCmd(strCheckedID);
                            string card;
                                                   
                            if (ret[0, 0] == "ok")
                            {
                                for (int j = 0; j < ret[0, 1].Length; j+=6)
                                {
                                    card = ret[0, 1].Substring(j, 6);
                                    if (card != "FFFFFF")
                                    {
                                        checkedListBox_card.Items.Add(card, true);
                                    }
                                }                               
                                state = State.idle;
                                MessageBox.Show("成功");
                            }
                            else if (ret[0, 0] == "fail")
                            {
                                state = State.idle;
                                MessageBox.Show("失败");
                            }
                        }                       
                        break;

                    case State.writeCardID:
                        //查询命令执行结果
                        if (strCheckedID != "")
                        {
                            string[,] ret = winFormDbClass.readCmd(strCheckedID);
                            //string card;

                            if (ret[0, 0] == "ok")
                            {
                                state = State.idle;
                                MessageBox.Show("成功");
                            }
                            else if (ret[0, 0] == "fail")
                            {
                                state = State.idle;
                                MessageBox.Show("失败");
                            }
                        }
                        break;

                    case State.readHistory:
                        //查询命令执行结果
                        if (strCheckedID != "")
                        {

                            string[,] ret = winFormDbClass.readCmd(strCheckedID);
                            if (ret[0, 0] == "ok")
                            {
                                string[,] retHistory = winFormDbClass.readHistory(strCheckedID);
                                if (retHistory != null)
                                {
                                    for (int i = 0; i < retHistory.Length / retHistory.GetLength(1); i++)
                                    {
                                        listBox_history.Items.Add(
                                            retHistory[i, 0] + "[" + retHistory[i, 1] + "]" + "[" + retHistory[i, 2] +
                                            "]");
                                    }
                                }

                                state = State.idle;
                                MessageBox.Show("成功");
                            }
                            else if (ret[0, 0] == "fail")
                            {
                                state = State.idle;
                                MessageBox.Show("失败");
                            }
                        }
                        break;

                    case State.writeOpenTime:
                        //查询命令执行结果
                        if (strCheckedID != "")
                        {
                            string[,] ret = winFormDbClass.readCmd(strCheckedID);

                            if (ret[0, 0] == "ok")
                            {
                                state = State.idle;
                                MessageBox.Show("成功");
                            }
                            else if (ret[0, 0] == "fail")
                            {
                                state = State.idle;
                                MessageBox.Show("失败");
                            }
                        }
                        break;

                    case State.readOpenTime:
                        //查询命令执行结果
                        if (strCheckedID != "")
                        {
                            string[,] ret = winFormDbClass.readCmd(strCheckedID);

                            if (ret[0, 0] == "ok")
                            {
                                state = State.idle;
                                MessageBox.Show(ret[0, 1]);
                                //MessageBox.Show("成功");
                            }
                            else if (ret[0, 0] == "fail")
                            {
                                state = State.idle;
                                MessageBox.Show("失败");
                            }
                        }
                        break;

                    case State.writeTime:
                        //查询命令执行结果
                        if (strCheckedID != "")
                        {
                            string[,] ret = winFormDbClass.readCmd(strCheckedID);

                            if (ret[0, 0] == "ok")
                            {
                                state = State.idle;
                                MessageBox.Show("成功");
                            }
                            else if (ret[0, 0] == "fail")
                            {
                                state = State.idle;
                                MessageBox.Show("失败");
                            }
                        }
                        break;

                    case State.readTime:
                        //查询命令执行结果
                        if (strCheckedID != "")
                        {
                            string[,] ret = winFormDbClass.readCmd(strCheckedID);

                            if (ret[0, 0] == "ok")
                            {                                
                                state = State.idle;
                                MessageBox.Show(ret[0, 1]);
                                //MessageBox.Show("成功");
                            }
                            else if (ret[0, 0] == "fail")
                            {
                                state = State.idle;
                                MessageBox.Show("失败");
                            }
                        }
                        break;

                    case State.remoteOpen:
                        //查询命令执行结果
                        if (strCheckedID != "")
                        {
                            string[,] ret = winFormDbClass.readCmd(strCheckedID);

                            if (ret[0, 0] == "ok")
                            {
                                state = State.idle;
                                MessageBox.Show("成功");
                            }
                            else if (ret[0, 0] == "fail")
                            {
                                state = State.idle;
                                MessageBox.Show("失败");
                            }
                        }
                        break;

                    case State.update:
                        //查询命令执行结果
                        if (strCheckedID != "")
                        {
                            string[,] ret = winFormDbClass.readCmd(strCheckedID);
                            textBox_updateProgress.Text = ret[0, 1];
                            if (ret[0, 0] == "ok")
                            {
                                state = State.idle;
                                MessageBox.Show("成功");
                            }
                            else if (ret[0, 0] == "fail")
                            {
                                state = State.idle;
                                MessageBox.Show("失败");
                            }
                        }
                        break;

                    case State.reboot:
                        //查询命令执行结果
                        if (strCheckedID != "")
                        {
                            string[,] ret = winFormDbClass.readCmd(strCheckedID);

                            if (ret[0, 0] == "ok")
                            {
                                state = State.idle;
                                MessageBox.Show("成功");
                            }
                            else if (ret[0, 0] == "fail")
                            {
                                state = State.idle;
                                MessageBox.Show("失败");
                            }
                        }
                        break;


                    default:
                        break;
                }

            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }       
            
        }

        private void button_search_Click(object sender, EventArgs e)
        {
            //先清空
            checkedListBox1.Items.Clear();
            textBox_selected.Text = "";

            string[,] ret = winFormDbClass.readDeviceInfo();
            if (ret != null)
            {
                for (int i = 0; i < ret.Length / ret.GetLength(1); i++)
                {
                    checkedListBox1.Items.Add(ret[i, 0] +"["+ ret[i, 1]+"]");
                }
            }
        }

        //读取卡号
        private void button1_Click(object sender, EventArgs e)
        {
            if (strCheckedID != "")
            {
                checkedListBox_card.Items.Clear();
                winFormDbClass.UpdateCmd(strCheckedID, "cmdName", "operation", "data", "generalCardID","read", "00");
                state = State.readCardID;
            }
        }

        //选择设备
        private void button_select_Click(object sender, EventArgs e)
        {
            if (checkedListBox1.CheckedItems.Count == 1)
            {
                textBox_selected.Text = checkedListBox1.CheckedItems[0].ToString().Substring(0,8);//获取已勾选项的设备ID号
                strCheckedID = textBox_selected.Text;
            }
        }

        //读取刷卡记录
        private void button3_Click(object sender, EventArgs e)
        {
            if (strCheckedID != "")
            {
                listBox_history.Items.Clear();
                winFormDbClass.UpdateCmd(strCheckedID, "cmdName", "operation", "data", "history", "read", "00");
                state = State.readHistory;
            }
        }
        
        //写卡号
        private void button2_Click(object sender, EventArgs e)
        {
            if (strCheckedID != "")
            {
                string allCardID = "";
                for (int i = 0; i < checkedListBox_card.CheckedItems.Count; i++)
                {
                    allCardID += checkedListBox_card.CheckedItems[i];
                }

                int len = allCardID.Length;
                for (int j = 0; j < (500 -  len/ 6); j++)
                {
                    allCardID += "FFFFFF";
                }

                winFormDbClass.UpdateCmd(strCheckedID, "cmdName", "operation", "data", "generalCardID", "write",
                    allCardID);
                state = State.writeCardID;
            }
        }
        //把当前刷卡号添加到复选框
        private void button_addCard_Click(object sender, EventArgs e)
        {
            checkedListBox_card.Items.Add(textBox_cardNow.Text, true);
        }

        //全选
        private void button_SelectAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBox_card.Items.Count; i++)
            {
                checkedListBox_card.SetItemChecked(i, true);
            }
        }

        //反选
        private void button_deSelectAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBox_card.Items.Count; i++)
            {
                checkedListBox_card.SetItemChecked(i, false);
            }
        }
        //读取时间
        private void button_readTime_Click(object sender, EventArgs e)
        {
            if (strCheckedID != "")
            {
                winFormDbClass.UpdateCmd(strCheckedID, "cmdName", "operation", "data", "time", "read", "00");
                state = State.readTime;
            }
        }
        //设置时间
        private void button_setTime_Click(object sender, EventArgs e)
        {
            if (strCheckedID != "")
            {
                string str = textBox_time.Text;
                if (str.Length == 5 * 2)
                {
                    winFormDbClass.UpdateCmd(strCheckedID, "cmdName", "operation", "data", "time", "write", str);
                    state = State.writeTime;
                }
            }
        }
        //设置开门时长
        private void button_setOpenTime_Click(object sender, EventArgs e)
        {
            if (strCheckedID != "")
            {
                string str = textBox_openTime.Text;
                if (str != "")
                {
                    winFormDbClass.UpdateCmd(strCheckedID, "cmdName", "operation", "data", "openTime", "write", str);
                    state = State.writeOpenTime;
                }
            }
        }

        //读取开门时长
        private void button_readOpenTime_Click(object sender, EventArgs e)
        {
            if (strCheckedID != "")
            {
                //string str = textBox_openTime.Text;
                //if (str != "")
                //{
                    winFormDbClass.UpdateCmd(strCheckedID, "cmdName", "operation", "data", "openTime", "read", "00");
                    state = State.readOpenTime;
                //}
            }
        }

        private void button_remoteOpen_Click(object sender, EventArgs e)
        {
            if (strCheckedID != "")
            {
                switch (comboBox_remoteOpen.SelectedIndex)
                {
                    case 0:
                        winFormDbClass.UpdateCmd(strCheckedID, "cmdName", "operation", "data", "remoteOpen", "write", "01");
                        break;

                    case 1:
                        winFormDbClass.UpdateCmd(strCheckedID, "cmdName", "operation", "data", "remoteOpen", "write", "10");
                        break;

                    default:
                        break;
                }
                state = State.remoteOpen;

            }
        }

        private void button_update_Click(object sender, EventArgs e)
        {
            if (strCheckedID != "")
            {
                winFormDbClass.UpdateCmd(strCheckedID, "cmdName", "operation", "data", "update", "write", textBox_update.Text);
                state = State.update;
            }
        }

        private void button_reboot_Click(object sender, EventArgs e)
        {
            if (strCheckedID != "")
            {
                winFormDbClass.UpdateCmd(strCheckedID, "cmdName", "operation", "data", "reboot", "write", "00");
                state = State.reboot;
            }
        }
    }
}
