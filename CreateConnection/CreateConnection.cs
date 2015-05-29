using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace CreateConnectionApp
{
    class CreateConnection : ICreateConnection
    {
        SqlConnection _con;
        SqlCommand    _sql_com;
        public FileInfo DBfileI;
        
        /// <summary>
        /// to know connection default instance or express
        /// </summary>        
        public void Connection()
        {
            // get DB Name
            string DBname = DBfileI.Name.Replace(".mdf", "");

            //usually we have 2 conditions for connection if default instacne "dot ." or SQLEXPRESS
            SqlConnection con = new SqlConnection();
            try
            {
                // if default instance = .
                // master connction we will use for attatch
                Properties.Settings.Default.ConnectionMaster = "Integrated Security=SSPI;Persist Security Info=False;Data Source=.";
                Properties.Settings.Default.Connection = "Integrated Security=SSPI;Persist Security Info=False;Data Source=.;Initial Catalog=" + DBname;

                Properties.Settings.Default.Server = System.Environment.MachineName;
                Properties.Settings.Default.Save();// for saving connection string into setting.
                con.ConnectionString = Properties.Settings.Default.ConnectionMaster;
                con.Open(); con.Close();
               
                // MessageBox.Show(Properties.Settings.Default.ConnectionMaster);

            }
            catch (Exception)
            {
                //if sql Express = computer_name\SQLEXPRESS
                // master connction we will use for attatch
                Properties.Settings.Default.ConnectionMaster = "Integrated Security=SSPI;Persist Security Info=False;Data Source=" + System.Environment.MachineName + @"\SQLEXPRESS";

                //dataBase Connection
                Properties.Settings.Default.Connection = "Integrated Security=SSPI;Persist Security Info=False;Data Source=" + System.Environment.MachineName + @"\SQLEXPRESS;Initial Catalog=" + DBname;
                Properties.Settings.Default.Server = System.Environment.MachineName + @"\SQLEXPRESS";

                Properties.Settings.Default.Save();// for saving connection string into setting.              
                con.ConnectionString = Properties.Settings.Default.ConnectionMaster;
                con.Open(); con.Close();
               
                // it is not important its for programmer to know which connection valid
                //MessageBox.Show(Properties.Settings.Default.ConnectionMaster);
            }
        }

        /// <summary>
        /// to Attatch DB to Server
        /// </summary>       
        public void attatchDB()
        {
            //attatch DB  
            string DBname = DBfileI.Name.Replace(".mdf", "");
            string q = @"CREATE DATABASE [" + DBname + @"] ON 
( FILENAME = N'" + DBfileI.FullName + @"' ),
( FILENAME = N'" + DBfileI.DirectoryName + @"\" + DBname + @"_log.ldf' )
 FOR ATTACH
";
            SqlConnection con = new SqlConnection();
            con.ConnectionString = Properties.Settings.Default.ConnectionMaster;

            SqlCommand scom = new SqlCommand();
            scom.Connection = con;
            scom.CommandText = q;

            try
            {
                con.Open();
                scom.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                // error Number 1801 in MS SQL Server Mean DB already exist, so we ignore it. 
                if (ex.Number != 1801)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            finally
            {
                con.Close();
            }


        }
        /// <summary>
        /// To Change Authintication Mode from Windows Authintication To SQL Server Authintication.
        /// </summary>
        public void Change_SQL_Server_authentication()
        {
            //create  login 
            _con = new SqlConnection();
            _con.ConnectionString = Properties.Settings.Default.ConnectionMaster;
            _sql_com = new SqlCommand();
            _sql_com.Connection = _con;

            _sql_com.CommandText = @"EXEC xp_instance_regwrite N'HKEY_LOCAL_MACHINE', 
    N'Software\Microsoft\MSSQLServer\MSSQLServer', N'LoginMode', REG_DWORD, 2";
            try
            {
                _con.Open();
                _sql_com.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                _con.Close();
            }
        }
        /// <summary>
        /// create User log in to Sever instance
        /// </summary>      
        public void createLogIn()
        {
            //create  login 
            _con = new SqlConnection();
            _con.ConnectionString = Properties.Settings.Default.ConnectionMaster;
            _sql_com = new SqlCommand();
            _sql_com.Connection = _con;

            _sql_com.CommandText = @"use [master]
create login user1 with password =N'P@$$w0rd',default_database =[master]";
            try
            {
                _con.Open();
                _sql_com.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                // error Number 15025 in MS SQL Server Mean UserName already existed, so we alter USER to change password. 
                if (ex.Number == 15025)                
                {
                    _sql_com.CommandText = @"use [master]
alter login user1 with password =N'P@$$w0rd',default_database =[master]";
                    _sql_com.ExecuteNonQuery();
                }
                
                else
                    MessageBox.Show(ex.Message);
            }
            finally
            {
                _con.Close();
            }
        }

        /// <summary>
        /// create user for database
        /// </summary>
        /// <param name="DBfileI">ur DB DBfileInfo</param>
        public void createUser()
        {
            string DBname = DBfileI.Name.Replace(".mdf", "");
            _con = new SqlConnection();
            _con.ConnectionString = Properties.Settings.Default.ConnectionMaster;

            _sql_com = new SqlCommand();
            _sql_com.Connection  = _con;
            _sql_com.CommandText = @"
USE [" + DBname + @"]

CREATE USER [user1] FOR LOGIN [user1] ";
            try
            {
                _con.Open();
                _sql_com.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                if (ex.Number == 15023)// already exist ,no proplem
                {
                    _sql_com.CommandText = @"


USE [" + DBname + @"]

drop USER [user1] ";
                    _sql_com.ExecuteNonQuery();
                    _con.Close();
                    createUser();
                    return;
                }
                else if (ex.Number == 15007)//user not exist , we will create user.
                {
                    createLogIn();
                    _con.Close();
                    return;
                }
                else//any other Exception
                {
                    MessageBox.Show(ex.Message);
                }
            }
            finally
            {
                _con.Close();
            }

        }

        /// <summary>
        /// create permissions/ROLEs for user
        /// </summary>       
        public void createPermission()
        {
            string DBname = DBfileI.Name.Replace(".mdf", "");
            SqlConnection con = new SqlConnection();
            con.ConnectionString = Properties.Settings.Default.ConnectionMaster;

            SqlCommand scom = new SqlCommand();
            scom.Connection = con;
            //create  login and permission
            scom.CommandText = @"
USE [" + DBname + @"]

ALTER ROLE [db_accessadmin] ADD MEMBER [user1]

USE [" + DBname + @"]

ALTER ROLE [db_datareader] ADD MEMBER [user1]

USE [" + DBname + @"]

ALTER ROLE [db_datawriter] ADD MEMBER [user1]

USE [" + DBname + @"]

ALTER ROLE [db_owner] ADD MEMBER [user1]
";

            try
            {
                con.Open();
                scom.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                if (ex.Number == 15151)// user not exist , we will create user.
                {
                    createUser(); return;
                }
                else//any other Exception
                {
                    MessageBox.Show(ex.Message);
                }
            }
            finally
            {
                con.Close();
            }

        }

        /// <summary>
        /// finally create txt file for connection
        /// </summary>        
        public void GenerateConnectionTxT()
        {
            // to ctreate connection txt
            string DBname = DBfileI.Name.Replace(".mdf", "");
            using (StreamWriter str = new StreamWriter(DBfileI.Directory + "/" + DBname + "Connection.txt", false))
            {
                str.WriteLine("Password=P@$$w0rd;Persist Security Info=True;User ID=user1;Initial Catalog=" + DBname + ";Data Source=" + Properties.Settings.Default.Server + "");
            }           
        }        

        /// <summary>
        /// create user for database
        /// </summary>        
        public  void SetArabicLang()
        {
            string DBname = DBfileI.Name.Replace(".mdf", "");
            SqlConnection con = new SqlConnection();
            con.ConnectionString = Properties.Settings.Default.ConnectionMaster;

            SqlCommand scom = new SqlCommand();
            scom.Connection = con;
            scom.CommandText = @"
alter database [" + DBname + @"] set single_user with rollback immediate
USE [master]
ALTER DATABASE [" + DBname + @"] COLLATE Arabic_BIN
alter database [" + DBname + @"] set multi_user with rollback immediate
USE [master]";
            try
            {
                con.Open();
                scom.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                if (ex.Number == 15007)//user not exist , we will create user.
                {
                    createLogIn();
                    con.Close(); return;
                }
                else//any other Exception
                {
                    MessageBox.Show(ex.Message);
                }
            }
            finally
            {
                con.Close();
            }

        }
    }
}
