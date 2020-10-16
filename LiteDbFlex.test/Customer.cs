using System;
using System.Collections.Generic;
using System.Text;

namespace LiteDbFlex.test {
    [LiteDbTable("MyData.db", "customers")]
    public class Customer {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string[] Phones { get; set; }
        public bool IsActive { get; set; }
    }
}
