using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace GksKatowiceBot.Helpers
{
    public class BaseDB
    {
        public static void AddToLog(string action)
        {
            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;

                cmd.CommandText = "INSERT INTO LogPolsatSport (Tresc) VALUES ('" + action + " " + DateTime.Now.ToString() + "')";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;

                sqlConnection1.Open();
                cmd.ExecuteNonQuery();

                sqlConnection1.Close();
            }
            catch (Exception ex)
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;

                cmd.CommandText = "INSERT INTO LogPolsatSport (Tresc) VALUES ('" + "Błąd dodawania wiadomosci do Loga" + " " + DateTime.Now.ToString() + "')";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;

                sqlConnection1.Open();
                cmd.ExecuteNonQuery();

                sqlConnection1.Close();
            }
        }
        public static void AddUser(string UserName, string UserId, string BotName, string BotId, string Url, byte flgTyp)
        {
            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;

                cmd.CommandText = "IF NOT EXISTS(Select * from [dbo].[UserPolsatSport] where UserId='" + UserId + "')BEGIN INSERT INTO [dbo].[UserPolsatSport] (UserName,UserId,BotName,BotId,Url,flgPlusLiga,DataUtw,flgDeleted) VALUES ('" + UserName + "','" + UserId + "','" + BotName + "','" + BotId + "','" + Url + "','" + flgTyp.ToString() + "','" + DateTime.Now + "','0')END";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;

                sqlConnection1.Open();
                cmd.ExecuteNonQuery();

                sqlConnection1.Close();
            }
            catch (Exception ex)
            {
                AddToLog("Blad dodawania uzytkownika "+ex.ToString());
            }
        }
        public static object czyAdministrator(string UserId)
        {
            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;

                cmd.CommandText = "sprawdzCzyAdministratorPolsatSport";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@userId", UserId);
                cmd.Connection = sqlConnection1;

                sqlConnection1.Open();
                var rowsAffected = cmd.ExecuteScalar();

                sqlConnection1.Close();

                return rowsAffected;
            }
            catch (Exception ex)
            {
                AddToLog("Blad sprawdzania uzytkownika czy admnistrator "+ex.ToString());
                return null;
            }
        }

        public static byte czyPowiadomienia(string UserId)
        {
            byte returnValue = 0;
            try
            {                
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;

                cmd.CommandText = "sprawdzCzyPowiadomieniaPolsatSport";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@userId", UserId);
                cmd.Connection = sqlConnection1;

                sqlConnection1.Open();
                var rowsAffected = cmd.ExecuteScalar();

                sqlConnection1.Close();

                if (rowsAffected!=null)
                {
                    returnValue = 1;
                }
                else
                {
                    returnValue = 0;
                }
                return returnValue;
            }
            catch (Exception ex)
            {
                AddToLog("Blad sprawdzania uzytkownika czy admnistrator " + ex.ToString());
                return returnValue;
            }
        }

        public static void DeleteUser(string UserId)
        {
            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;

                cmd.CommandText = "Delete [dbo].[UserPolsatSport] where UserId='" + UserId + "'";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;

                sqlConnection1.Open();
                cmd.ExecuteNonQuery();

                sqlConnection1.Close();
            }
            catch
            {
                AddToLog("Blad usuwania uzytkownika: " + UserId);
            }
        }
        public static void AddWiadomoscNajwazniejsze(System.Collections.Generic.List<string> hrefList)
        {
            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;

                

                sqlConnection1.Open();

                cmd.CommandText = "Delete from [dbo].[WiadomosciPSNajwazniejsze]";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;
                cmd.ExecuteNonQuery();

                cmd.CommandText = "INSERT INTO [dbo].[WiadomosciPSNajwazniejsze]  (Nazwa,DataUtw,Wiadomosc1,Wiadomosc2,Wiadomosc3,Wiadomosc4,Wiadomosc5,Wiadomosc6,Wiadomosc7,Wiadomosc8,Wiadomosc9,Wiadomosc10) VALUES ('" + "" + "','" + DateTime.Now + "','" + hrefList[0] + "','" + hrefList[1] + "','" + hrefList[2] + "','" + hrefList[3] + "','" + hrefList[4] + "','" + hrefList[5] + "','" + hrefList[6] + "','" + hrefList[7] + "','" + hrefList[8] + "','" + hrefList[9] + "')";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;
                cmd.ExecuteNonQuery();

                sqlConnection1.Close();
            }
            catch (Exception ex)
            {
                AddToLog("Błąd dodawania wiadomości: " + ex.ToString());
            }
        }
        public static void AddWiadomoscNajwazniejszeImg(System.Collections.Generic.List<string> hrefList)
        {
            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;



                sqlConnection1.Open();

                cmd.CommandText = "Delete from [dbo].[WiadomosciPSNajwazniejszeImg]";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;
                cmd.ExecuteNonQuery();

                cmd.CommandText = "INSERT INTO [dbo].[WiadomosciPSNajwazniejszeImg] (Nazwa,DataUtw,Wiadomosc1,Wiadomosc2,Wiadomosc3,Wiadomosc4,Wiadomosc5,Wiadomosc6,Wiadomosc7,Wiadomosc8,Wiadomosc9,Wiadomosc10) VALUES ('" + "" + "','" + DateTime.Now + "','" + hrefList[0] + "','" + hrefList[1] + "','" + hrefList[2] + "','" + hrefList[3] + "','" + hrefList[4] + "','" + hrefList[5] + "','" + hrefList[6] + "','" + hrefList[7] + "','" + hrefList[8] + "','" + hrefList[9] + "')";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;
                cmd.ExecuteNonQuery();

                sqlConnection1.Close();
            }
            catch (Exception ex)
            {
                AddToLog("Błąd dodawania wiadomości: " + ex.ToString());
            }
        }

        public static void AddWiadomoscNajwazniejszeTytul(System.Collections.Generic.List<string> hrefList)
        {
            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;



                sqlConnection1.Open();

                cmd.CommandText = "Delete from [dbo].[WiadomosciPSNajwazniejszeTytul]";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;
                cmd.ExecuteNonQuery();

                cmd.CommandText = "INSERT INTO [dbo].[WiadomosciPSNajwazniejszeTytul]  (Nazwa,DataUtw,Wiadomosc1,Wiadomosc2,Wiadomosc3,Wiadomosc4,Wiadomosc5,Wiadomosc6,Wiadomosc7,Wiadomosc8,Wiadomosc9,Wiadomosc10) VALUES ('" + "" + "','" + DateTime.Now + "','" + hrefList[0] + "','" + hrefList[1] + "','" + hrefList[2] + "','" + hrefList[3] + "','" + hrefList[4] + "','" + hrefList[5] + "','" + hrefList[6] + "','" + hrefList[7] + "','" + hrefList[8] + "','" + hrefList[9] + "')";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;
                cmd.ExecuteNonQuery();

                sqlConnection1.Close();
            }
            catch (Exception ex)
            {
                AddToLog("Błąd dodawania wiadomości: " + ex.ToString());
            }
        }

        public static DataTable GetWiadomoscNajwazniejsze()
        {
            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;
                DataTable dataTable = new DataTable();


                sqlConnection1.Open();
                
                cmd.CommandText = "Select Wiadomosc1,Wiadomosc2,Wiadomosc3,Wiadomosc4,Wiadomosc5,Wiadomosc6,Wiadomosc7," +
                    "Wiadomosc8,Wiadomosc9,Wiadomosc10 from [dbo].[WiadomosciPSNajwazniejsze]";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                // this will query your database and return the result to your datatable
                da.Fill(dataTable);
                sqlConnection1.Close();
                da.Dispose();
                return dataTable;
            }
            catch (Exception ex)
            {
                AddToLog("Błąd dodawania wiadomości: " + ex.ToString());
                return null;
            }
        }


        public static DataTable GetWiadomoscNajwazniejszeImg()
        {
            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;
                DataTable dataTable = new DataTable();


                sqlConnection1.Open();

                cmd.CommandText = "Select Wiadomosc1,Wiadomosc2,Wiadomosc3,Wiadomosc4,Wiadomosc5,Wiadomosc6,Wiadomosc7," +
                    "Wiadomosc8,Wiadomosc9,Wiadomosc10 from [dbo].[WiadomosciPSNajwazniejszeImg]";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                // this will query your database and return the result to your datatable
                da.Fill(dataTable);
                sqlConnection1.Close();
                da.Dispose();
                return dataTable;
            }
            catch (Exception ex)
            {
                AddToLog("Błąd dodawania wiadomości: " + ex.ToString());
                return null;
            }
        }

        public static DataTable GetWiadomoscNajwazniejszeTytul()
        {
            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;
                DataTable dataTable = new DataTable();


                sqlConnection1.Open();

                cmd.CommandText = "Select Wiadomosc1,Wiadomosc2,Wiadomosc3,Wiadomosc4,Wiadomosc5,Wiadomosc6,Wiadomosc7," +
                    "Wiadomosc8,Wiadomosc9,Wiadomosc10 from [dbo].[WiadomosciPSNajwazniejszeTytul]";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                // this will query your database and return the result to your datatable
                da.Fill(dataTable);
                sqlConnection1.Close();
                da.Dispose();
                return dataTable;
            }
            catch (Exception ex)
            {
                AddToLog("Błąd dodawania wiadomości: " + ex.ToString());
                return null;
            }
        }


        public static DataTable GetWiadomoscPopularne()
        {
            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;
                DataTable dataTable = new DataTable();


                sqlConnection1.Open();

                cmd.CommandText = "Select Wiadomosc1,Wiadomosc2,Wiadomosc3,Wiadomosc4,Wiadomosc5,Wiadomosc6,Wiadomosc7," +
                    "Wiadomosc8,Wiadomosc9,Wiadomosc10 from [dbo].[WiadomosciPSPopularne]";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                // this will query your database and return the result to your datatable
                da.Fill(dataTable);
                sqlConnection1.Close();
                da.Dispose();
                return dataTable;
            }


            catch (Exception ex)
            {
                AddToLog("Błąd dodawania wiadomości: " + ex.ToString());
                return null;
            }
        }


        public static DataTable GetWiadomoscPopularneImg()
        {
            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;
                DataTable dataTable = new DataTable();


                sqlConnection1.Open();

                cmd.CommandText = "Select Wiadomosc1,Wiadomosc2,Wiadomosc3,Wiadomosc4,Wiadomosc5,Wiadomosc6,Wiadomosc7," +
                    "Wiadomosc8,Wiadomosc9,Wiadomosc10 from [dbo].[WiadomosciPSPopularneImg]";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                // this will query your database and return the result to your datatable
                da.Fill(dataTable);
                sqlConnection1.Close();
                da.Dispose();
                return dataTable;
            }
            catch (Exception ex)
            {
                AddToLog("Błąd dodawania wiadomości: " + ex.ToString());
                return null;
            }
        }

        public static DataTable GetWiadomoscPopularneTytul()
        {
            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;
                DataTable dataTable = new DataTable();


                sqlConnection1.Open();

                cmd.CommandText = "Select Wiadomosc1,Wiadomosc2,Wiadomosc3,Wiadomosc4,Wiadomosc5,Wiadomosc6,Wiadomosc7," +
                    "Wiadomosc8,Wiadomosc9,Wiadomosc10 from [dbo].[WiadomosciPSPopularneTytul]";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                // this will query your database and return the result to your datatable
                da.Fill(dataTable);
                sqlConnection1.Close();
                da.Dispose();
                return dataTable;
            }
            catch (Exception ex)
            {
                AddToLog("Błąd dodawania wiadomości: " + ex.ToString());
                return null;
            }
        }


        public static DataTable GetWiadomoscNajnowsze()
        {
            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;
                DataTable dataTable = new DataTable();


                sqlConnection1.Open();

                cmd.CommandText = "Select Wiadomosc1,Wiadomosc2,Wiadomosc3,Wiadomosc4,Wiadomosc5,Wiadomosc6,Wiadomosc7," +
                    "Wiadomosc8,Wiadomosc9,Wiadomosc10 from [dbo].[WiadomosciPSNajnowsze]";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                // this will query your database and return the result to your datatable
                da.Fill(dataTable);
                sqlConnection1.Close();
                da.Dispose();
                return dataTable;
            }
            catch (Exception ex)
            {
                AddToLog("Błąd dodawania wiadomości: " + ex.ToString());
                return null;
            }
        }


        public static DataTable GetWiadomoscNajnowszeImg()
        {
            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;
                DataTable dataTable = new DataTable();


                sqlConnection1.Open();

                cmd.CommandText = "Select Wiadomosc1,Wiadomosc2,Wiadomosc3,Wiadomosc4,Wiadomosc5,Wiadomosc6,Wiadomosc7," +
                    "Wiadomosc8,Wiadomosc9,Wiadomosc10 from [dbo].[WiadomosciPSNajnowszeImg]";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                // this will query your database and return the result to your datatable
                da.Fill(dataTable);
                sqlConnection1.Close();
                da.Dispose();
                return dataTable;
            }
            catch (Exception ex)
            {
                AddToLog("Błąd dodawania wiadomości: " + ex.ToString());
                return null;
            }
        }

        public static DataTable GetWiadomoscNajnowszeTytul()
        {
            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;
                DataTable dataTable = new DataTable();


                sqlConnection1.Open();

                cmd.CommandText = "Select Wiadomosc1,Wiadomosc2,Wiadomosc3,Wiadomosc4,Wiadomosc5,Wiadomosc6,Wiadomosc7," +
                    "Wiadomosc8,Wiadomosc9,Wiadomosc10 from [dbo].[WiadomosciPSNajnowszeTytul]";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                // this will query your database and return the result to your datatable
                da.Fill(dataTable);
                sqlConnection1.Close();
                da.Dispose();
                return dataTable;
            }
            catch (Exception ex)
            {
                AddToLog("Błąd dodawania wiadomości: " + ex.ToString());
                return null;
            }
        }


        public static void AddWiadomoscNajnowsze(System.Collections.Generic.List<string> hrefList)
        {
            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;



                sqlConnection1.Open();

                cmd.CommandText = "Delete from [dbo].[WiadomosciPSNajnowsze]";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;
                cmd.ExecuteNonQuery();

                cmd.CommandText = "INSERT INTO [dbo].[WiadomosciPSNajnowsze]  (Nazwa,DataUtw,Wiadomosc1,Wiadomosc2,Wiadomosc3,Wiadomosc4,Wiadomosc5,Wiadomosc6,Wiadomosc7,Wiadomosc8,Wiadomosc9,Wiadomosc10) VALUES ('" + "" + "','" + DateTime.Now + "','" + hrefList[0] + "','" + hrefList[1] + "','" + hrefList[2] + "','" + hrefList[3] + "','" + hrefList[4] + "','" + hrefList[5] + "','" + hrefList[6] + "','" + hrefList[7] + "','" + hrefList[8] + "','" + hrefList[9] + "')";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;
                cmd.ExecuteNonQuery();

                sqlConnection1.Close();
            }
            catch (Exception ex)
            {
                AddToLog("Błąd dodawania wiadomości: " + ex.ToString());
            }
        }

        public static void AddWiadomoscNajnowszeImg(System.Collections.Generic.List<string> hrefList)
        {
            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;



                sqlConnection1.Open();

                cmd.CommandText = "Delete from [dbo].[WiadomosciPSNajnowszeImg]";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;
                cmd.ExecuteNonQuery();

                cmd.CommandText = "INSERT INTO [dbo].[WiadomosciPSNajnowszeImg]  (Nazwa,DataUtw,Wiadomosc1,Wiadomosc2,Wiadomosc3,Wiadomosc4,Wiadomosc5,Wiadomosc6,Wiadomosc7,Wiadomosc8,Wiadomosc9,Wiadomosc10) VALUES ('" + "" + "','" + DateTime.Now + "','" + hrefList[0] + "','" + hrefList[1] + "','" + hrefList[2] + "','" + hrefList[3] + "','" + hrefList[4] + "','" + hrefList[5] + "','" + hrefList[6] + "','" + hrefList[7] + "','" + hrefList[8] + "','" + hrefList[9] + "')";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;
                cmd.ExecuteNonQuery();

                sqlConnection1.Close();
            }
            catch (Exception ex)
            {
                AddToLog("Błąd dodawania wiadomości: " + ex.ToString());
            }
        }
        public static void AddWiadomoscNajnowszeTytul(System.Collections.Generic.List<string> hrefList)
        {
            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;



                sqlConnection1.Open();

                cmd.CommandText = "Delete from [dbo].[WiadomosciPSNajnowszeTytul]";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;
                cmd.ExecuteNonQuery();

                cmd.CommandText = "INSERT INTO [dbo].[WiadomosciPSNajnowszeTytul] (Nazwa,DataUtw,Wiadomosc1,Wiadomosc2,Wiadomosc3,Wiadomosc4,Wiadomosc5,Wiadomosc6,Wiadomosc7,Wiadomosc8,Wiadomosc9,Wiadomosc10) VALUES ('" + "" + "','" + DateTime.Now + "','" + hrefList[0] + "','" + hrefList[1] + "','" + hrefList[2] + "','" + hrefList[3] + "','" + hrefList[4] + "','" + hrefList[5] + "','" + hrefList[6] + "','" + hrefList[7] + "','" + hrefList[8] + "','" + hrefList[9] + "')";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;
                cmd.ExecuteNonQuery();

                sqlConnection1.Close();
            }
            catch (Exception ex)
            {
                AddToLog("Błąd dodawania wiadomości: " + ex.ToString());
            }
        }

        public static void AddWiadomoscPopularne(System.Collections.Generic.List<string> hrefList)
        {
            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;



                sqlConnection1.Open();

                cmd.CommandText = "Delete from [dbo].[WiadomosciPSPopularne]";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;
                cmd.ExecuteNonQuery();

                cmd.CommandText = "INSERT INTO [dbo].[WiadomosciPSPopularne] (Nazwa,DataUtw,Wiadomosc1,Wiadomosc2,Wiadomosc3,Wiadomosc4,Wiadomosc5,Wiadomosc6,Wiadomosc7,Wiadomosc8,Wiadomosc9,Wiadomosc10) VALUES ('" + "" + "','" + DateTime.Now + "','" + hrefList[0] + "','" + hrefList[1] + "','" + hrefList[2] + "','" + hrefList[3] + "','" + hrefList[4] + "','" + hrefList[5] + "','" + hrefList[6] + "','" + hrefList[7] + "','" + hrefList[8] + "','" + hrefList[9] + "')";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;
                cmd.ExecuteNonQuery();

                sqlConnection1.Close();
            }
            catch (Exception ex)
            {
                AddToLog("Błąd dodawania wiadomości: " + ex.ToString());
            }
        }

        public static void AddWiadomoscPopularneImg(System.Collections.Generic.List<string> hrefList)
        {
            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;



                sqlConnection1.Open();

                cmd.CommandText = "Delete from [dbo].[WiadomosciPSPopularneImg]";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;
                cmd.ExecuteNonQuery();

                cmd.CommandText = "INSERT INTO [dbo].[WiadomosciPSPopularneImg] (Nazwa,DataUtw,Wiadomosc1,Wiadomosc2,Wiadomosc3,Wiadomosc4,Wiadomosc5,Wiadomosc6,Wiadomosc7,Wiadomosc8,Wiadomosc9,Wiadomosc10) VALUES ('" + "" + "','" + DateTime.Now + "','" + hrefList[0] + "','" + hrefList[1] + "','" + hrefList[2] + "','" + hrefList[3] + "','" + hrefList[4] + "','" + hrefList[5] + "','" + hrefList[6] + "','" + hrefList[7] + "','" + hrefList[8] + "','" + hrefList[9] + "')";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;
                cmd.ExecuteNonQuery();

                sqlConnection1.Close();
            }
            catch (Exception ex)
            {
                AddToLog("Błąd dodawania wiadomości: " + ex.ToString());
            }
        }

        public static void AddWiadomoscPopularneTytul(System.Collections.Generic.List<string> hrefList)
        {
            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;



                sqlConnection1.Open();

                cmd.CommandText = "Delete from [dbo].[WiadomosciPSPopularneTytul]";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;
                cmd.ExecuteNonQuery();


                cmd.CommandText = "INSERT INTO [dbo].[WiadomosciPSPopularneTytul] (Nazwa,DataUtw,Wiadomosc1,Wiadomosc2,Wiadomosc3,Wiadomosc4,Wiadomosc5,Wiadomosc6,Wiadomosc7,Wiadomosc8,Wiadomosc9,Wiadomosc10) VALUES ('" + "" + "','" + DateTime.Now + "','" + hrefList[0] + "','" + hrefList[1] + "','" + hrefList[2] + "','" + hrefList[3] + "','" + hrefList[4] + "','" + hrefList[5] + "','" + hrefList[6] + "','" + hrefList[7] + "','" + hrefList[8] + "','" + hrefList[9] + "')";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;
                cmd.ExecuteNonQuery();

                sqlConnection1.Close();
            }
            catch (Exception ex)
            {
                AddToLog("Błąd dodawania wiadomości: " + ex.ToString());
            }
        }

        public static void AddWiadomoscSiatka(List<System.Linq.IGrouping<string, string>> hrefList)
        {
            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;

                cmd.CommandText = "INSERT INTO [dbo].[WiadomosciPolsatSport] (Nazwa,DataUtw,Wiadomosc1,Wiadomosc2,Wiadomosc3,Wiadomosc4,Wiadomosc5) VALUES ('" + "" + "','" + DateTime.Now + "','" + hrefList[0].Key + "','" + hrefList[1].Key + "','" + hrefList[2].Key + "','" + hrefList[3].Key + "','" + hrefList[4].Key + "')";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;

                sqlConnection1.Open();
                cmd.ExecuteNonQuery();

                sqlConnection1.Close();
            }
            catch (Exception ex)
            {
                AddToLog("Błąd dodawania wiadomości Orlen: " + ex.ToString());
            }
        }

        public static void AddWiadomoscHokej(List<System.Linq.IGrouping<string, string>> hrefList)
        {
            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;

                cmd.CommandText = "INSERT INTO [dbo].[WiadomosciGKSKatowiceHokej] (Nazwa,DataUtw,Wiadomosc1,Wiadomosc2,Wiadomosc3) VALUES ('" + "" + "','" + DateTime.Now + "','" + hrefList[0].Key + "','" + hrefList[1].Key + "','" + hrefList[2].Key + "')";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;

                sqlConnection1.Open();
                cmd.ExecuteNonQuery();

                sqlConnection1.Close();
            }
            catch (Exception ex)
            {
                AddToLog("Błąd dodawania wiadomości Orlen: " + ex.ToString());
            }
        }


        public static void ChangeNotification(string id, byte tryb)
        {
            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;

                cmd.CommandText = "Update [dbo].[UserPolsatSport] SET flgDeleted = "+ tryb+" where UserId="+"'"+id+"'";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;

                sqlConnection1.Open();
                cmd.ExecuteNonQuery();

                sqlConnection1.Close();
            }
            catch (Exception ex)
            {
                AddToLog("Błąd aktualizacji powiadomień: " + ex.ToString());
            }
        }
    }
}