using System;
using System.Security.Cryptography;

namespace EZAuthClient.API
{
    public static class InfoHelpers
    {
        public static string GetHardwareId()
        {
            using(var sha256 = SHA256.Create())
            {
                var hwid = System.Environment.MachineName;
                return hwid;
            }
        }
    }
}