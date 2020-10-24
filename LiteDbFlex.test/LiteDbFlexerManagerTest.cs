using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LiteDbFlex.test {
    public class LiteDbFlexerManagerTest {
        [Test]
        public void Test1() {
            var result1 = LiteDbSafeFlexer<Customer>
                .Instance
                .Value
                .SetAdditionalName()
                .Execute<Customer>(o => {
                    return o.Get(1).GetResult<Customer>();
                });

            Assert.NotNull(result1);
        }

        [Test]
        public async Task Test2() {
            var result1 = await LiteDbSafeFlexer<Customer>
                .Instance
                .Value
                .SetAdditionalName()
                .ExecuteAsync<Customer>(o => {
                    return o.Get(1).GetResult<Customer>();
                });

            var result2 = await LiteDbSafeFlexer<Customer>
                .Instance
                .Value
                .SetAdditionalName()
                .ExecuteAsync<Customer>(o => {
                    return o.Get(1).GetResult<Customer>();
                });

            Assert.NotNull(result1);
        }
    }
}
