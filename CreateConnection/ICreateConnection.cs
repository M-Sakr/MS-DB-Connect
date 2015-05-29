using System;
namespace CreateConnectionApp
{
    interface ICreateConnection
    {

        void attatchDB();
        void Change_SQL_Server_authentication();
        void Connection();
        void createLogIn();
        void createPermission();
        void createUser();        
        void GenerateConnectionTxT();
        void SetArabicLang();
    }
}
