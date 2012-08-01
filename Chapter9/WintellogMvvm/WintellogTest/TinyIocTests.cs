using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PortableWintellog;

namespace WintellogTest
{
    [TestClass]
    public class TinyIocTests
    {
        private ITinyIoc _target;

        [TestInitialize]
        public void TestInitialize()
        {
            _target = new TinyIoc();
        }

        [TestMethod]
        public void GivenIocContainerWhenInterfaceRequestThenContainerShouldBeReturned()
        {
            var actual = _target.Resolve<ITinyIoc>();
            Assert.AreSame(_target, actual, "Test failed: instance was not returned.");
        }

        [TestMethod]
        public void GivenTypeConfiguredWhenTypeRequestedThenTypeShouldBeReturned()
        {
            _target.Register<object>(tinyIoc => new {id = Guid.NewGuid()});
            dynamic expected = _target.Resolve<object>();
            Assert.IsInstanceOfType(expected.id, typeof (Guid), "Test failed: dynamic type was not returned.");
        }

        [TestMethod]
        public void GivenTypeConfiguredWhenSharedInstanceRequestedThenSharedInstanceShouldBeReturned()
        {
            _target.Register<object>(tinyIoc => new {id = Guid.NewGuid()});
            var expected = _target.Resolve<object>();
            var actual = _target.Resolve<object>();
            Assert.AreSame(expected, actual, "Test failed: the same instance was not returned.");
        }

        [TestMethod]
        public void GivenTypeConfiguredWhenNewInstanceRequestedThenDifferentInstanceShouldBeReturned()
        {
            _target.Register<object>(tinyIoc => new {id = Guid.NewGuid()});
            var firstInstance = _target.Resolve<object>();
            var newInstance = _target.Resolve<object>(true);
            Assert.IsInstanceOfType(firstInstance, newInstance.GetType(),
                "Test failed: same type was not returned.");
            Assert.AreNotSame(firstInstance, newInstance, "Test failed: new instance was not returned.");
        }
    }
}
