using NUnit.Framework;
using LiteDbFlex;
using System.Linq;
using LiteDB;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

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
                Menu = "hamburger",
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
            var customers = LiteDbSafeFlexer<Customer>
                .Instance.Value
                .Execute<IEnumerable<Customer>>(o => {
                    return o.Gets().GetResult<IEnumerable<Customer>>();
                });
            Random random = new Random();
            foreach (var customer in customers) {
                var order = new Order() {
                    Menu = "hamburger",
                    Amt = random.Next(10),
                    Price = 7500,
                    Customer = customer
                };
                var result = LiteDbSafeFlexer<Order>
                    .Instance.Value
                    .Execute(o => o.BeginTrans().Insert(order).Commit().GetResult<BsonValue>());
                Assert.Greater((int)result, 0);
            }
        }

        [Test]
        public void Test2() {
            using(var orderBuilder = new LiteDbFlexer<Order>(additionalNameOrder))
            {
                var results = orderBuilder.GetEnumerable(m => m.Menu == "hamburger");
                using(var customerBuilder = new LiteDbFlexer<Customer>(additionalNameCustomer))
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

        [Test]
        public async Task Test3() {
            var result1 = LiteDbSafeFlexer<Order>.Instance
                .Value
                .SetAdditionalDbFileName()
                .Execute<IEnumerable<Order>>(o => {
                    return o.Gets().GetResult<IEnumerable<Order>>();
                });

            foreach(var order in result1) {
                var customer = await LiteDbSafeFlexer<Customer>.Instance
                    .Value.SetAdditionalDbFileName().ExecuteAsync<Customer>(o => {
                        return o.Get(order.Customer.Id).GetResult<Customer>();
                    });

                Assert.NotNull(customer);
            }
        }
    }
}