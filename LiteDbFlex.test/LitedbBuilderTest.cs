using NUnit.Framework;
using LiteDbFlex;
using System.Linq;
using LiteDB;
using System.Collections.Generic;
using System;

namespace LiteDbFlex.test
{
    public class LitedbBuilderTest
    {
        string additionalName = DateTime.Now.ToString("yyyyMMdd") + "_customer";
        
        [SetUp]
        public void SetUp() {

        }

        [Test]
        public void Test1()
        {
            using(var builder = new LitedbFlexBuilder<Customer>(additionalName))
            {
                builder.DropIndex("Name");
                var result = builder.BeginTrans()
                .EnsureIndex(m => m.Id, true)
                .Insert(new Customer() {
                    Name = "seokwon hong",
                    Phones = new string[] { "8000-0000", "9000-0000" },
                    Age = 30,
                    IsActive = true
                })
                .Commit()
                .GetResult<BsonValue>();

                Assert.Greater((int)result, 0);
            }
        }

        [Test]
        public void Test2() {
            using(var builder = new LitedbFlexBuilder<Customer>(additionalName))
            {
                var result = builder
                .Gets()
                .GetResult<IEnumerable<Customer>>();

                Assert.Greater(result.Count(), 1);
            }
        }

        [Test]
        public void Test3() {
            using(var builder = new LitedbFlexBuilder<Customer>(additionalName))
            {
                var result = builder
                .Get(m => m.Name == "seokwon hong")
                .GetResult<Customer>();

                Assert.NotNull(result);
            }
        }
        
        [Test]
        public void Test4() {
            using(var builder = new LitedbFlexBuilder<Customer>(additionalName)) {
                var result = builder
                .Gets(null, 0, 1)
                .GetResult<IEnumerable<Customer>>();

                Assert.AreEqual(result.ToList()[0].Id, 1);
            }
        }

        [Test]
        public void ExceptionTest1() {
            using(var builder = new LitedbFlexBuilder<Customer>(additionalName)) {
                var result = builder
                .Gets(null, 0, 1)
                .GetResult<IEnumerable<Customer>>();

                Assert.AreEqual(result.ToList()[0].Id, 1);
                //throw exception
                using(var builder2 = new LitedbFlexBuilder<Customer>(additionalName)) {
                    var result2 = builder2
                    .Gets(null, 0, 1)
                    .GetResult<IEnumerable<Customer>>();

                    Assert.AreEqual(result2.ToList()[0].Id, 1);
                }                
            }
        }
    }
}