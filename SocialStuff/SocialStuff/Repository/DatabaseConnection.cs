﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace SocialStuff.Repository
{
    internal class DatabaseConnection
    {
        string connectionString = @"Data Source=RAZVAN\SQLEXPRESS01;Initial Catalog=BankingDB;Integrated Security=True;TrustServerCertificate=True";


        private SqlConnection conn;

        public DatabaseConnection()
        {
            conn = new SqlConnection(connectionString);
        }

        public void OpenConnection()
        {
            if (conn.State == System.Data.ConnectionState.Closed)
            {
                conn.Open();
                Console.WriteLine("Database Connected!");
                
            }
        }

        public int CheckConnection()
        {
            if (conn.State == System.Data.ConnectionState.Open)
            {
                Console.WriteLine("Database Connection is Open!");
                // print something on the screen
                return 1;
            }
            else
            {
                Console.WriteLine("Database Connection is Closed!");
                // print something on the screen
                return 0;
            }

        }
        public void CloseConnection()
        {
            if (conn.State == System.Data.ConnectionState.Open)
            {
                conn.Close();
                Console.WriteLine("Database Connection Closed!");
                // print something on the screen

            }
        }

    }


}
