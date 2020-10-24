# LiteDbFlex
implement litedb extensions and chain methods

[usage]
1. LiteDb Extension Method (LiteDbFluentMethods)

    ```csharp
        [LiteDbTable("MyData.db", "customers")]
        public class Customer {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Age { get; set; }
            public string[] Phones { get; set; }
            public bool IsActive { get; set; }
        }

        //insert
        var addCustomer = new Customer() {
            Name = "seokwon hong",
            Phones = new string[] { "8000-0000", "9000-0000" },
            Age = 30,
            IsActive = true
        };

        using (var db = LiteDbResolver.Resolve<Customer>()) {
            var tran = db.jBeginTrans();
            var addId = tran.jGetCollection<Customer>()
                .jInsert(addCustomer);
            tran.jCommit();
        }

        //get
        using(var db = LiteDbResolver.Resolve<Customer>()) {
            var customer = db.jGetCollection<Customer>()
                .jGet(1);
        }

        //getall
        using(var db = LiteDbResolver.Resolve<Customer>()) {
            var customers = db.jGetCollection<Customer>().jGetAll().ToList();
        }

        //delete
        using(var db = LiteDbResolver.Resolve<Customer>()) {
            var exists = db.jGetCollection<Customer>().jGet(x => x.Id == 2);
            if (exists == null) Assert.Fail();
            var tran = db.jBeginTrans();
            var result = db.jGetCollection<Customer>().jDelete(exists.Id);
            if(result) {
                tran.jCommit();
            }
        }
    ```

2. LiteDbFlexer

``` csharp
        [LiteDbTable("MyData.db", "customers")]
        public class Customer {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Age { get; set; }
            public string[] Phones { get; set; }
            public bool IsActive { get; set; }
        }

        string additionalName = DateTime.Now.ToString("yyyyMMdd") + "_customer";
        
        [SetUp]
        public void SetUp() {

        }

        [Test]
        public void Test1()
        {
            using(var builder = new LiteDbFlexer<Customer>(additionalName))
            {
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
            using(var builder = new LiteDbFlexer<Customer>(additionalName))
            {
                var result = builder
                .Gets()
                .GetResult<IEnumerable<Customer>>();

                Assert.Greater(result.Count(), 1);
            }
        }

        [Test]
        public void Test3() {
            using(var builder = new LiteDbFlexer<Customer>(additionalName))
            {
                var result = builder
                .Get(m => m.Name == "seokwon hong")
                .GetResult<Customer>();

                Assert.NotNull(result);
            }
        }
        
        [Test]
        public void Test4() {
            using(var builder = new LiteDbFlexer<Customer>(additionalName)) {
                var result = builder
                .Gets(null, 0, 1)
                .GetResult<IEnumerable<Customer>>();

                Assert.AreEqual(result.ToList()[0].Id, 1);
            }
        }

        [Test]
        public void ExceptionTest1() {
            using(var builder = new LiteDbFlexer<Customer>(additionalName)) {
                var result = builder
                .Gets(null, 0, 1)
                .GetResult<IEnumerable<Customer>>();

                Assert.AreEqual(result.ToList()[0].Id, 1);
                //throw exception
                using(var builder2 = new LiteDbFlexer<Customer>(additionalName)) {
                    var result2 = builder2
                    .Gets(null, 0, 1)
                    .GetResult<IEnumerable<Customer>>();

                    Assert.AreEqual(result2.ToList()[0].Id, 1);
                }                
            }
        }
```

3. LiteDbSafeFlexer
```csharp
    var result1 = LiteDbSafeFlexer<Customer>
        .Instance
        .Value
        .SetAdditionalName()
        .Execute<Customer>(o => {
            return o.Get(1).GetResult<Customer>();
        });
```