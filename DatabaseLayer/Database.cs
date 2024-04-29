#pragma warning disable S1215
#pragma warning disable S3881

using Dapper;
using DatabaseLayer.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using static DatabaseLayer.Logic.HelperFunctions;

namespace DatabaseLayer.DataLayer
{
    public class Database : IDisposable
    {
        private readonly string _databasePath;
        internal SqliteConnection _conn;

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

            using (SqliteCommand cmd = this._conn.CreateCommand())
            {
                cmd.CommandText = LoadEmbeddedSql("CreateTableAdrs");
                cmd.ExecuteNonQuery();
            }

            this._conn.Close();
        }

        private static DynamicParameters CreateInsertDynamicParameters(AdrRecord adr)
        {
            Dictionary<string, object> paramDictionary = new()
                    {
                        { "@v", adr.Value },
                        { "@t", adr.Timestamp },
                        { "@o", adr.Outcome switch
                        {
                            AdrRecord.Outcomes.Loss => 1,
                            AdrRecord.Outcomes.Win => 2,
                            AdrRecord.Outcomes.Draw => 3,
                            _ => 0
                        } }
                    };

            return new(paramDictionary);
        }

        public void Open()
        {
            this.Close();

            SqliteConnectionStringBuilder b = new()
            {
                DataSource = this._databasePath,
                Pooling = true
            };

            this._conn = new(b.ToString());
            SQLitePCL.Batteries.Init();

            this._conn.Open();
        }

        public void Close()
        {
            this._conn?.Close();
            this._conn?.Dispose();
        }

        public bool AddAdr(AdrRecord adr)
        {
            return this.AddAdr([adr]);
        }

        public bool AddAdr(IEnumerable<AdrRecord> adrs)
        {
            if (!adrs.Any(x => x.IsValid()))
            {
                return false;
            }

            int transCmdsCount = 0;

            using (SqliteTransaction trans = this._conn.BeginTransaction())
            {

                foreach (AdrRecord a in adrs)
                {
                    transCmdsCount += this._conn.Execute("INSERT INTO adrs (value,timestamp,outcome) VALUES (@v,@t,@o);", CreateInsertDynamicParameters(a), trans);
                }

                trans.Commit();
            }

            return adrs.Count() == transCmdsCount;
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

            return this._conn.Query<AdrRecord>($"SELECT id,value,timestamp,outcome FROM adrs ORDER BY timestamp DESC LIMIT {count};");
        }

        public IEnumerable<AdrRecord> GetAdrs()
        {
            return this._conn.Query<AdrRecord>("SELECT id,value,timestamp,outcome FROM adrs;");
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
            SqliteConnection.ClearAllPools();
            this._conn?.Close();
            this._conn?.Dispose();
        }
    }
}
