using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;


namespace TeleBillingUtility.Helpers
{
    public class DAL
    {
        public readonly string _ConnString;
        public DAL()
        {
            var builder = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", true, true);

            IConfigurationRoot configuration = builder.Build();

            _ConnString = configuration.GetConnectionString("DefaultConnection");
        }
        public SqlDataReader GetDataReader(string sSQL)
        {
            // Create Instance of Connection and Command Object
            SqlConnection myConnection = new SqlConnection(_ConnString);
            SqlCommand cmd = new SqlCommand(sSQL, myConnection);
            SqlDataReader result = default(SqlDataReader);

            // Execute the command
            try
            {
                cmd.CommandType = CommandType.StoredProcedure;
                myConnection.Open();
                result = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (Exception)
            {
                //ErrorHandler.WriteError(Convert.ToString(ex));
                return null;
            }

            // Return the datareader result
            return result;
        }

        public SqlDataReader GetDataReader(string sSQL, SortedList paramList)
        {
            // Create Instance of Connection and Command Object
            SqlConnection myConnection = new SqlConnection(_ConnString);
            SqlCommand cmd = new SqlCommand(sSQL, myConnection);
            SqlDataReader result = default(SqlDataReader);
            int x = 0;
            // Execute the command
            try
            {
                cmd.CommandType = CommandType.StoredProcedure;
                for (x = 0; x <= paramList.Count - 1; x++)
                {
                    cmd.Parameters.AddWithValue((String)paramList.GetKey(x), paramList.GetByIndex(x));
                }
                myConnection.Open();
                result = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (Exception ex)
            {
                //ErrorHandler.WriteError(Convert.ToString(ex));
                return null;
            }

            // Return the datareader result
            return result;
        }

        public DataSet GetDataSet(string sSQL, SortedList paramList)
        {
            // Create Instance of Connection

            SqlConnection myConnection = new SqlConnection(_ConnString);
            SqlCommand cmd = new SqlCommand(sSQL, myConnection);
            int x = 0;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = sSQL;
            cmd.Connection = myConnection;
            for (x = 0; x <= paramList.Count - 1; x++)
            {
                //cmd.Parameters.Add(paramList.GetKey(x), paramList.GetByIndex(x));
                cmd.Parameters.AddWithValue((String)paramList.GetKey(x), paramList.GetByIndex(x));
            }
            SqlDataAdapter myAdapter = default(SqlDataAdapter);
            myAdapter = new SqlDataAdapter(cmd);

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

            SqlConnection myConnection = new SqlConnection(_ConnString);
            SqlDataAdapter myAdapter = new SqlDataAdapter(sSQL, myConnection);
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

        public int ExecuteSQL(string sSQL)
        {
            // Create Instance of Connection and Command Object
            SqlConnection myConnection = new SqlConnection(_ConnString);
            SqlCommand cmd = new SqlCommand(sSQL, myConnection);
            int result = 0;

            // Execute the command
            try
            {
                cmd.CommandType = CommandType.Text;
                myConnection.Open();
                result = cmd.ExecuteNonQuery();
                myConnection.Close();
            }
            catch (Exception ex)
            {
                result = -1;
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
                }
                //ErrorHandler.WriteError(Convert.ToString(ex));
            }
            // Return the result
            return result;
        }

        public int ExecuteSQL(string sSQL, SortedList paramList)
        {
            // Create Instance of Connection and Command Object
            SqlConnection myConnection = new SqlConnection(_ConnString);
            SqlCommand cmd = new SqlCommand(sSQL, myConnection);
            int result = 0;
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
                result = cmd.ExecuteNonQuery();
                myConnection.Close();
            }
            catch (Exception ex)
            {
                result = -1;
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
                }
                //ErrorHandler.WriteError(Convert.ToString(ex));
            }
            // Return the result
            return result;
        }

        public int ExecuteSQL(string sSQL, SortedList paramList, SqlConnection myConnection, SqlTransaction myTransaction)
        {
            // Create Instance of Connection and Command Object
            //SqlConnection myConnection = new SqlConnection(_ConnString);
            SqlCommand cmd = new SqlCommand(sSQL, myConnection, myTransaction);
            int result = 0;
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
                //myConnection.Open();
                result = cmd.ExecuteNonQuery();
                //myConnection.Close();
            }
            catch (Exception ex)
            {
                result = -1;
                try
                {
                    if (myConnection.State == ConnectionState.Open)
                    {
                        //myConnection.Close();
                    }
                }
                catch (Exception ex2)
                {
                    //ErrorHandler.WriteError(Convert.ToString(ex2));
                }
                //ErrorHandler.WriteError(Convert.ToString(ex));
            }
            // Return the result
            return result;
        }



