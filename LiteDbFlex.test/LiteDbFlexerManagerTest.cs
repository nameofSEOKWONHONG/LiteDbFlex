using LiteDB;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace LiteDbFlex.test {
    public class LiteDbFlexerManagerTest {
        List<string> additionalDbFileNames = new List<string>() {
            "30000",
            "20000",
            "10000",
            "00000"
        };

        [SetUp]
        public void SetUp() {

        }

        [Test]
        public void Test1() {
            var result1 = LiteDbFlexerManager.Instance.Value.Create<Customer>().Get(1).GetResult<Customer>();
            var result2 = LiteDbFlexerManager.Instance.Value.Create<Customer>().Get(1).GetResult<Customer>();

            var result3 = LiteDbFlexerManager.Instance.Value.Create<Customer>().Insert(new Customer() {
                Name = "seokwon hong",
                Phones = new string[] { "8000-0000", "9000-0000" },
                Age = 30,
                IsActive = true
            }).GetResult<BsonValue>();

            var result4 = LiteDbFlexerManager.Instance.Value.Create<Customer>().Insert(new Customer() {
                Name = "seokwon hong",
                Phones = new string[] { "8000-0000", "9000-0000" },
                Age = 30,
                IsActive = true
            }).GetResult<BsonValue>();

            LiteDbFlexerManager.Instance.Value.Dispose();

            var flexer = LiteDbFlexerManager.Instance.Value.Create<Customer>();
            Assert.Pass();
        }

        [Test]
        public void Test2() {
            foreach(var item in additionalDbFileNames) {
                var result1 = LiteDbFlexerManager.Instance.Value.Create<Customer>(item).Get(1).GetResult<Customer>();
                var result2 = LiteDbFlexerManager.Instance.Value.Create<Customer>(item).Get(1).GetResult<Customer>();

                var result3 = LiteDbFlexerManager.Instance.Value.Create<Customer>(item).Insert(new Customer() {
                    Name = "seokwon hong",
                    Phones = new string[] { "8000-0000", "9000-0000" },
                    Age = 30,
                    IsActive = true
                }).GetResult<BsonValue>();

                var result4 = LiteDbFlexerManager.Instance.Value.Create<Customer>(item).Insert(new Customer() {
                    Name = "seokwon hong",
                    Phones = new string[] { "8000-0000", "9000-0000" },
                    Age = 30,
                    IsActive = true
                }).GetResult<BsonValue>();
            }

            LiteDbFlexerManager.Instance.Value.Dispose();

            var flexer = LiteDbFlexerManager.Instance.Value.Create<Customer>();


            Assert.Pass();
        }
    }
}
