﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MarkMpn.Sql4Cds.Engine.Tests
{
    [TestClass]
    public class WindowFunctionTests : FakeXrmEasyTestsBase
    {
        [TestMethod]
        public void WindowFunctionsCanOnlyAppearInTheSelectOrOrderByClauses()
        {
            using (var con = new Sql4CdsConnection(_localDataSources))
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = "SELECT name FROM account WHERE ROW_NUMBER() OVER (ORDER BY name) = 1";

                try
                {
                    cmd.ExecuteNonQuery();
                    Assert.Fail("Expected exception");
                }
                catch (Sql4CdsException ex)
                {
                    Assert.AreEqual(4108, ex.Number);
                }
            }
        }

        [TestMethod]
        public void RowNumber()
        {
            using (var con = new Sql4CdsConnection(_localDataSources))
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = @"INSERT INTO account (name, employees) VALUES
('Data10', 30),
('Data8', 10),
('Data9', 20)";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "SELECT name, ROW_NUMBER() OVER (ORDER BY employees) AS rownum FROM account";

                using (var reader = cmd.ExecuteReader())
                {
                    Assert.IsTrue(reader.Read());
                    Assert.AreEqual("Data8", reader["name"]);
                    Assert.AreEqual(1L, reader["rownum"]);

                    Assert.IsTrue(reader.Read());
                    Assert.AreEqual("Data9", reader["name"]);
                    Assert.AreEqual(2L, reader["rownum"]);

                    Assert.IsTrue(reader.Read());
                    Assert.AreEqual("Data10", reader["name"]);
                    Assert.AreEqual(3L, reader["rownum"]);

                    Assert.IsFalse(reader.Read());
                }
            }
        }

        [TestMethod]
        public void RowNumberWithPartition()
        {
            using (var con = new Sql4CdsConnection(_localDataSources))
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = @"INSERT INTO account (name, employees) VALUES
('Data9', 30),
('Data8', 10),
('Data8', 20)";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "SELECT name, ROW_NUMBER() OVER (PARTITION BY name ORDER BY employees) AS rownum FROM account";

                using (var reader = cmd.ExecuteReader())
                {
                    Assert.IsTrue(reader.Read());
                    Assert.AreEqual("Data8", reader["name"]);
                    Assert.AreEqual(1L, reader["rownum"]);

                    Assert.IsTrue(reader.Read());
                    Assert.AreEqual("Data8", reader["name"]);
                    Assert.AreEqual(2L, reader["rownum"]);

                    Assert.IsTrue(reader.Read());
                    Assert.AreEqual("Data9", reader["name"]);
                    Assert.AreEqual(1L, reader["rownum"]);

                    Assert.IsFalse(reader.Read());
                }
            }
        }

        [TestMethod]
        public void Rank()
        {
            // https://learn.microsoft.com/en-us/sql/t-sql/functions/rank-transact-sql?view=sql-server-ver16#a-ranking-rows-within-a-partition
            using (var con = new Sql4CdsConnection(_localDataSources))
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = @"INSERT INTO account (name, turnover, employees) VALUES
('Paint - Black', 3, 17),
('Paint - Black', 4, 14),
('Paint - Silver', 4, 12),
('Paint - Silver', 3, 49),
('Paint - Blue', 3, 49),
('Paint - Blue', 4, 35),
('Paint - Red', 3, 41),
('Paint - Red', 4, 24),
('Paint - Yellow', 3, 30),
('Paint - Yellow', 4, 25)";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "SELECT name, turnover, employees, RANK() OVER (PARTITION BY turnover ORDER BY employees DESC) AS rank FROM account";

                using (var reader = cmd.ExecuteReader())
                {
                    Assert.IsTrue(reader.Read());
                    Assert.AreEqual("Paint - Silver", reader["name"]);
                    Assert.AreEqual(3m, reader["turnover"]);
                    Assert.AreEqual(49, reader["employees"]);
                    Assert.AreEqual(1L, reader["rank"]);

                    Assert.IsTrue(reader.Read());
                    Assert.AreEqual("Paint - Blue", reader["name"]);
                    Assert.AreEqual(3m, reader["turnover"]);
                    Assert.AreEqual(49, reader["employees"]);
                    Assert.AreEqual(1L, reader["rank"]);

                    Assert.IsTrue(reader.Read());
                    Assert.AreEqual("Paint - Red", reader["name"]);
                    Assert.AreEqual(3m, reader["turnover"]);
                    Assert.AreEqual(41, reader["employees"]);
                    Assert.AreEqual(3L, reader["rank"]);

                    Assert.IsTrue(reader.Read());
                    Assert.AreEqual("Paint - Yellow", reader["name"]);
                    Assert.AreEqual(3m, reader["turnover"]);
                    Assert.AreEqual(30, reader["employees"]);
                    Assert.AreEqual(4L, reader["rank"]);

                    Assert.IsTrue(reader.Read());
                    Assert.AreEqual("Paint - Black", reader["name"]);
                    Assert.AreEqual(3m, reader["turnover"]);
                    Assert.AreEqual(17, reader["employees"]);
                    Assert.AreEqual(5L, reader["rank"]);

                    Assert.IsTrue(reader.Read());
                    Assert.AreEqual("Paint - Blue", reader["name"]);
                    Assert.AreEqual(4m, reader["turnover"]);
                    Assert.AreEqual(35, reader["employees"]);
                    Assert.AreEqual(1L, reader["rank"]);

                    Assert.IsTrue(reader.Read());
                    Assert.AreEqual("Paint - Yellow", reader["name"]);
                    Assert.AreEqual(4m, reader["turnover"]);
                    Assert.AreEqual(25, reader["employees"]);
                    Assert.AreEqual(2L, reader["rank"]);

                    Assert.IsTrue(reader.Read());
                    Assert.AreEqual("Paint - Red", reader["name"]);
                    Assert.AreEqual(4m, reader["turnover"]);
                    Assert.AreEqual(24, reader["employees"]);
                    Assert.AreEqual(3L, reader["rank"]);

                    Assert.IsTrue(reader.Read());
                    Assert.AreEqual("Paint - Black", reader["name"]);
                    Assert.AreEqual(4m, reader["turnover"]);
                    Assert.AreEqual(14, reader["employees"]);
                    Assert.AreEqual(4L, reader["rank"]);

                    Assert.IsTrue(reader.Read());
                    Assert.AreEqual("Paint - Silver", reader["name"]);
                    Assert.AreEqual(4m, reader["turnover"]);
                    Assert.AreEqual(12, reader["employees"]);
                    Assert.AreEqual(5L, reader["rank"]);

                    Assert.IsFalse(reader.Read());
                }
            }
        }

        [TestMethod]
        public void DenseRank()
        {
            // https://learn.microsoft.com/en-us/sql/t-sql/functions/dense-rank-transact-sql?view=sql-server-ver16#a-ranking-rows-within-a-partition
            using (var con = new Sql4CdsConnection(_localDataSources))
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = @"INSERT INTO account (name, turnover, employees) VALUES
('Paint - Black', 3, 17),
('Paint - Black', 4, 14),
('Paint - Silver', 4, 12),
('Paint - Silver', 3, 49),
('Paint - Blue', 3, 49),
('Paint - Blue', 4, 35),
('Paint - Red', 3, 41),
('Paint - Red', 4, 24),
('Paint - Yellow', 3, 30),
('Paint - Yellow', 4, 25)";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "SELECT name, turnover, employees, DENSE_RANK() OVER (PARTITION BY turnover ORDER BY employees DESC) AS rank FROM account";

                using (var reader = cmd.ExecuteReader())
                {
                    Assert.IsTrue(reader.Read());
                    Assert.AreEqual("Paint - Silver", reader["name"]);
                    Assert.AreEqual(3m, reader["turnover"]);
                    Assert.AreEqual(49, reader["employees"]);
                    Assert.AreEqual(1L, reader["rank"]);

                    Assert.IsTrue(reader.Read());
                    Assert.AreEqual("Paint - Blue", reader["name"]);
                    Assert.AreEqual(3m, reader["turnover"]);
                    Assert.AreEqual(49, reader["employees"]);
                    Assert.AreEqual(1L, reader["rank"]);

                    Assert.IsTrue(reader.Read());
                    Assert.AreEqual("Paint - Red", reader["name"]);
                    Assert.AreEqual(3m, reader["turnover"]);
                    Assert.AreEqual(41, reader["employees"]);
                    Assert.AreEqual(2L, reader["rank"]);

                    Assert.IsTrue(reader.Read());
                    Assert.AreEqual("Paint - Yellow", reader["name"]);
                    Assert.AreEqual(3m, reader["turnover"]);
                    Assert.AreEqual(30, reader["employees"]);
                    Assert.AreEqual(3L, reader["rank"]);

                    Assert.IsTrue(reader.Read());
                    Assert.AreEqual("Paint - Black", reader["name"]);
                    Assert.AreEqual(3m, reader["turnover"]);
                    Assert.AreEqual(17, reader["employees"]);
                    Assert.AreEqual(4L, reader["rank"]);

                    Assert.IsTrue(reader.Read());
                    Assert.AreEqual("Paint - Blue", reader["name"]);
                    Assert.AreEqual(4m, reader["turnover"]);
                    Assert.AreEqual(35, reader["employees"]);
                    Assert.AreEqual(1L, reader["rank"]);

                    Assert.IsTrue(reader.Read());
                    Assert.AreEqual("Paint - Yellow", reader["name"]);
                    Assert.AreEqual(4m, reader["turnover"]);
                    Assert.AreEqual(25, reader["employees"]);
                    Assert.AreEqual(2L, reader["rank"]);

                    Assert.IsTrue(reader.Read());
                    Assert.AreEqual("Paint - Red", reader["name"]);
                    Assert.AreEqual(4m, reader["turnover"]);
                    Assert.AreEqual(24, reader["employees"]);
                    Assert.AreEqual(3L, reader["rank"]);

                    Assert.IsTrue(reader.Read());
                    Assert.AreEqual("Paint - Black", reader["name"]);
                    Assert.AreEqual(4m, reader["turnover"]);
                    Assert.AreEqual(14, reader["employees"]);
                    Assert.AreEqual(4L, reader["rank"]);

                    Assert.IsTrue(reader.Read());
                    Assert.AreEqual("Paint - Silver", reader["name"]);
                    Assert.AreEqual(4m, reader["turnover"]);
                    Assert.AreEqual(12, reader["employees"]);
                    Assert.AreEqual(5L, reader["rank"]);

                    Assert.IsFalse(reader.Read());
                }
            }
        }
    }
}
