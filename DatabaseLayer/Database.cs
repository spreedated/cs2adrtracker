#pragma warning disable S1215
#pragma warning disable S3881

using DatabaseLayer.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using static DatabaseLayer.Logic.HelperFunctions;

namespace DatabaseLayer.DataLayer
{
    public class Database : IDisposable
    {
        private readonly string _databasePath;
        internal SQLiteConnection _conn;

        #region Constructor
        /// <summary>
        /// Create a new Database Object
        /// </summary>
        /// <param name="dbFilepath"></param>
        /// <param name="autoOpenConnection">Auto open a connection</param>
        public Database(string dbFilepath, bool autoOpenConnection = true)
        {
            this._databasePath = dbFilepath;

            if (string.IsNullOrEmpty(dbFilepath))
            {
                throw new ArgumentException("Filepath cannot be null or empty", nameof(dbFilepath));
            }

            if (!File.Exists(this._databasePath))
            {
                this.CreateEmptyDatabase();
            }

            if (autoOpenConnection)
            {
                this.Open();
            }
        }
        #endregion

        private void CreateEmptyDatabase()
        {
            this.Open();

            using (SQLiteCommand cmd = this._conn.CreateCommand())
            {
                cmd.CommandText = LoadEmbeddedSql("CreateTableAdrs");
                cmd.ExecuteNonQuery();
            }

            this._conn.Close();
        }

        public void Open()
        {
            this.Close();

            SQLiteConnectionStringBuilder b = new()
            {
                DataSource = this._databasePath,
                Pooling = true
            };

            this._conn = new(b.ToString());
            this._conn.Open();
        }

        public void Close()
        {
            this._conn?.Close();
            this._conn?.Dispose();
        }

        /// <summary>
        /// Get the last # of records
        /// </summary>
        /// <param name="count">Number of records, default is 10</param>
        /// <returns></returns>
        public IEnumerable<AdrRecord> GetLast(int count = 10)
        {
            if (count <= 0)
            {
                count = 1;
            }

            using (SQLiteTransaction trans = this._conn.BeginTransaction())
            {
                using (SQLiteCommand cmd = this._conn.CreateCommand())
                {
                    cmd.CommandText = $"SELECT id,value,timestamp FROM adrs ORDER BY timestamp DESC LIMIT {count};";

                    using (SQLiteDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            yield return new()
                            {
                                Id = dr.GetInt32(dr.GetOrdinal("id")),
                                Value = dr.GetInt32(dr.GetOrdinal("value")),
                                UnixTimestamp = dr.GetInt32(dr.GetOrdinal("timestamp"))
                            };
                        }
                    }
                }

                trans.Commit();
            }
        }

        public IEnumerable<AdrRecord> GetAdrs()
        {
            using (SQLiteTransaction trans = this._conn.BeginTransaction())
            {
                using (SQLiteCommand cmd = this._conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT id,value,timestamp FROM adrs;";

                    using (SQLiteDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            yield return new()
                            {
                                Id = dr.GetInt32(dr.GetOrdinal("id")),
                                Value = dr.GetInt32(dr.GetOrdinal("value")),
                                UnixTimestamp = dr.GetInt32(dr.GetOrdinal("timestamp"))
                            };
                        }
                    }
                }

                trans.Commit();
            }
        }

        private SQLiteCommand CreateInsertCommand(AdrRecord adr)
        {
            SQLiteCommand cmd = this._conn.CreateCommand();

            cmd.CommandText = "INSERT INTO adrs (value,timestamp) VALUES (@v,@t);";
            cmd.Parameters.AddWithValue("@v", adr.Value);
            cmd.Parameters.AddWithValue("@t", adr.UnixTimestamp);

            return cmd;
        }

        public bool AddAdr(AdrRecord adr)
        {
            if (!adr.IsValid())
            {
                return false;
            }

            using (SQLiteCommand cmd = this.CreateInsertCommand(adr))
            {
                return cmd.ExecuteNonQuery() >= 1;
            }
        }

        public bool AddAdr(IEnumerable<AdrRecord> adrs)
        {
            if (!adrs.Any(x => x.IsValid()))
            {
                return false;
            }

            Queue<SQLiteCommand> cmds = new();

            foreach (AdrRecord a in adrs)
            {
                cmds.Enqueue(this.CreateInsertCommand(a));
            }

            int cmdsCount = cmds.Count;
            int transCmdsCount = 0;

            using (SQLiteTransaction trans = this._conn.BeginTransaction())
            {
                for (int i = 0; i < cmdsCount; i++)
                {
                    SQLiteCommand cmd = cmds.Dequeue();
                    cmd.Transaction = trans;

                    transCmdsCount += cmd.ExecuteNonQuery();
                }

                trans.Commit();
            }

            return cmdsCount == transCmdsCount;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            SQLiteConnection.ClearAllPools();
            this._conn?.Close();
            this._conn?.Dispose();
        }
    }
}
