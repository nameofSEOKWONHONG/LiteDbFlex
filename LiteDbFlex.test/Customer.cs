using System;
using System.Collections.Generic;
using System.Text;

namespace LiteDbFlex.test {
    [LiteDbTable("MyData.db", "customers")]
    [LiteDbIndex(
        new[] { "Name" }, 
        new[] { true }
    )]
    public class Customer {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string[] Phones { get; set; }
        public bool IsActive { get; set; }
    }

    [LiteDbTable("MyData.db", "orders")]
    public class Order {
        public int Id {get; set;}
        public string Menu {get;set;}
        public decimal Price {get;set;}
        public int Amt {get;set;}
        public decimal TotalPrice {get {return this.Price * this.Amt;}}
        public Customer Customer {get;set;}
    }
     
}
