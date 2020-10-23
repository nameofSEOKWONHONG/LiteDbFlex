using NUnit.Framework;
using LiteDbFlex;
using System.Linq;
using LiteDB;
using System.Collections.Generic;

namespace LiteDbFlex.test
{
    public class LitedbBuilderTest
    {
        [Test]
        public void Test1()
        {
            using(var builder = new LitedbFlexBuilder<Customer>())
            {
                var result = builder.BeginTrans()
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
            using(var builder = new LitedbFlexBuilder<Customer>())
            {
                var result = builder.EnsureIndex(m => m.Name, true)
                .Gets()
                .GetResult<IEnumerable<Customer>>();

                Assert.Greater(result.Count(), 0);
            }
        }

        [Test]
        public void Test3() {
            using(var builder = new LitedbFlexBuilder<Customer>())
            {
                var result = builder.EnsureIndex(m => m.Name, true)
                .Get(m => m.Name == "seokwon hong")
                .GetResult<Customer>();

                Assert.NotNull(result);
            }
        }
        
        [Test]
        public void Test4() {
            using(var builder = new LitedbFlexBuilder<Customer>()) {
                var result = builder.EnsureIndex(m => m.Name, true)
                .Gets(null, 1, 1)
                .GetResult<IEnumerable<Customer>>();

                Assert.AreEqual(result.ToList()[0].Id, 2);
            }
        }
    }
}