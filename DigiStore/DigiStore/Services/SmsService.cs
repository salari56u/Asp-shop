using DigiStore.Web.Services; // Namespace پروژه فعلی
using Kavenegar;

namespace DigiStore.Web.Services
{
    public class SmsService : ISmsService
    {
        private readonly string _apiKey;
        private readonly string _senderNumber;

        // تزریق IConfiguration برای خواندن تنظیمات
        public SmsService(IConfiguration configuration)
        {
            _apiKey = configuration["Kavenegar:ApiKey"];
            _senderNumber = configuration["Kavenegar:SenderNumber"];
        }

        public async Task SendSmsAsync(string phoneNumber, string message)
        {
            try
            {
                // چون کتابخانه کاوه نگار Sync است، آن را در یک Task.Run می گذاریم
                // تا برنامه اصلی را قفل نکند (ترد را آزاد کند)
                await Task.Run(() =>
                {
                    var api = new KavenegarApi(_apiKey);
                    var result = api.Send(_senderNumber, phoneNumber, message);

                    // جهت اطمینان در کنسول چاپ میکنیم (فقط برای تست)
                    Console.WriteLine($"Kavenegar Result: {result.StatusText}");
                });
            }
            catch (Kavenegar.Exceptions.ApiException ex)
            {
                // خطاهای سمت کاوه نگار (مثل کلید اشتباه یا گیرنده مسدود)
                Console.WriteLine($"Kavenegar API Error: {ex.Message}");
                // throw; // فعلا خطا را پرتاب نمیکنیم تا سایت کرش نکند
            }
            catch (Kavenegar.Exceptions.HttpException ex)
            {
                // خطاهای شبکه و اینترنت
                Console.WriteLine($"Kavenegar HTTP Error: {ex.Message}");
            }
        }
    }
}