        public object ExecuteScaler(string sSQL)
        {

            // Create Instance of Connection and Command Object
            SqlConnection myConnection = new SqlConnection(_ConnString);
            SqlCommand cmd = new SqlCommand(sSQL, myConnection);
            object result = null;

            // Execute the command
            try
            {
                cmd.CommandType = CommandType.StoredProcedure;
                myConnection.Open();
                result = cmd.ExecuteScalar();
                myConnection.Close();
            }
            catch (Exception ex)
            {
                //ErrorHandler.WriteError(Convert.ToString(ex));
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

        public object ExecuteScaler(string sSQL, String type)
        {

            // Create Instance of Connection and Command Object
            SqlConnection myConnection = new SqlConnection(_ConnString);
            SqlCommand cmd = new SqlCommand(sSQL, myConnection);
            object result = null;

            // Execute the command
            try
            {
                cmd.CommandType = CommandType.Text;
                myConnection.Open();
                result = cmd.ExecuteScalar();
                myConnection.Close();
            }
            catch (Exception ex)
            {
                //ErrorHandler.WriteError(Convert.ToString(ex));
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

        public object ExecuteScaler(string sSQL, SortedList paramList)
        {

            // Create Instance of Connection and Command Object
            SqlConnection myConnection = new SqlConnection(_ConnString);
            SqlCommand cmd = new SqlCommand(sSQL, myConnection);
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

        public DataTable GetDataTable(string SQL, string VAL)
        {
            DataTable dt = new DataTable();
            SqlConnection myConnection = new SqlConnection(_ConnString);
            SqlCommand cmd = new SqlCommand(SQL, myConnection);
            try
            {
                if (VAL == "TEXT")
                {
                    cmd.Connection = myConnection;
                    cmd.CommandText = SQL;
                    cmd.CommandType = CommandType.Text;
                    myConnection.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                    myConnection.Close();
                    da.Dispose();
                }
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
                    return dt;
                }
                //ErrorHandler.WriteError(Convert.ToString(ex));
            }
            return dt;
        }

        public DataTable GetDataTable(string sp)
        {
            DataTable dt = new DataTable();
            SqlConnection myConnection = new SqlConnection(_ConnString);
            SqlCommand cmd = new SqlCommand(sp, myConnection);
            try
            {
                cmd.Connection = myConnection;
                cmd.CommandText = sp;
                cmd.CommandType = CommandType.StoredProcedure;
                myConnection.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                myConnection.Close();
                da.Dispose();

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
                    return dt;
                }
                //ErrorHandler.WriteError(Convert.ToString(ex));
            }
            return dt;
        }

        public DataTable GetDataTableSchema(string sp)
        {
            SqlConnection myConnection = new SqlConnection(_ConnString);
            SqlCommand cmd = new SqlCommand(sp, myConnection);
            DataTable dt = new DataTable();
            try
            {
                cmd.Connection = myConnection;
                cmd.CommandText = sp;
                cmd.CommandType = CommandType.StoredProcedure;
                myConnection.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.FillSchema(dt, SchemaType.Source);
                myConnection.Close();
                da.Dispose();
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
                    return dt;
                }
                //ErrorHandler.WriteError(Convert.ToString(ex));
            }
            return dt;
        }

        public int DataTableUpdate(string sSQL, DataTable dtUpdate)
        {
            SqlDataAdapter adpt = default(SqlDataAdapter);
            SqlCommand cmd = default(SqlCommand);
            int result = 0;
            SqlConnection myConnection = new SqlConnection(_ConnString);
            //
            try
            {

                cmd = new SqlCommand(sSQL, myConnection);
                adpt = new SqlDataAdapter(cmd);
                adpt.RowUpdated += OnRowUpdated;
                SqlCommandBuilder custCB = new SqlCommandBuilder(adpt);
                result = adpt.Update(dtUpdate);
                return result;
            }
            catch (Exception ex)
            {
                //websurveytool.ErrHandler.WriteError(ex.ToString());
                return -1;
            }
        }

        private void OnRowUpdated(object sender, SqlRowUpdatedEventArgs args)
        {
            System.Exception exp = default(System.Exception);
            exp = args.Errors;
            try
            {
                if ((exp != null))
                {
                    args.Status = UpdateStatus.Continue;
                }
                //Else
                // If fieldName <> "" Then
                // If affectedLeadId <> "" Then
                // affectedLeadId = affectedLeadId & "," & CStr(args.Row.Item(fieldName))
                // Else
                // affectedLeadId = CStr(args.Row.Item(fieldName))
                // End If
                // End If
            }
            catch (Exception ex)
            {


            }
        }

        public DataTable GetDataTable(string sp, SortedList paramList)
        {

            SqlConnection myConnection = new SqlConnection(_ConnString);
            SqlCommand cmd = new SqlCommand(sp, myConnection);
            DataTable dt = new DataTable();
            Int16 x = default(Int16);
            try
            {
                cmd.Connection = myConnection;
                cmd.CommandText = sp;
                cmd.CommandType = CommandType.StoredProcedure;
                for (x = 0; x <= paramList.Count - 1; x++)
                {
                    //cmd.Parameters.Add(paramList.GetKey(x), paramList.GetByIndex(x));
                    cmd.Parameters.AddWithValue((String)paramList.GetKey(x), paramList.GetByIndex(x));
                }
                myConnection.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                myConnection.Close();
                da.Dispose();

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
                    return dt;
                }
                //ErrorHandler.WriteError(Convert.ToString(ex));
            }
            return dt;
        }


