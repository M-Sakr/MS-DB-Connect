using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace CreateConnectionApp
{
    public partial class frm_CreateConnection : Form
    {
        public frm_CreateConnection()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CreateConnection createConnection = new CreateConnection();
            
            int DB_AttachedCount = 0;
            string pass = Directory.GetCurrentDirectory();             
            DirectoryInfo src = new DirectoryInfo(pass);

            if (src.Exists)
            {
                foreach (FileInfo file in src.GetFiles())
                {
                    if (file.Extension.ToLower() == ".mdf")
                    {
                        DB_AttachedCount++;
                        createConnection.DBfileI = file;
                        // u can comment what you did not need
                        createConnection.Connection();
                        createConnection.attatchDB();
                        #region Server Authintication and permission, you can comment this region if you don't use network" you use one device"
                        createConnection.Change_SQL_Server_authentication();
                        createConnection.createLogIn();
                        createConnection.createUser();
                        createConnection.createPermission();
                        #endregion
                        createConnection.GenerateConnectionTxT();
                        // you can comment this "SetArabicLang" if it is not important for you.
                        createConnection.SetArabicLang();
                    }

                }
                button1.BackColor = Color.Green;
                if (DB_AttachedCount == 0)
                    MessageBox.Show("No DataBase found");
                else
                    MessageBox.Show(DB_AttachedCount + " DataBases Done");
            }
            else
                MessageBox.Show("No DB exist in this pass ");
        }
    }
}
