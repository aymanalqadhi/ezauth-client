using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using EZAuthClient.API;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EZAuthClient.Tests
{
    [TestClass]
    public class EzAuthClientTests
    {
        [TestMethod]
        public void MustFailLogin()
        {
            var client = new EzAuthClient("http://localhost/api");
            Assert.IsFalse(client.Login("root", "123").Result);
        }

        [TestMethod]
        public void MustSucceedLogin()
        {
            var client = new EzAuthClient("http://localhost/api");
            var res = client.Login("admin", "12345678").Result;
            Assert.IsTrue(res);
        }

        [TestMethod]
        public void MustGetValidToken()
        {
            var client = new EzAuthClient("http://localhost/api");
            var res = client.Login("admin", "12345678").Result;
            Assert.IsTrue(Regex.IsMatch(client.Token, @"[a-f0-9]{64}"));
        }

        [TestMethod]
        public void MustGetToken()
        {
            var client = new EzAuthClient("http://localhost/api");
            var res = client.Login("admin", "12345678").Result;
            Assert.IsTrue(Regex.IsMatch(client.Token, @"[a-f0-9]{64}"));
        }

        [TestMethod]
        public void MustSucceedRequestActivation()
        {
            var client = new EzAuthClient("http://localhost/api");
            var res = client.Login("admin", "12345678").Result;

            var af = File.ReadAllBytes("/home/ayman/Projects/ezauth-client/EZAuthClient.Tests/key");
            Assert.IsTrue(client.RequestActivation(af).Result);
        }
        
        [TestMethod]
        public void MustSucceedDecrptLicense()
        {
            var client = new EzAuthClient("http://localhost/api");
            var licenseKey = 
        }
    }
}