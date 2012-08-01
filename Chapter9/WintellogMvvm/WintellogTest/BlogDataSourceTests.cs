using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PortableWintellog.Data;
using WintellogTest.TestHelpers;

namespace WintellogTest
{
    [TestClass]
    public class BlogDataSourceTests
    {
        private BlogDataSource _target;        
        private StorageUtilityTest _storageUtilityTest;
        private DialogTest _dialogTest;
        private ApplicationContextTest _applicationContextTest;
        private SyndicationHelperTest _syndicationHelperTest;

        [TestInitialize]
        public void TestInitialize()
        {
            _dialogTest = new DialogTest();
            _applicationContextTest = new ApplicationContextTest();
            _syndicationHelperTest = new SyndicationHelperTest();
            _storageUtilityTest = new StorageUtilityTest();
            _target = new BlogDataSource(
                _storageUtilityTest,
                _applicationContextTest,
                _dialogTest,
                _syndicationHelperTest);
        }

        [TestMethod]
        public void GivenNewInstanceThenListShouldNotBeNull()
        {
            Assert.IsNotNull(_target.GroupList, "Test failed: group list should not be empty.");
        }

        [TestMethod]
        public void GivenInvalidGroupIdWhenRequestedThenShouldReturnNull()
        {
            var actual = _target.GetGroup("123");
            Assert.IsNull(actual, "Test failed: actual group returned should be null.");
        }

        [TestMethod]
        public void GivenValidGroupIdWhenRequestedThenShouldReturnGroup()
        {
            var expected = new BlogGroup {Id = Guid.NewGuid().ToString()};
            _target.GroupList.Add(expected);
            var actual = _target.GetGroup(expected.Id);
            Assert.AreSame(expected, actual, "Test failed: actual group does not match expected.");
        }

        [TestMethod]
        public void GivenInvalidItemIdWhenRequestedThenShouldReturnNull()
        {
            var actual = _target.GetItem("123");
            Assert.IsNull(actual, "Test failed: actual group returned should be null.");
        }

        [TestMethod]
        public void GivenValidItemIdWhenRequestedThenShouldReturnItem()
        {
            var expected = new BlogItem {Id = Guid.NewGuid().ToString()};
            var group = new BlogGroup {Id = Guid.NewGuid().ToString()};
            group.Items.Add(expected);
            _target.GroupList.Add(group);
            var actual = _target.GetItem(expected.Id);
            Assert.AreSame(expected, actual, "Test failed: actual item does not match expected.");
        }
        
        [TestMethod]
        public async Task GivenNewItemsExistWhenGroupIsPopulatedThenTotalAndNewItemsShouldBeCorrect()
        {
            // simulate a cached item
            var cached = new BlogItem {Id = Guid.NewGuid().ToString()};
            
            // give a false hash code
            _storageUtilityTest.Items = new[] {"123"};

            // simulate a new item 
            var newItem = new BlogItem {Id = Guid.NewGuid().ToString()};
            _syndicationHelperTest.BlogItems.Add(newItem);

            // simulate a blog 
            var blog = new BlogGroup {Id = Guid.NewGuid().ToString()};
            _syndicationHelperTest.BlogGroups.Add(blog);

            _storageUtilityTest.Restore = (type, folder, hashCode) =>
                {
                    if (type == typeof(BlogGroup))
                    {
                        return blog;
                    }

                    return cached;
                };

            // simulate loading groups
            await _target.LoadGroups();

            // should be one group
            Assert.AreEqual(1, _target.GroupList.Count, "Task failed: should have generated one group.");

            // now simulate loading items
            foreach(var group in _target.GroupList)
            {
                await _target.LoadAllItems(group);
            }

            // should have a total of 2 
            Assert.AreEqual(2, _target.GroupList[0].ItemCount, "Test failed: item count should have been 2.");

            // should have a total new of 1 
            Assert.AreEqual(1, _target.GroupList[0].NewItemCount, "Test failed: new item count should have been 1.");

            // list should match
            CollectionAssert.AreEquivalent(new[] { cached, newItem }, _target.GroupList[0].Items, "Test failed: lists do not match.");

            // dialog should have been called with an error message
            Assert.IsTrue(!string.IsNullOrEmpty(_dialogTest.Message), "Test failed: dialog was not called with error message.");
        }
    }
}
