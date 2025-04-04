﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace atOpticalDecenter
{
    public partial class LoginForm : DevExpress.XtraEditors.XtraForm
    {
        public string WorkerID { get; set; } = string.Empty;
        public string WorkerName { get; set; } = string.Empty;
        public string JobInformation { get; set; } = string.Empty;
        public LoginForm()
        {
            InitializeComponent();
        }
        public LoginForm(bool _bSystemLanguage)
        {
            InitializeComponent();
            if (!_bSystemLanguage)
            {
                simpleButtonExit.Text = "Exit";
                simpleButtonLogIn.Text = "Login";
                layoutControlItem1.Text = "Worker ID";
                layoutControlItem2.Text = "Worker Name";
                layoutControlItem3.Text = "Order Number";
            }
            else
            {
                simpleButtonExit.Text = "종료";
                simpleButtonLogIn.Text = "로그인";
                layoutControlItem1.Text = "작업자 사번";
                layoutControlItem2.Text = "작업자 이름";
                layoutControlItem3.Text = "작업 지시서";
            }

        }
        public LoginForm(string strWorkID, string strWorkerName, string strJobInformation)
        {
            InitializeComponent();
            textEditWorkerID.Text = strWorkID;
            textEditWorkerName.Text = strWorkerName;
            textEditJobInformation.Text = strJobInformation;
        }

        private void textEditWorkerID_EditValueChanging(object sender, DevExpress.XtraEditors.Controls.ChangingEventArgs e)
        {
            this.WorkerID = Convert.ToString(e.NewValue);
        }

        private void textEditWorkerName_EditValueChanging(object sender, DevExpress.XtraEditors.Controls.ChangingEventArgs e)
        {
            this.WorkerName = Convert.ToString(e.NewValue);
        }

        private void textEditJobInformation_EditValueChanging(object sender, DevExpress.XtraEditors.Controls.ChangingEventArgs e)
        {
            this.JobInformation = Convert.ToString(e.NewValue);
        }

        private void LoginForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.DialogResult == DialogResult.OK)
            {
                if (string.IsNullOrEmpty(this.WorkerID))
                {
                    MessageBox.Show("작업자 사번이 입력되지 않았습니다.", "사번 누락", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    e.Cancel = true;

                    return;
                }
                else
                {
                    int value;

                    if (int.TryParse(this.WorkerID, out value))
                    {
                        if (value < 10000000 || value > 11000000)
                        {
                            MessageBox.Show("작업자 사번이 잘 못 입력되었습니다.", "사번 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);

                            e.Cancel = true;

                            return;
                        }
                    }
                    else
                    {
                        MessageBox.Show("작업자 사번이 잘 못 입력되었습니다.", "사번 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        e.Cancel = true;

                        return;
                    }
                }

                if (string.IsNullOrEmpty(this.WorkerName))
                {
                    MessageBox.Show("작업자 이름이 입력되지 않았습니다.", "이름 누락", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    e.Cancel = true;

                    return;
                }

                if (string.IsNullOrEmpty(this.JobInformation))
                {
                    MessageBox.Show("작업지시서가 입력되지 않았습니다.", "작업지시서 누락", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    e.Cancel = true;

                    return;
                }
            }
        }

        private void textEditJobInformation_EditValueChanged(object sender, EventArgs e)
        {
            string source = string.Empty;
            source = textEditJobInformation.Text;

            if (source.Length > 7)
            {
                if (source.IndexOf("%") == -1)
                {
                    if (source.Length > 12)
                    {
                        if (source.Length == 13)
                        {
                            JobInformation = source;
                        }
                        else
                        {
                            JobInformation = string.Empty;
                        }
                    }
                }
                else
                {
                    string[] words = source.Split('%');
                    if (words[1].Length > 12)
                    {
                        if (words[1].Length == 13)
                        {
                            JobInformation = words[1];
                        }
                        else
                        {
                            JobInformation = string.Empty;
                        }
                    }
                }

                if (textEditJobInformation.InvokeRequired)
                {
                    textEditJobInformation.Invoke(new MethodInvoker(delegate { textEditJobInformation.EditValue = JobInformation; }));
                }
                else
                {
                    //textEditJobInformation.EditValue = JobInformation;
                    textEditJobInformation.Text = JobInformation;
                }
            }
        }
    }
}