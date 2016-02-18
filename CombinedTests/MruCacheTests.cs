using NUnit.Framework;
using RedChess.ChessCommon;

namespace RedChess.CombinedTests
{
    [TestFixture]
    class MruCacheTests
    {
        private MruCache<int, int> m_cache;

        [SetUp]
        public void Setup()
        {
            m_cache = new MruCache<int, int>(3);
        }

        [Test]
        public void EmptyCache()
        {
            int answer;
            Assert.IsFalse(m_cache.TryGet(1, out answer), "Expected new cache to be empty");
        }

        [Test]
        public void ElementsUpToSizeAreCached()
        {
            int answer;
            m_cache.Set(1, 11);
            m_cache.Set(2, 12);
            m_cache.Set(3, 13);

            Assert.IsTrue(m_cache.TryGet(1, out answer));
            Assert.AreEqual(11,answer, "Wrong value from cache");
            Assert.IsTrue(m_cache.TryGet(2, out answer));
            Assert.AreEqual(12, answer, "Wrong value from cache");
            Assert.IsTrue(m_cache.TryGet(3, out answer));
            Assert.AreEqual(13, answer, "Wrong value from cache");
        }

        [Test]
        public void OldestElementIsDeleted()
        {
            int answer;
            m_cache.Set(1, 11);
            m_cache.Set(2, 12);
            m_cache.Set(3, 13);
            m_cache.Set(4, 14);

            Assert.IsFalse(m_cache.TryGet(1, out answer));

            Assert.IsTrue(m_cache.TryGet(2, out answer));
            Assert.AreEqual(12, answer, "Wrong value from cache");
            Assert.IsTrue(m_cache.TryGet(3, out answer));
            Assert.AreEqual(13, answer, "Wrong value from cache");
            Assert.IsTrue(m_cache.TryGet(4, out answer));
            Assert.AreEqual(14, answer, "Wrong value from cache");
        }

        [Test]
        public void AtMostSizeElementsAreKept()
        {
            int answer;
            for(int i=0;i<21;i++)
                m_cache.Set(i, i*10 + 1);

            Assert.IsTrue(m_cache.TryGet(18, out answer));
            Assert.AreEqual(181, answer, "Wrong value from cache");
            Assert.IsTrue(m_cache.TryGet(19, out answer));
            Assert.AreEqual(191, answer, "Wrong value from cache");
            Assert.IsTrue(m_cache.TryGet(20, out answer));
            Assert.AreEqual(201, answer, "Wrong value from cache");

            for(int i=0;i<17;i++)
                Assert.IsFalse(m_cache.TryGet(i, out answer),"Expected no cache data for this key");
        }

    }
}