        public string FormatXml(string input)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(input);
            using (StringWriter buffer = new StringWriter())
            {
                XmlWriterSettings settings = new XmlWriterSettings()
                {
                    Indent = true
                };
                using (XmlWriter writer = XmlWriter.Create(buffer, settings))
                {
                    doc.WriteTo(writer);
                    writer.Flush();
                }
                buffer.Flush();
                return buffer.ToString();
            }
        }


        public int ExecuteSQL(string sSQL, List<SqlParameter> paramList)
        {
            // Create Instance of Connection and Command Object
            SqlConnection myConnection = new SqlConnection(_ConnString);
            SqlCommand cmd = new SqlCommand(sSQL, myConnection);
            int result = 0;
            // Execute the command
            try
            {
                cmd.CommandType = CommandType.StoredProcedure;

                SqlParameter[] sqlparam = new SqlParameter[paramList.Count];
                paramList.CopyTo(sqlparam);
                cmd.Parameters.AddRange(sqlparam);

                myConnection.Open();
                result = cmd.ExecuteNonQuery();
                myConnection.Close();
            }
            catch (Exception ex)
            {
                result = -1;
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
                }
                //ErrorHandler.WriteError(Convert.ToString(ex));
            }
            // Return the result
            return result;
        }

        public object ExecuteScaler(string sSQL, List<SqlParameter> paramList)
        {

            // Create Instance of Connection and Command Object
            SqlConnection myConnection = new SqlConnection(_ConnString);
            SqlCommand cmd = new SqlCommand(sSQL, myConnection);
            object result = null;
            // Execute the command
            try
            {
                cmd.CommandType = CommandType.StoredProcedure;

                SqlParameter[] sqlparam = new SqlParameter[paramList.Count];
                paramList.CopyTo(sqlparam);
                cmd.Parameters.AddRange(sqlparam);

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


        public DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name);
            }
            if (items != null)
            {
                foreach (T item in items)
                {
                    var values = new object[Props.Length];
                    for (int i = 0; i < Props.Length; i++)
                    {
                        //inserting property values to datatable rows
                        values[i] = Props[i].GetValue(item, null);
                    }
                    dataTable.Rows.Add(values);
                }
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }

        public List<T> ConvertDataTableToGenericList<T>(DataTable dt)
        {
            var columnNames = dt.Columns.Cast<DataColumn>()
                   .Select(c => c.ColumnName)
                   .ToList();

            var properties = typeof(T).GetProperties();
            DataRow[] rows = dt.Select();
            return rows.Select(row =>
            {
                var objT = Activator.CreateInstance<T>();
                foreach (var pro in properties)
                {
                    if (columnNames.Contains(pro.Name))
                        pro.SetValue(objT, row[pro.Name]);
                }

                return objT;
            }).ToList();
        }
    }
}
