using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace LiteDbFlex.test {
    public class LiteDbCacheSafeFlexerTest : BaseTestClass {
        const int MAX_LOOP = 1000;
        [SetUp]
        public void SetUp() {

        }

        [Test]
        public void Test1() {
            var request = new Customer() {
                Name = "seokwon hong",
                Age = 30
            };

            // 44.3 sec
            Enumerable.Range(1, MAX_LOOP).ToList().ForEach(loop => {
                var result1 = LiteDbSafeFlexer<Customer>.Instance
                .Value
                .SetAdditionalDbFileName()
                .Execute((o) => {
                    return o.Gets(x => x.Name == "seokwon hong" && x.Age == 30)
                    .GetResult<IEnumerable<Customer>>();
                });

                Assert.Greater(result1.Count(), 0);

                var result2 = LiteDbSafeFlexer<Customer>.Instance
                    .Value
                    .SetAdditionalDbFileName()
                    .Execute((o) => {
                        return o.Gets(x => x.Name == "seokwon hong" && x.Age == 30)
                        .GetResult<IEnumerable<Customer>>();
                    });

                Assert.Greater(result2.Count(), 0);
            });

        }

        [Test]
        public void Test2() {
            var request = new Customer() {
                Name = "seokwon hong",
                Age = 30
            };

            // 736 ms
            Enumerable.Range(1, MAX_LOOP).ToList().ForEach(loop => {
                var result1 = LiteDbCacheSafeFlexer<Customer, Customer>.Instance
                    .Value
                    .SetRequest(request)
                    .SetAdditionalDbFileName()
                    .Execute((o, r) => {
                        return o.Gets(x => x.Name == r.Name && x.Age == r.Age)
                        .GetResult<IEnumerable<Customer>>();
                    });

                LiteDbCacheSafeFlexer<Customer, Customer>.Instance.Value.ClearCache();

                Assert.Greater(result1.Count(), 0);

                var result2 = LiteDbCacheSafeFlexer<Customer, Customer>.Instance
                    .Value
                    .SetRequest(request)
                    .SetAdditionalDbFileName()
                    .Execute((o, r) => {
                        return o.Gets(x => x.Name == r.Name && x.Age == r.Age)
                        .GetResult<IEnumerable<Customer>>();
                    });

                LiteDbCacheSafeFlexer<Customer, Customer>.Instance.Value.ClearCache();

                Assert.Greater(result2.Count(), 0);
            });

        }
    }
}
