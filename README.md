# LiteDbFlex
litedb extensions

[usage]

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

[builder usage]
``` csharp
    //insert
    using(var builder = new LitedbFlexBuilder<Customer>())
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

    //getall
    using(var builder = new LitedbFlexBuilder<Customer>())
    {
        var result = builder
        .Gets()
        .GetResult<IEnumerable<Customer>>();

        Assert.Greater(result.Count(), 1);
    }

    //where get
    using(var builder = new LitedbFlexBuilder<Customer>())
    {
        var result = builder
        .Get(m => m.Name == "seokwon hong")
        .GetResult<Customer>();

        Assert.NotNull(result);
    }
    
    //get skip, take
    using(var builder = new LitedbFlexBuilder<Customer>()) {
        var result = builder
        .Gets(null, 0, 1)
        .GetResult<IEnumerable<Customer>>();

        Assert.AreEqual(result.ToList()[0].Id, 1);
    }    
```