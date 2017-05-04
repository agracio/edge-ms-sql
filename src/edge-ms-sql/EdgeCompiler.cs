using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

public class EdgeCompiler
{
    public Func<object, Task<object>> CompileFunc(IDictionary<string, object> parameters)
    {
        string command = ((string)parameters["source"]).TrimStart();
        string connectionString = Environment.GetEnvironmentVariable("EDGE_SQL_CONNECTION_STRING");
        int? commandTimeout = null;

        object connectionStringTmp;
        if (parameters.TryGetValue("connectionString", out connectionStringTmp))
        {
            connectionString = (string)connectionStringTmp;
        }

        object commandTimeoutTmp;
        if (parameters.TryGetValue("commandTimeout", out commandTimeoutTmp))
        {
            commandTimeout = (int)commandTimeoutTmp;
        }

        if (command.StartsWith("select ", StringComparison.InvariantCultureIgnoreCase))
        {
            return async (queryParameters) => await 
            ExecuteQuery(connectionString, 
            command, 
            (IDictionary<string, object>)queryParameters, 
            commandTimeout);
        }
        if (command.StartsWith("insert ", StringComparison.InvariantCultureIgnoreCase)
            || command.StartsWith("update ", StringComparison.InvariantCultureIgnoreCase)
            || command.StartsWith("delete ", StringComparison.InvariantCultureIgnoreCase))
        {
            return async (queryParameters) => await 
            ExecuteNonQuery(
                connectionString, 
                command, (IDictionary<string, object>)queryParameters, 
                commandTimeout);
        }
        if (command.StartsWith("exec ", StringComparison.InvariantCultureIgnoreCase))
        {
            return async (queryParameters) => await
                ExecuteStoredProcedure(
                    connectionString,
                    command,
                    (IDictionary<string, object>)queryParameters,
                    commandTimeout);
        }
        throw new InvalidOperationException("Unsupported type of SQL command. Only select, insert, update, delete, and exec are supported.");
    }

    void AddParamaters(
        SqlCommand command, 
        IDictionary<string, object> parameters)
    {
        if (parameters != null)
        {
            foreach (KeyValuePair<string, object> parameter in parameters)
            {
                command.Parameters.AddWithValue(parameter.Key, parameter.Value ?? DBNull.Value);
            }
        }
    }

    async Task<object> ExecuteQuery(
        string connectionString, 
        string commandString, 
        IDictionary<string, object> parameters, 
        int? commandTimeout = null)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            using (var command = new SqlCommand(commandString, connection))
            {
                if (commandTimeout.HasValue) command.CommandTimeout = commandTimeout.Value;
                return await ExecuteQuery(parameters, command, connection);
            }
        }
    }

    async Task<object> ExecuteQuery(
        IDictionary<string, object> parameters, 
        SqlCommand command, 
        SqlConnection connection)
    {
        List<object> rows = new List<object>();
        AddParamaters(command, parameters);
        await connection.OpenAsync();
        using (SqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
        {
            IDataRecord record = reader;
            while (await reader.ReadAsync())
            {
                var dataObject = new ExpandoObject() as IDictionary<string, object>;
                var resultRecord = new object[record.FieldCount];
                record.GetValues(resultRecord);

                for (int i = 0; i < record.FieldCount; i++)
                {      
                    Type type = record.GetFieldType(i);
                    if (resultRecord[i] is DBNull)
                    {
                        resultRecord[i] = null;
                    }
                    else if (type == typeof(byte[]) || type == typeof(char[]))
                    {
                        resultRecord[i] = Convert.ToBase64String((byte[])resultRecord[i]);
                    }
                    else if (type == typeof(Guid) || type == typeof(DateTime))
                    {
                        resultRecord[i] = resultRecord[i].ToString();
                    }
                    else if (type == typeof(IDataReader))
                    {
                        resultRecord[i] = "<IDataReader>";
                    }

                    dataObject.Add(record.GetName(i), resultRecord[i]);
                }

                rows.Add(dataObject);
            }

            return rows;
        }
    }

    async Task<object> ExecuteNonQuery(
        string connectionString, 
        string commandString, 
        IDictionary<string, object> parameters, 
        int? commandTimeout = null)
    {
        using (var connection = new SqlConnection(connectionString))
        {
            using (var command = new SqlCommand(commandString, connection))
            {
                if (commandTimeout.HasValue) command.CommandTimeout = commandTimeout.Value;
                AddParamaters(command, parameters);
                await connection.OpenAsync();
                return await command.ExecuteNonQueryAsync();
            }
        }
    }

    async Task<object> ExecuteStoredProcedure(
        string connectionString,
        string commandString,
        IDictionary<string, object> parameters,
        int? commandTimeout = null)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            SqlCommand command = new SqlCommand(commandString.Substring(5).TrimEnd(), connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            if (commandTimeout.HasValue) command.CommandTimeout = commandTimeout.Value;
            using (command)
            {
                return await ExecuteQuery(parameters, command, connection);
            }
        }
    }
}
