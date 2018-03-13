namespace MenJinWinForm
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_selected = new System.Windows.Forms.TextBox();
            this.button_select = new System.Windows.Forms.Button();
            this.button_search = new System.Windows.Forms.Button();
            this.checkedListBox1 = new System.Windows.Forms.CheckedListBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.button_deSelectAll = new System.Windows.Forms.Button();
            this.button_SelectAll = new System.Windows.Forms.Button();
            this.button_addCard = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.textBox_cardNow = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.listBox_history = new System.Windows.Forms.ListBox();
            this.checkedListBox_card = new System.Windows.Forms.CheckedListBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.button_readOpenTime = new System.Windows.Forms.Button();
            this.button_setOpenTime = new System.Windows.Forms.Button();
            this.textBox_openTime = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.button_readTime = new System.Windows.Forms.Button();
            this.button_setTime = new System.Windows.Forms.Button();
            this.textBox_time = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.button_remoteOpen = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.comboBox_remoteOpen = new System.Windows.Forms.ComboBox();
            this.button_update = new System.Windows.Forms.Button();
            this.textBox_update = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.textBox_selected);
            this.groupBox1.Controls.Add(this.button_select);
            this.groupBox1.Controls.Add(this.button_search);
            this.groupBox1.Controls.Add(this.checkedListBox1);
            this.groupBox1.Location = new System.Drawing.Point(13, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(273, 656);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "搜索设备";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 475);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(95, 12);
            this.label2.TabIndex = 6;
            this.label2.Text = "当前操作设备ID:";
            // 
            // textBox_selected
            // 
            this.textBox_selected.Location = new System.Drawing.Point(7, 490);
            this.textBox_selected.Name = "textBox_selected";
            this.textBox_selected.Size = new System.Drawing.Size(105, 21);
            this.textBox_selected.TabIndex = 5;
            // 
            // button_select
            // 
            this.button_select.Location = new System.Drawing.Point(192, 431);
            this.button_select.Name = "button_select";
            this.button_select.Size = new System.Drawing.Size(75, 23);
            this.button_select.TabIndex = 2;
            this.button_select.Text = "选择";
            this.button_select.UseVisualStyleBackColor = true;
            this.button_select.Click += new System.EventHandler(this.button_select_Click);
            // 
            // button_search
            // 
            this.button_search.Location = new System.Drawing.Point(6, 431);
            this.button_search.Name = "button_search";
            this.button_search.Size = new System.Drawing.Size(75, 23);
            this.button_search.TabIndex = 1;
            this.button_search.Text = "搜索";
            this.button_search.UseVisualStyleBackColor = true;
            this.button_search.Click += new System.EventHandler(this.button_search_Click);
            // 
            // checkedListBox1
            // 
            this.checkedListBox1.FormattingEnabled = true;
            this.checkedListBox1.Location = new System.Drawing.Point(7, 21);
            this.checkedListBox1.Name = "checkedListBox1";
            this.checkedListBox1.Size = new System.Drawing.Size(260, 404);
            this.checkedListBox1.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.button_deSelectAll);
            this.groupBox2.Controls.Add(this.button_SelectAll);
            this.groupBox2.Controls.Add(this.button_addCard);
            this.groupBox2.Controls.Add(this.button3);
            this.groupBox2.Controls.Add(this.textBox_cardNow);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.button2);
            this.groupBox2.Controls.Add(this.button1);
            this.groupBox2.Controls.Add(this.listBox_history);
            this.groupBox2.Controls.Add(this.checkedListBox_card);
            this.groupBox2.Location = new System.Drawing.Point(293, 13);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(593, 656);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "卡号配置与记录读取";
            // 
            // button_deSelectAll
            // 
            this.button_deSelectAll.Location = new System.Drawing.Point(82, 627);
            this.button_deSelectAll.Name = "button_deSelectAll";
            this.button_deSelectAll.Size = new System.Drawing.Size(68, 23);
            this.button_deSelectAll.TabIndex = 7;
            this.button_deSelectAll.Text = "反选";
            this.button_deSelectAll.UseVisualStyleBackColor = true;
            this.button_deSelectAll.Click += new System.EventHandler(this.button_deSelectAll_Click);
            // 
            // button_SelectAll
            // 
            this.button_SelectAll.Location = new System.Drawing.Point(82, 602);
            this.button_SelectAll.Name = "button_SelectAll";
            this.button_SelectAll.Size = new System.Drawing.Size(68, 23);
            this.button_SelectAll.TabIndex = 6;
            this.button_SelectAll.Text = "全选";
            this.button_SelectAll.UseVisualStyleBackColor = true;
            this.button_SelectAll.Click += new System.EventHandler(this.button_SelectAll_Click);
            // 
            // button_addCard
            // 
            this.button_addCard.Location = new System.Drawing.Point(192, 21);
            this.button_addCard.Name = "button_addCard";
            this.button_addCard.Size = new System.Drawing.Size(58, 23);
            this.button_addCard.TabIndex = 5;
            this.button_addCard.Text = "添加";
            this.button_addCard.UseVisualStyleBackColor = true;
            this.button_addCard.Click += new System.EventHandler(this.button_addCard_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(290, 614);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(96, 23);
            this.button3.TabIndex = 4;
            this.button3.Text = "读取刷卡记录";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // textBox_cardNow
            // 
            this.textBox_cardNow.Location = new System.Drawing.Point(81, 21);
            this.textBox_cardNow.Name = "textBox_cardNow";
            this.textBox_cardNow.Size = new System.Drawing.Size(105, 21);
            this.textBox_cardNow.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 12);
            this.label1.TabIndex = 3;
            this.label1.Text = "当前刷卡号:";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(6, 627);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(70, 23);
            this.button2.TabIndex = 3;
            this.button2.Text = "写入卡号";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(7, 602);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(69, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "读取卡号";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // listBox_history
            // 
            this.listBox_history.FormattingEnabled = true;
            this.listBox_history.ItemHeight = 12;
            this.listBox_history.Location = new System.Drawing.Point(290, 16);
            this.listBox_history.Name = "listBox_history";
            this.listBox_history.Size = new System.Drawing.Size(297, 580);
            this.listBox_history.TabIndex = 1;
            // 
            // checkedListBox_card
            // 
            this.checkedListBox_card.FormattingEnabled = true;
            this.checkedListBox_card.Location = new System.Drawing.Point(6, 64);
            this.checkedListBox_card.Name = "checkedListBox_card";
            this.checkedListBox_card.Size = new System.Drawing.Size(200, 532);
            this.checkedListBox_card.TabIndex = 0;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.button_update);
            this.groupBox3.Controls.Add(this.textBox_update);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.comboBox_remoteOpen);
            this.groupBox3.Controls.Add(this.button_remoteOpen);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.button_readOpenTime);
            this.groupBox3.Controls.Add(this.button_setOpenTime);
            this.groupBox3.Controls.Add(this.textBox_openTime);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.button_readTime);
            this.groupBox3.Controls.Add(this.button_setTime);
            this.groupBox3.Controls.Add(this.textBox_time);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Location = new System.Drawing.Point(892, 13);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(326, 656);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "参数配置";
            // 
            // button_readOpenTime
            // 
            this.button_readOpenTime.Location = new System.Drawing.Point(156, 42);
            this.button_readOpenTime.Name = "button_readOpenTime";
            this.button_readOpenTime.Size = new System.Drawing.Size(52, 23);
            this.button_readOpenTime.TabIndex = 13;
            this.button_readOpenTime.Text = "读取";
            this.button_readOpenTime.UseVisualStyleBackColor = true;
            this.button_readOpenTime.Click += new System.EventHandler(this.button_readOpenTime_Click);
            // 
            // button_setOpenTime
            // 
            this.button_setOpenTime.Location = new System.Drawing.Point(98, 42);
            this.button_setOpenTime.Name = "button_setOpenTime";
            this.button_setOpenTime.Size = new System.Drawing.Size(52, 23);
            this.button_setOpenTime.TabIndex = 12;
            this.button_setOpenTime.Text = "确定";
            this.button_setOpenTime.UseVisualStyleBackColor = true;
            this.button_setOpenTime.Click += new System.EventHandler(this.button_setOpenTime_Click);
            // 
            // textBox_openTime
            // 
            this.textBox_openTime.Location = new System.Drawing.Point(10, 42);
            this.textBox_openTime.Name = "textBox_openTime";
            this.textBox_openTime.Size = new System.Drawing.Size(82, 21);
            this.textBox_openTime.TabIndex = 11;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 102);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(269, 12);
            this.label4.TabIndex = 10;
            this.label4.Text = "设置时间(例1203051423(2018年3月5日14时23分):";
            // 
            // button_readTime
            // 
            this.button_readTime.Location = new System.Drawing.Point(156, 117);
            this.button_readTime.Name = "button_readTime";
            this.button_readTime.Size = new System.Drawing.Size(52, 23);
            this.button_readTime.TabIndex = 9;
            this.button_readTime.Text = "读取";
            this.button_readTime.UseVisualStyleBackColor = true;
            this.button_readTime.Click += new System.EventHandler(this.button_readTime_Click);
            // 
            // button_setTime
            // 
            this.button_setTime.Location = new System.Drawing.Point(98, 117);
            this.button_setTime.Name = "button_setTime";
            this.button_setTime.Size = new System.Drawing.Size(52, 23);
            this.button_setTime.TabIndex = 8;
            this.button_setTime.Text = "确定";
            this.button_setTime.UseVisualStyleBackColor = true;
            this.button_setTime.Click += new System.EventHandler(this.button_setTime_Click);
            // 
            // textBox_time
            // 
            this.textBox_time.Location = new System.Drawing.Point(11, 118);
            this.textBox_time.Name = "textBox_time";
            this.textBox_time.Size = new System.Drawing.Size(81, 21);
            this.textBox_time.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 27);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(173, 12);
            this.label3.TabIndex = 6;
            this.label3.Text = "设置开门时间长度(秒,01至FF):";
            // 
            // button_remoteOpen
            // 
            this.button_remoteOpen.Location = new System.Drawing.Point(194, 165);
            this.button_remoteOpen.Name = "button_remoteOpen";
            this.button_remoteOpen.Size = new System.Drawing.Size(58, 23);
            this.button_remoteOpen.TabIndex = 15;
            this.button_remoteOpen.Text = "确定";
            this.button_remoteOpen.UseVisualStyleBackColor = true;
            this.button_remoteOpen.Click += new System.EventHandler(this.button_remoteOpen_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 168);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(59, 12);
            this.label5.TabIndex = 14;
            this.label5.Text = "远程开门:";
            // 
            // comboBox_remoteOpen
            // 
            this.comboBox_remoteOpen.FormattingEnabled = true;
            this.comboBox_remoteOpen.Location = new System.Drawing.Point(67, 165);
            this.comboBox_remoteOpen.Name = "comboBox_remoteOpen";
            this.comboBox_remoteOpen.Size = new System.Drawing.Size(114, 20);
            this.comboBox_remoteOpen.TabIndex = 16;
            // 
            // button_update
            // 
            this.button_update.Location = new System.Drawing.Point(194, 221);
            this.button_update.Name = "button_update";
            this.button_update.Size = new System.Drawing.Size(58, 23);
            this.button_update.TabIndex = 19;
            this.button_update.Text = "确定";
            this.button_update.UseVisualStyleBackColor = true;
            this.button_update.Visible = false;
            this.button_update.Click += new System.EventHandler(this.button_update_Click);
            // 
            // textBox_update
            // 
            this.textBox_update.Location = new System.Drawing.Point(6, 221);
            this.textBox_update.Name = "textBox_update";
            this.textBox_update.Size = new System.Drawing.Size(182, 21);
            this.textBox_update.TabIndex = 18;
            this.textBox_update.Visible = false;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(9, 206);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(83, 12);
            this.label6.TabIndex = 17;
            this.label6.Text = "升级文件路径:";
            this.label6.Visible = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1264, 681);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "门禁demo";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button button_search;
        private System.Windows.Forms.CheckedListBox checkedListBox1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ListBox listBox_history;
        private System.Windows.Forms.CheckedListBox checkedListBox_card;
        private System.Windows.Forms.Button button_addCard;
        private System.Windows.Forms.TextBox textBox_cardNow;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_selected;
        private System.Windows.Forms.Button button_select;
        private System.Windows.Forms.Button button_deSelectAll;
        private System.Windows.Forms.Button button_SelectAll;
        private System.Windows.Forms.Button button_readTime;
        private System.Windows.Forms.Button button_setTime;
        private System.Windows.Forms.TextBox textBox_time;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button_readOpenTime;
        private System.Windows.Forms.Button button_setOpenTime;
        private System.Windows.Forms.TextBox textBox_openTime;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button button_remoteOpen;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button button_update;
        private System.Windows.Forms.TextBox textBox_update;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox comboBox_remoteOpen;
    }
}

