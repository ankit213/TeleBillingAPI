using System.Data;
using System.Collections;
using System;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;

namespace TeleBillingUtility.Helpers
{
    public class DALMySql 
    {
		public readonly string _ConnString;
	
		public DALMySql()
		{
			var builder = new ConfigurationBuilder()
					.AddJsonFile("appsettings.json", true, true);

			IConfigurationRoot configuration = builder.Build();

			_ConnString = configuration.GetConnectionString("DefaultConnection");
		}

       
        public DataSet GetDataSet(string sSQL, SortedList paramList)
        {	
            MySqlConnection myConnection = new MySqlConnection(_ConnString);
            MySqlCommand cmd = new MySqlCommand(sSQL, myConnection);
            int x = 0;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = sSQL;
            cmd.Connection = myConnection;
            for (x = 0; x <= paramList.Count - 1; x++)
            {
                //cmd.Parameters.Add(paramList.GetKey(x), paramList.GetByIndex(x));
                cmd.Parameters.AddWithValue((String)paramList.GetKey(x), paramList.GetByIndex(x));
            }

            MySqlDataAdapter myAdapter = default(MySqlDataAdapter);
            myAdapter = new MySqlDataAdapter(cmd);

            DataSet result = new DataSet();
            try
            {
                myAdapter.Fill(result);
            }
            catch (Exception ex)
            {
                //ErrorHandler.WriteError(Convert.ToString(ex));
                return result;
            }
            // Return the datareader result
            return result;
        }

        public DataSet GetDataSet(string sSQL)
        {
            // Create Instance of Connection

            MySqlConnection myConnection = new MySqlConnection(_ConnString);
            MySqlCommand cmd = new MySqlCommand(sSQL, myConnection);
            int x = 0;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = sSQL;
            cmd.Connection = myConnection;
            MySqlDataAdapter myAdapter = default(MySqlDataAdapter);
            myAdapter = new MySqlDataAdapter(cmd);

            DataSet result = new DataSet();
            try
            {
                myAdapter.Fill(result);
            }
            catch (Exception ex)
            {
                //ErrorHandler.WriteError(Convert.ToString(ex));
                return result;
            }
            // Return the datareader result
            return result;
        }


		public object ExecuteScaler(string sSQL, SortedList paramList)
		{

			// Create Instance of Connection and Command Object
			MySqlConnection myConnection = new MySqlConnection(_ConnString);
			MySqlCommand cmd = new MySqlCommand(sSQL, myConnection);
			object result = null;
			Int16 x = default(Int16);
			// Execute the command
			try
			{
				cmd.CommandType = CommandType.StoredProcedure;
				for (x = 0; x <= paramList.Count - 1; x++)
				{
					//cmd.Parameters.Add(paramList.GetKey(x), paramList.GetByIndex(x));
					cmd.Parameters.AddWithValue((String)paramList.GetKey(x), paramList.GetByIndex(x));
				}
				myConnection.Open();
				result = cmd.ExecuteScalar();
				myConnection.Close();
			}
			catch (Exception ex)
			{
				try
				{
					if (myConnection.State == ConnectionState.Open)
					{
						myConnection.Close();
					}
				}
				catch (Exception ex2)
				{
					//ErrorHandler.WriteError(Convert.ToString(ex2));
					return null;
				}
				//ErrorHandler.WriteError(Convert.ToString(ex));
			}
			// Return the result
			return result;
		}
	}
}
