using NUnit.Framework;
using LiteDbFlex;
using System.Linq;
using LiteDB;
using System.Collections.Generic;
using System;

namespace LiteDbFlex.test
{
    public class OverhitTest : BaseTestClass
    {
        string additionalNameOrder = DateTime.Now.ToString("yyyyMMdd") + "_order";
        string additionalNameCustomer = DateTime.Now.ToString("yyyyMMdd") + "_customer";
        
        [SetUp]
        public void SetUp() {

        }

        [Test]
        public void Test1()
        {
            using(var builder = new LiteDbFlexer<Customer>(additionalNameCustomer))
            {
                Enumerable.Range(1, 5000).ToList().ForEach(i => {
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
                });
            }
        }

        [Test]
        public void Test2() {
            using(var builder = new LiteDbFlexer<Customer>(additionalNameCustomer))
            {
                var results = builder.GetEnumerable(m => m.Name == "seokwon hong");
                foreach(var customer in results) {
                    Assert.NotNull(customer);
                }
            }
        }
    }
}