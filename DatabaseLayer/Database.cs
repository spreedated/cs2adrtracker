#pragma warning disable S1215
#pragma warning disable S3881

using Dapper;
using DatabaseLayer.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static DatabaseLayer.Logic.HelperFunctions;

namespace DatabaseLayer.DataLayer
{
    public class Database : IDisposable
    {
        private readonly ILogger logger;
        private readonly string databasePath;
        internal SqliteConnection conn;

        #region Constructor
        /// <summary>
        /// Create a new Database Object
        /// </summary>
        /// <param name="dbFilepath"></param>
        /// <param name="autoOpenConnection">Auto open a connection</param>
        public Database(string dbFilepath, bool autoOpenConnection = true, ILogger logger = null)
        {
            this.logger = logger;
            this.databasePath = dbFilepath;

            if (string.IsNullOrEmpty(dbFilepath))
            {
                throw new ArgumentException("Filepath cannot be null or empty", nameof(dbFilepath));
            }

            if (!File.Exists(this.databasePath))
            {
                this.CreateEmptyDatabase().Wait();
            }

            if (autoOpenConnection)
            {
                this.OpenAsync().Wait();
            }
        }
        #endregion

        private async Task CreateEmptyDatabase()
        {
            await this.OpenAsync();
            await this.conn.ExecuteAsync(LoadEmbeddedSql("CreateTableAdrs"));
            await this.conn.CloseAsync();

            this.logger?.LogInformation("Database created at {DatabasePath}", this.databasePath);
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

        public async Task OpenAsync()
        {
            this.Close();

            SqliteConnectionStringBuilder b = new()
            {
                DataSource = this.databasePath,
                Pooling = true
            };

            this.conn = new(b.ToString());
            SQLitePCL.Batteries.Init();

            await this.conn.OpenAsync();

            this.logger?.LogTrace("Database connection opened to {DatabasePath}", this.databasePath);
        }

        public void Close()
        {
            this.conn?.Close();
            this.conn?.Dispose();

            this.logger?.LogTrace("Database connection closed to {DatabasePath}", this.databasePath);
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

            using (SqliteTransaction trans = this.conn.BeginTransaction())
            {
                foreach (AdrRecord a in adrs)
                {
                    transCmdsCount += this.conn.Execute(LoadEmbeddedSql("AddAdr"), CreateInsertDynamicParameters(a), trans);
                }

                trans.Commit();
            }

            return adrs.Count() == transCmdsCount;
        }

        public bool DeleteAdr(int id)
        {
            return this.conn.Execute(LoadEmbeddedSql("DeleteAdr"), new { id }) == 1;
        }

        public Statistic GetStatistic()
        {
            return this.conn.QueryFirst<Statistic>(LoadEmbeddedSql("GetStatistic"));
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

            return this.conn.Query<AdrRecord>($"SELECT id,value,timestamp,outcome FROM adrs ORDER BY timestamp DESC LIMIT {count};");
        }

        public IEnumerable<AdrRecord> GetAdrs()
        {
            return this.conn.Query<AdrRecord>("SELECT id,value,timestamp,outcome FROM adrs;");
        }

        #region Dispose
        public void Dispose()
        {
            this.Dispose(true);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.SuppressFinalize(this);

            this.logger?.LogTrace("Database disposed");
        }

        protected virtual void Dispose(bool disposing)
        {
            SqliteConnection.ClearAllPools();
            this.conn?.Close();
            this.conn?.Dispose();
        }
        #endregion
    }
}
