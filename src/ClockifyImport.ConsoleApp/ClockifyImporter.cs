using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using SQLitePCL;

namespace ClockifyImport.ConsoleApp
{
    public static class ClockifyImporter
    {
        public static void ProcessFile(string sourceFolder, string filePath, string configPrefix)
        {
            try
            {
                int errorCount = 0;
                var lines = File.ReadAllLines(filePath);
                var linesList = lines.ToList<string>();
                // get rid of header row
                linesList.RemoveAt(0);

                string delimiter = Core.GetConfigValue("Delimiter");
                string connectionString = Core.GetConnectionString("ConnectionString", configPrefix);
                using (SqliteConnection conn = new SqliteConnection(connectionString))
                {
                    try
                    {
                        string columnsToImport = Core.GetConfigValue("ImportColumns", configPrefix);
                        string[] columnNames = columnsToImport.Split(delimiter);
                        string columnDataTypes = Core.GetConfigValue("ColumnDataTypes", configPrefix);
                        string[] columnTypes = columnDataTypes.Split(delimiter);
                        int expectedColumnCount = columnNames.Length;
                        string tableName = Core.GetConfigValue("InsertTableName", configPrefix);

                        conn.Open();
                        Core.UserMessage("Connection opened successfully!", false);
                        int lineNum = 0;

                        foreach (string line in linesList)
                        {
                            lineNum++;
                            string[] values = line.Split(delimiter);
                            if (values.Length == expectedColumnCount)
                            {
                                try
                                {
                                    SqliteCommand cmd = new SqliteCommand();
                                    cmd.Parameters.Add("@Id", SqliteType.Text);
                                    cmd.Parameters["@Id"].Value = Guid.NewGuid().ToString();
                                    cmd.Parameters.Add("@DateRecorded", SqliteType.Integer);
                                    cmd.Parameters["@DateRecorded"].Value = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds();
                                    cmd.Parameters.Add("@FilePath", SqliteType.Text);
                                    cmd.Parameters["@FilePath"].Value = filePath;
                                    cmd.Parameters.Add("@LineNum", SqliteType.Integer);
                                    cmd.Parameters["@LineNum"].Value = lineNum;
                                    string cmdText = "insert into " + tableName + " (Id,FilePath,LineNum,DateRecorded, ";
                                    foreach (string col in columnNames)
                                    {
                                        cmdText += "`" + col + "`,";
                                    }
                                    cmdText = cmdText.TrimEnd(',') + ") values (@Id,@FilePath,@LineNum,@DateRecorded,";
                                    StringBuilder insertValueArray = new StringBuilder();
                                    for (int i = 0; i < columnNames.Length; i++)
                                    {
                                        cmdText += "@" + columnNames[i] + ",";
                                        cmd.Parameters.Add("@" + columnNames[i], Enum.Parse<SqliteType>(columnTypes[i]));
                                        cmd.Parameters["@" + columnNames[i]].Value = values[i].TrimStart('"').TrimEnd('"');
                                    }
                                    cmdText = cmdText.TrimEnd(',') + ")";
                                    cmd.CommandText = cmdText;
                                    cmd.Connection = conn;
                                    cmd.ExecuteNonQuery();
                                    Core.UserMessage("Line " + lineNum + " inserted successfully!", false);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine();
                                    Core.UserMessage("ERROR for line: [" + line + "]\r\n" + ex.Message);
                                    errorCount++;
                                }
                            }
                            else
                            {
                                Core.UserMessage("Found incomplete row in import file \"" + filePath + "\", received [" + line + "]", false);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Core.UserMessage(ex.Message);
                    }
                    finally
                    {
                        if (conn.State != System.Data.ConnectionState.Closed)
                        {
                            conn.Close();
                            Core.UserMessage("Connection to SQLite database closed", false);
                        }
                    }

                    bool moveFile = true;
                    if (errorCount>0)
                    {
                        Console.WriteLine("There " + (errorCount == 1 ? " was 1 error" : " were " + errorCount + " errors") + " uploading data. Move to Processed folder anyway? (Y/N)");
                        string errorResp = Console.ReadLine()!;
                        if (!errorResp.Equals("y", StringComparison.OrdinalIgnoreCase))
                        {
                            moveFile = false;
                        }
                    }

                    if (moveFile)
                    {
                        // Move file to processed folder
                        try
                        {
                            string destPath = Path.Join(sourceFolder, "Processed");
                            Directory.CreateDirectory(destPath);
                            File.Move(filePath, Path.Join(destPath, Path.GetFileName(filePath)));
                        }
                        catch (Exception ex)
                        {
                            Core.UserMessage(ex.Message);
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                Core.UserMessage(ex.Message);
            }
        }

    }
}
