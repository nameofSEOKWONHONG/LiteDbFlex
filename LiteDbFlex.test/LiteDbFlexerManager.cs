using LiteDB;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace LiteDbFlex.test {
    public class LiteDbFlexerManager {
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
            var result1 = LiteDbFlexerManager<Customer>.Instance.Value.Create().Get(1).GetResult<Customer>();
            var result2 = LiteDbFlexerManager<Customer>.Instance.Value.Create().Get(1).GetResult<Customer>();

            var result3 = LiteDbFlexerManager<Customer>.Instance.Value.Create().Insert(new Customer() {
                Name = "seokwon hong",
                Phones = new string[] { "8000-0000", "9000-0000" },
                Age = 30,
                IsActive = true
            }).GetResult<BsonValue>();

            var result4 = LiteDbFlexerManager<Customer>.Instance.Value.Create().Insert(new Customer() {
                Name = "seokwon hong",
                Phones = new string[] { "8000-0000", "9000-0000" },
                Age = 30,
                IsActive = true
            }).GetResult<BsonValue>();

            LiteDbFlexerManager<Customer>.Instance.Value.Dispose();

            var flexer = LiteDbFlexerManager<Customer>.Instance.Value.Create();
            Assert.Pass();
        }

        [Test]
        public void Test2() {
            foreach(var item in additionalDbFileNames) {
                var result1 = LiteDbFlexerManager<Customer>.Instance.Value.Create(item).Get(1).GetResult<Customer>();
                var result2 = LiteDbFlexerManager<Customer>.Instance.Value.Create(item).Get(1).GetResult<Customer>();

                var result3 = LiteDbFlexerManager<Customer>.Instance.Value.Create(item).Insert(new Customer() {
                    Name = "seokwon hong",
                    Phones = new string[] { "8000-0000", "9000-0000" },
                    Age = 30,
                    IsActive = true
                }).GetResult<BsonValue>();

                var result4 = LiteDbFlexerManager<Customer>.Instance.Value.Create(item).Insert(new Customer() {
                    Name = "seokwon hong",
                    Phones = new string[] { "8000-0000", "9000-0000" },
                    Age = 30,
                    IsActive = true
                }).GetResult<BsonValue>();
            }

            LiteDbFlexerManager<Customer>.Instance.Value.Dispose();

            var flexer = LiteDbFlexerManager<Customer>.Instance.Value.Create();


            Assert.Pass();
        }
    }
}
