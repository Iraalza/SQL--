using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace laba1
{
    class Program
    {
        static string connectionString = "Server=.;Database=Minions;Trusted_Connection=True";
        static void Main(string[] args)
        {
            //NameVillains();
            //NameMinions(2);
            //AddMinion("Robert", 20, "Paris", "Ivan");
            //DeleteVillain(10);
            YearsPassed(new int[] { 5, 7, 8 } );
        }

        static void NameVillains()
        {
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            using (connection)
            {
                string selectionCommandString = "SELECT Villains.Name, COUNT(MinionsVillains.MinionId) " +
                    "FROM Villains, MinionsVillains " +
                    "WHERE MinionsVillains.VillainId = Villains.Id " +
                    "GROUP BY Villains.Name " +
                    "HAVING COUNT(MinionsVillains.MinionId) >= 3 " +
                    "ORDER BY COUNT(MinionsVillains.MinionId) DESC ";
                SqlCommand command = new SqlCommand(selectionCommandString, connection);
                SqlDataReader reader = command.ExecuteReader();
                using (reader)
                {
                    while (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            Console.Write($"{reader[i]} ");
                        }
                        Console.WriteLine();
                    }
                }
            }
        }

        static int NameMinions(int id)
        {
            string selectionCommandString = $"SELECT Name " +
                    $"FROM Villains " +
                    $"WHERE Id = @id ";
            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand(selectionCommandString, connection);
            SqlParameter parameter = new SqlParameter("@id", SqlDbType.Int) { Value = id };
            command.Parameters.Add(parameter);
            connection.Open();
            using (connection)
            {
                SqlDataReader reader = command.ExecuteReader();
                using (reader)
                {
                    if (!(reader.HasRows))
                    {
                        Console.WriteLine($"No villain with ID {id} exists in the database.");
                        return 0;
                    }
                    else
                    {
                        reader.Read();
                        Console.WriteLine($"Villain: {reader[0]}");
                    }
                }
                selectionCommandString = $"SELECT Name, Age " +
                $"FROM Minions " +
                $"JOIN MinionsVillains " +
                $"ON MinionsVillains.MinionId = Minions.Id " +
                $"Where MinionsVillains.VillainId = @id ";
                command = new SqlCommand(selectionCommandString, connection);
                parameter = new SqlParameter("@id", SqlDbType.Int) { Value = id };
                command.Parameters.Add(parameter);
                reader = command.ExecuteReader();
                using (reader)
                {
                    if (!(reader.HasRows))
                    {
                        Console.WriteLine("no minions");
                    }
                    else
                    {
                        int count = 1;
                        while (reader.Read())
                        {
                            Console.WriteLine($"{ count}.");
                            count++;
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                Console.Write($"{reader[i]} ");
                            }
                            Console.WriteLine();
                        }
                    }
                }
            }
            return 1;
        }

        static void AddMinion(string minionName, int age, string town, string villainName)
        {
            string selectionCommandString = $"SELECT Name " +
                    $"FROM Towns " +
                    $"WHERE Name = @town ";
            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand(selectionCommandString, connection);
            command.Parameters.AddWithValue("@town", town);
            connection.Open();
            using (connection)
            {
                SqlDataReader reader = command.ExecuteReader();
                using (reader)
                {
                    if (!(reader.HasRows))
                    {
                        command = new SqlCommand($"INSERT INTO Towns " +
                            $"(Name) VALUES " +
                            $"(@town)", connection);
                        command.Parameters.AddWithValue("@town", town);
                        reader.Close();
                        command.ExecuteNonQuery();
                        Console.WriteLine($"Город {town} был добавлен в базу данных.");
                    }

                }

                command = new SqlCommand($"SELECT Name " +
                    $"FROM Villains " +
                    $"WHERE Name = @villainName", connection);
                command.Parameters.AddWithValue("@villainName", villainName);
                reader = command.ExecuteReader();
                using (reader)
                {
                    if (!(reader.HasRows))
                    {
                        command = new SqlCommand($"INSERT INTO Villains " +
                        $"(Name, EvilnessFactorId) VALUES " +
                        $"(@villainName, 4)", connection);
                        command.Parameters.AddWithValue("@villainName", villainName);
                        reader.Close();
                        command.ExecuteNonQuery();
                        Console.WriteLine($"Злодей {villainName} был добавлен в базу данных.");
                    }

                }

                command = new SqlCommand($"INSERT INTO Minions " +
                    $"(Name, Age, TownId) VALUES " +
                    $"(@minionName, @age, (SELECT Id FROM Towns WHERE Name = @town))", connection);
                command.Parameters.AddWithValue("@minionName", minionName);
                command.Parameters.AddWithValue("@age", age);
                command.Parameters.AddWithValue("@town", town);
                command.ExecuteNonQuery();
                command = new SqlCommand("INSERT INTO MinionsVillains " +
                "(MinionId, VillainId) VALUES " +
                "((SELECT TOP 1 Id FROM Minions ORDER BY Id DESC), (SELECT Id FROM Villains WHERE Name = @villainName))", connection);
                command.Parameters.AddWithValue("@minionName", minionName);
                command.Parameters.AddWithValue("@villainName", villainName);
                command.ExecuteNonQuery();
                Console.WriteLine($"Успешно добавлен {minionName}, чтобы быть миньоном {villainName}.");
            }

        }

        static void DeleteVillain(int id)
        {
            string selectionCommandString = $"SELECT Name FROM Villains " +
                $"WHERE Id = @id";
            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand(selectionCommandString, connection);
            command.Parameters.AddWithValue("@id", id);
            connection.Open();
            using (connection)
            {
                SqlDataReader reader = command.ExecuteReader();
                using (reader)
                {
                    if (!(reader.HasRows))
                    {
                        Console.WriteLine("Такой злодей не найден.");
                    }
                    else
                    {
                        reader.Read();
                        Console.WriteLine($"{reader[0]} был удалён.");
                        reader.Close();
                        command = new SqlCommand($"SELECT COUNT(MinionId) " +
                            $"FROM MinionsVillains " +
                            $"WHERE VillainId = @id", connection);
                        command.Parameters.AddWithValue("@id", id);
                        reader = command.ExecuteReader();
                        reader.Read();
                        Console.WriteLine($"{reader[0]} миньонов было освобождено.");
                        reader.Close();
                        command = new SqlCommand($"DELETE FROM MinionsVillains " +
                            $"WHERE VillainId = @id", connection);
                        command.Parameters.AddWithValue("@id", id);
                        command.ExecuteNonQuery();
                        command = new SqlCommand($"DELETE FROM Villains " +
                        "WHERE Id = @id", connection);
                        command.Parameters.AddWithValue("@id", id);
                        command.ExecuteNonQuery();

                    }

                }

            }
        }

        static void YearsPassed(int[] id)
        {
            string temp_request = $" {id[0]}";
            for (int i = 1; i < id.Length; i++)
            {
                temp_request = temp_request + $" OR Id = {id[i]}";
            }
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            using (connection)
            {
                string selectionCommandString = $"UPDATE Minions " +
                    $"SET Age = Age + 1 " +
                    $"WHERE Id ={temp_request}";
                SqlCommand command = new SqlCommand(selectionCommandString, connection);
                command.ExecuteNonQuery();
                command = new SqlCommand("SELECT Name, Age " +
                    "FROM Minions", connection);
                SqlDataReader reader = command.ExecuteReader();
                using (reader)
                {
                    while (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            Console.Write($"{reader[i]}\t");
                        }
                        Console.WriteLine();
                    }
                }
            }
        }      
    }
}
          