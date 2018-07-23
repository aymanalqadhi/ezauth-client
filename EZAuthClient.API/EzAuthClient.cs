using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EZAuthClient.API
{
    public class EzAuthClient
    {
        private readonly HttpClient _client;
        private readonly HttpClientHandler _clienthandler;
        private readonly CookieContainer _cookieContainer;

        #region Public Properties

        public string BaseUrl { get; private set; }
        public string Token { get; private set; }
        public string ErrorMessage { get; private set; }
        public bool IsLoggedIn { get; private set; }

        #endregion

        public EzAuthClient(string baseUrl)
        {
            BaseUrl = baseUrl;

            _cookieContainer = new CookieContainer();
            _clienthandler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                UseCookies = true,
                CookieContainer = _cookieContainer
            };

            _client = new HttpClient(_clienthandler);
            _client.BaseAddress = new Uri(baseUrl);
        }

        public async Task<bool> Login(string username, string password)
        {
            var paramsList = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("password", password)
            };

            var form = new FormUrlEncodedContent(paramsList);
            var res = await _client.PostAsync(_client.BaseAddress + "/login", form);
            var ret = (await res.Content.ReadAsStringAsync()).Trim();

            if (ret.Length == 64 && res.StatusCode == HttpStatusCode.OK)
            {
                Token = ret;
                IsLoggedIn = true;
                return true;
            }
            else
            {
                ErrorMessage = ret;
                IsLoggedIn = false;
                return false;
            }
        }

        public async Task<bool> RefreshToken()
        {
            var res = await _client.GetAsync("/token");

            if (res.StatusCode == HttpStatusCode.OK)
            {
                Token = await res.RequestMessage.Content.ReadAsStringAsync();
                return true;
            }

            return false;
        }

        public async Task<bool> RequestActivation(byte[] activationFile)
        {
            var hwId = InfoHelpers.GetHardwareId();

            if (!IsLoggedIn || string.IsNullOrEmpty(Token))
                throw new Exception("Access Denied!");

            using (var formData = new MultipartFormDataContent())
            {
                formData.Add(new StringContent(Token), string.Format("\"{0}\"", "token"));
                formData.Add(new ByteArrayContent(activationFile), "data", "data");
                formData.Add(new StringContent(hwId), string.Format("\"{0}\"", "hw_id"));

                var res = await _client.PostAsync(_client.BaseAddress + "/activate", formData);

                if (res.IsSuccessStatusCode) return true;
                throw new Exception(await res.Content.ReadAsStringAsync());
            }
        }

        public Task<string> DecryptKey(byte[] licenseKey, string decryptionKey)
        {
            if (decryptionKey.Length != 64)
                throw new ArgumentException("Invalid Key!", "decryptionKey");

            return Task.Run(() =>
            {
                using (var aes = AesManaged.Create())
                {
                    aes.Key = Encoding.ASCII.GetBytes(decryptionKey);
                    aes.IV = null;

                    var buffer = new byte[1024];
                    if (aes.CreateDecryptor().TransformBlock(licenseKey, 0, licenseKey.Length, buffer, 0) < 0)
                        throw new Exception("Decryption Error!");

                    return Encoding.ASCII.GetString(buffer);
                }
            });
        }
    }
}