using NUnit.Framework;
using LiteDbFlex;
using System.Linq;

namespace LiteDbFlex.test {
    public class LiteDbFlexTest {
        Customer addCustomer = null;
        [SetUp]
        public void Setup() {
            addCustomer = new Customer() {
                Name = "seokwon hong",
                Phones = new string[] { "8000-0000", "9000-0000" },
                Age = 30,
                IsActive = true
            };
        }

        [Test]
        public void InsertTest() {
            using (var db = LiteDbResolver.Resolve<Customer>()) {
                var tran = db.jBeginTrans();
                var addId = tran.jGetCollection<Customer>()
                    .jInsert(addCustomer);
                tran.jCommit();

                Assert.Greater((int)addId, 0);
            }
        }

        [Test]
        public void GetTest() {
            using(var db = LiteDbResolver.Resolve<Customer>()) {
                var customer = db.jGetCollection<Customer>()
                    .jGet(1);

                Assert.NotNull(customer);
            }
        }

        [Test]
        public void GetAllTest() {
            using(var db = LiteDbResolver.Resolve<Customer>()) {
                var customers = db.jGetCollection<Customer>().jGetAll().ToList();

                Assert.Greater(customers.Count, 0);
            }
        }

        [Test]
        public void DeleteTest() {
            using(var db = LiteDbResolver.Resolve<Customer>()) {
                var exists = db.jGetCollection<Customer>().jGet(x => x.Id == 2);
                if (exists == null) Assert.Fail();
                var tran = db.jBeginTrans();
                var result = db.jGetCollection<Customer>().jDelete(exists.Id);
                if(result) {
                    tran.jCommit();
                }
                Assert.IsTrue(result);
            }
        }
    }
}