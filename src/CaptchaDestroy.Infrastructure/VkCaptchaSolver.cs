using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CaptchaDestroy.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace CaptchaDestroy.Infrastructure
{
    public class VkCaptchaSolver : IVkCaptchaSolver
    {
        private readonly ILogger<VkCaptchaSolver> _logger;

        public VkCaptchaSolver(ILogger<VkCaptchaSolver> logger)
        {
            _logger = logger;
        }

        public async Task<string> SolveCaptcha(Uri uri)
        {
            var solver = new CptchCaptchaSolver();
            return solver.Solve(uri.ToString());
            //_logger.LogWarning($"Sending email to {to} from {from} with subject {subject}.");
        }
    }
    public class CptchCaptchaSolver
    {
        private const String _CPTCH_UPLOAD_URL = "http://localhost:{0}/getsolution";
        private String CPTCH_UPLOAD_URL
        {
            get
            {
                CurrentCaptchaCount %= CountPorts;
                return String.Format(_CPTCH_UPLOAD_URL, StartPort + CurrentCaptchaCount++);
            }
        }
        private const int StartPort = 3000;
        private const int CountPorts = 1;
        private static int CurrentCaptchaCount = 0;

        public CptchCaptchaSolver()
        {

        }

        public string Solve(string url)
        {
            Console.WriteLine("Решаем капчу: " + url);
            //Скачиваем файл капчи из Вконтакте
            byte[] captcha = DownloadCaptchaFromVk(url);
            if (captcha != null)
            {
                string uploadResponse = UploadCaptchaToCptch(captcha);
                var solution = ParseSolutionResponse(uploadResponse);

                return solution;

            }
            else
            {
                Console.WriteLine("Не удалось скачать капчу с Вконтакте");
            }

            return null;
        }


        private byte[] DownloadCaptchaFromVk(string captchaUrl)
        {
            using (WebClient client = new WebClient())
            using (Stream s = client.OpenRead(captchaUrl))
            {
                return client.DownloadData(captchaUrl);
            }
        }

        private string UploadCaptchaToCptch(byte[] captcha)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                MultipartFormDataContent form = new MultipartFormDataContent();

                form.Add(new StringContent("post"), "method");
                form.Add(new ByteArrayContent(captcha, 0, captcha.Length), "file", "captcha");
                var response = httpClient.PostAsync(CPTCH_UPLOAD_URL, form).Result;
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = response.Content;
                    return responseContent.ReadAsStringAsync().Result;
                }
                else
                {
                    return null;
                }
            }
        }



        private string ParseSolutionResponse(string response)
        {
            if (response.Equals("ERROR"))
            {
                Console.WriteLine("Ошибка во время получения ответа: " + response);
                return null;
            }
            else if (response.Equals("CAPCHA_NOT_READY"))
            {
                Console.WriteLine("Капча еще не готова");
                Thread.Sleep(1000);
                return null;
            }
            else if (response.Equals("ERROR_CAPTCHA_UNSOLVABLE"))
            {
                Console.WriteLine("Капча не может быть решена. СЛОЖНААА! СЛОЖНААААА!");
                return "qwef23";
            }
            else if (response.Contains("OK"))
            {
                return response.Split('|')[1];
            }
            return null;
        }

        public void CaptchaIsFalse()
        {
            Console.WriteLine("Последняя капча была распознана неверно");
        }
    }

}