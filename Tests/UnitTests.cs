using System;
using System.Collections.Generic;
using GameFrameX.Runtime;
using GameFrameX.Network.Runtime;
using NUnit.Framework;

namespace GameFrameX.Network.Tests
{
    internal class UnitTests
    {
        private DateTime dateTime, dateTime1;

        [SetUp]
        public void Setup()
        {
            dateTime = DateTime.Now;
            dateTime1 = DateTime.Now.AddHours(1);
        }

        // Here is an example of a unit test for the IsUnixSameDay method
        [Test]
        public void TestIsUnixSameDay()
        {
            // Arrange
            // long timestamp1 = 1617842400; // April 7, 2021 12:00:00 AM UTC
            // long timestamp2 = 1617896400; // April 7, 2021 12:00:00 PM UTC

            // Act
        }


        [Test]
        public void Test1()
        {
            Assert.That(dateTime1.Year, Is.EqualTo(dateTime.Year));
            Assert.That(dateTime1.Month, Is.EqualTo(dateTime.Month));
            Assert.That(dateTime1.Day, Is.EqualTo(dateTime.Day));
        }

        [Test]
        public void NetworkOptions_ConfigureAndTryGet_RoundTripsDifferentialValues()
        {
            GameFrameXOptions.Reset();
            GameFrameXOptions.Configure(new NetworkOptions
            {
                RpcTimeout = 8000,
                FocusHeartbeat = true,
                IgnoredSendNetworkIds = new List<int> { 1001 },
                IgnoredReceiveNetworkIds = new List<int> { 2002 },
            });

            // 组件 Awake 路径在 EditMode 测试中不可达，退化为直接验证注册表消费语义（TryGet 即组件 Awake 的读取入口）
            bool hit = GameFrameXOptions.TryGet<NetworkOptions>(out var networkOptions);

            Assert.IsTrue(hit, "Configure 后 TryGet 应命中");
            Assert.That(networkOptions.RpcTimeout, Is.EqualTo(8000));
            Assert.IsTrue(networkOptions.FocusHeartbeat);
            Assert.That(networkOptions.IgnoredSendNetworkIds, Is.EqualTo(new List<int> { 1001 }));
            Assert.That(networkOptions.IgnoredReceiveNetworkIds, Is.EqualTo(new List<int> { 2002 }));
        }

        [TearDown]
        public void OptionsTearDown()
        {
            GameFrameXOptions.Reset();
        }
    }
}