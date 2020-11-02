using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace LiteDbFlex.test {
    public class LiteDbCacheFlexerTest {
        [Test]
        public void litedb_cache_performance() {
            LiteDbCacheFlexer.Instance.Value.DropCollection<Customer>();

            Enumerable.Range(1, 1000).ToList().ForEach(i => {
                LiteDbCacheFlexer.Instance.Value.SetCache<Customer>(() => {
                    return new CacheInfo<Customer>() {
                        Data = new Customer() {
                            Name = "seokwon hong",
                            Phones = new string[] { "8000-0000", "9000-0000" },
                            Age = 30,
                            IsActive = true
                        },
                        CacheName = "CustomerCache",
                        Interval = 1,
                        SetTime = DateTime.Now
                    };
                });

                var customer = LiteDbCacheFlexer.Instance.Value.GetCache<Customer>("CustomerCache");
            });

            LiteDbCacheFlexer.Instance.Value.Dispose();

            Assert.Pass();
        }

        [Test]
        public void litedb_cache_test() {
            LiteDbCacheFlexer.Instance.Value.DropCollection<Customer>();
            LiteDbCacheFlexer.Instance.Value.SetCache<Customer>(() => {
                return new CacheInfo<Customer>() {
                    Data = new Customer() {
                        Name = "seokwon hong",
                        Phones = new string[] { "8000-0000", "9000-0000" },
                        Age = 30,
                        IsActive = true
                    },
                    CacheName = "CustomerCache",
                    Interval = 1,
                    SetTime = DateTime.Now
                };
            });

            Thread.Sleep(1000);


            var customer = LiteDbCacheFlexer.Instance.Value.GetCache<Customer>("CustomerCache");
            Assert.NotNull(customer);
            Assert.AreEqual(customer.EnumCacheState, ENUM_CACHE_STATE.DELETED);


            LiteDbCacheFlexer.Instance.Value.Dispose();
        }

        [Test]
        public void litedb_cache_dont_set_settime_test() {
            LiteDbCacheFlexer.Instance.Value.DropCollection();
            LiteDbCacheFlexer.Instance.Value.SetCache<Customer>(() => {
                return new CacheInfo<Customer>() {
                    Data = new Customer() {
                        Name = "seokwon hong",
                        Phones = new string[] { "8000-0000", "9000-0000" },
                        Age = 30,
                        IsActive = true
                    },
                    CacheName = "CustomerCache",
                    Interval = 1,
                    SetTime = null
                };
            });

            Thread.Sleep(2000);

            var customer = LiteDbCacheFlexer.Instance.Value.GetCache<Customer>("CustomerCache");
            Assert.NotNull(customer);
            Assert.AreEqual(customer.EnumCacheState, ENUM_CACHE_STATE.NORMAL);


            LiteDbCacheFlexer.Instance.Value.Dispose();
        }
    }
}
