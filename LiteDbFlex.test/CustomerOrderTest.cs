using NUnit.Framework;
using LiteDbFlex;
using System.Linq;
using LiteDB;
using System.Collections.Generic;
using System;

namespace LiteDbFlex.test
{
    public class CustomerOrderTest : BaseTestClass
    {
        string additionalNameOrder = DateTime.Now.ToString("yyyyMMdd") + "_order";
        string additionalNameCustomer = DateTime.Now.ToString("yyyyMMdd") + "_customer";

        Order order;
        
        [SetUp]
        public void SetUp() {
            order = new Order() {
                Menu = "hambugger",
                Amt = 1,
                Price = 4800,
                Customer = new Customer() {
                    Id = 1,
                    Name = "seokwon hong",
                    Phones = new string[] { "8000-0000", "9000-0000" },
                    Age = 30,
                    IsActive = true
                }
            };
        }

        [Test]
        public void Test1()
        {
            using(var customerBuilder =  new LitedbFlexBuilder<Customer>(additionalNameCustomer)) {
                var results = customerBuilder.Gets().GetResult<IEnumerable<Customer>>();
                using(var builder = new LitedbFlexBuilder<Order>(additionalNameOrder))
                {
                    Random random = new Random();
                    foreach(var customer in results) {
                        var order = new Order() {
                            Menu = "hambugger",
                            Amt = random.Next(10),
                            Price = 4800,
                            Customer = customer
                        };
                        var result = builder.BeginTrans()
                        .EnsureIndex(m => m.Id, true)
                        .Insert(order)
                        .Commit()
                        .GetResult<BsonValue>();

                        Assert.Greater(result, 0);
                    }
                }
            }
        }

        [Test]
        public void Test2() {
            using(var orderBuilder = new LitedbFlexBuilder<Order>(additionalNameOrder))
            {
                var results = orderBuilder.GetEnumerable(m => m.Menu == "hambugger");
                using(var customerBuilder = new LitedbFlexBuilder<Customer>(additionalNameCustomer))
                {
                    foreach(var order in results) 
                    {
                        var customer = customerBuilder.Get(order.Customer.Id)
                            .GetResult<Customer>();
                        Assert.NotNull(customer);
                    }
                }

                Assert.Greater(results.Count(), 0);
            }
        }
    }
}