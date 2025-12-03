using DigiStore.Web.Services; // Namespace پروژه فعلی
using Kavenegar;

namespace DigiStore.Web.Services
{
    public class SmsService : ISmsService
    {
        private readonly string _apiKey;
        private readonly string _senderNumber;

        public SmsService(IConfiguration configuration)
        {
            _apiKey = configuration["Kavenegar:ApiKey"];
            _senderNumber = configuration["Kavenegar:SenderNumber"];
        }

        public async Task SendSmsAsync(string phoneNumber, string message)
        {
            try
            {
                await Task.Run(() =>
                {
                    var api = new KavenegarApi(_apiKey);
                    var result = api.Send(_senderNumber, phoneNumber, message);
                    Console.WriteLine($"Kavenegar Result: {result.StatusText}");
                });
            }
            catch (Kavenegar.Exceptions.ApiException ex)
            {
                Console.WriteLine($"Kavenegar API Error: {ex.Message}");
            }
            catch (Kavenegar.Exceptions.HttpException ex)
            {
                Console.WriteLine($"Kavenegar HTTP Error: {ex.Message}");
            }
        }
    }
}