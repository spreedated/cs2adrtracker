using DatabaseLayer.DataLayer;
using DatabaseLayer.Models;
using Microsoft.Data.Sqlite;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace UnitTests
{
    [TestFixture]
    public class DataLayerTests
    {
        private string nunitTestFolder;
        private string databaseTestFile;
        private Database database;

        private void DeleteAllFilesFromTestFolder()
        {
            foreach (string f in Directory.GetFiles(this.nunitTestFolder))
            {
                File.Delete(f);
            }

            this.CreateDummyFile();
        }

        private void CreateDummyFile()
        {
            byte[] randomBytes = new byte[8];

            for (int i = 0; i < randomBytes.Length; i++)
            {
                Random rand = new(BitConverter.ToInt32(Guid.NewGuid().ToByteArray()));

                randomBytes[i] = (byte)rand.Next(0, 255);
            }

            using (FileStream fs = File.Open(Path.Combine(this.nunitTestFolder, "dummy"), FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                fs.Write(randomBytes, 0, randomBytes.Length);
            }
        }

        [SetUp]
        public void SetUp()
        {
            this.nunitTestFolder = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(typeof(DataLayerTests).Assembly.Location), "..", "..", "..", "..", "NunitTestFolder"));
            this.databaseTestFile = Path.Combine(this.nunitTestFolder, "db.nexn");

            this.DeleteAllFilesFromTestFolder();
        }

        [Test]
        public void FileCreationTests()
        {
            Assert.That(File.Exists(this.databaseTestFile), Is.False);
            Assert.DoesNotThrow(() =>
            {
                this.database = new(this.databaseTestFile, false);
            });
            Assert.That(File.Exists(this.databaseTestFile), Is.True);
        }

        [Test]
        public void BlankDatabaseTests()
        {
            this.database = new(this.databaseTestFile, false);

            Assert.That(this.database._conn.State, Is.EqualTo(ConnectionState.Closed));

            this.database.Open();

            Assert.That(this.database._conn.State, Is.EqualTo(ConnectionState.Open));

            using (SqliteCommand cmd = this.database._conn.CreateCommand())
            {
                cmd.CommandText = $"SELECT count(*) FROM sqlite_schema;";

                Assert.That((long)cmd.ExecuteScalar(), Is.GreaterThanOrEqualTo(1));
            }

            using (SqliteCommand cmd = this.database._conn.CreateCommand())
            {
                List<string> names = [];

                cmd.CommandText = $"SELECT name FROM sqlite_schema;";

                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        names.Add(dr.GetString(0));
                    }
                }

                cmd.ExecuteNonQuery();

                Assert.That(names, Does.Contain("adrs"));
            }
        }

        [Test]
        public void SetAdrTests()
        {
            this.database = new(this.databaseTestFile);
            Stopwatch sw = new();

            //# Batch ADR insert
            List<AdrRecord> randomAdrs = [];

            Assert.DoesNotThrow(() =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    Random rnd = new(BitConverter.ToInt32(Guid.NewGuid().ToByteArray()));
                    randomAdrs.Add(new() { Value = rnd.Next(20, 189), Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds() });
                }
            });

            sw.Start();
            Assert.That(this.database.AddAdr(randomAdrs), Is.True);
            sw.Stop();

            Console.WriteLine($"Batch ADR insert time: \"{sw.Elapsed:mm\\:ss\\:ffffff}\"");
            sw.Reset();

            using (SqliteCommand cmd = this.database._conn.CreateCommand())
            {
                cmd.CommandText = $"SELECT count(*) FROM adrs;";

                Assert.That((long)cmd.ExecuteScalar(), Is.GreaterThanOrEqualTo(1000));
            }
            //# ### #

            //# Single ADR insert
            sw.Start();
            Assert.DoesNotThrow(() =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    Random rnd = new(BitConverter.ToInt32(Guid.NewGuid().ToByteArray()));

                    this.database.AddAdr(new AdrRecord() { Value = rnd.Next(20, 189), Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds() });
                }
            });
            sw.Stop();

            Console.WriteLine($"Single ADR insert time: \"{sw.Elapsed:mm\\:ss\\:ffffff}\"");
            sw.Reset();

            using (SqliteCommand cmd = this.database._conn.CreateCommand())
            {
                cmd.CommandText = $"SELECT count(*) FROM adrs;";

                Assert.That((long)cmd.ExecuteScalar(), Is.GreaterThanOrEqualTo(2000));
            }
            //# ### #
        }

        [Test]
        public void GetAdrsTests()
        {
            this.database = new(this.databaseTestFile);

            List<AdrRecord> randomAdrs = [];

            Assert.DoesNotThrow(() =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    Random rnd = new(BitConverter.ToInt32(Guid.NewGuid().ToByteArray()));
                    randomAdrs.Add(new() { Value = rnd.Next(20, 189), Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds() });
                }
            });

            Assert.That(this.database.AddAdr(randomAdrs), Is.True);

            IEnumerable<AdrRecord> getAdrs = this.database.GetAdrs();

            Assert.Multiple(() =>
            {
                Assert.That(getAdrs.Count(), Is.EqualTo(randomAdrs.Count));
                Assert.That(getAdrs.Sum(x => x.Value), Is.EqualTo(randomAdrs.Sum(x => x.Value)));
                Assert.That(getAdrs.Any(x => x.Id != default), Is.True);
            });
        }

        [TearDown]
        public void TearDown()
        {
            this.database?.Dispose();
            this.DeleteAllFilesFromTestFolder();
        }
    }
}
