using System.Net.Http.Json;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MyCurrencyBot;

class Program
{
  static void Main(string[] args)
  {
    var token = "";
    var bot = new TelegramBotClient(token);

    var options = new ReceiverOptions()
    {
      AllowedUpdates = new UpdateType[] { UpdateType.Message },
    };
    Console.WriteLine("Bot started");
    bot.StartReceiving(UpdateHandler, ErrorHandler, options);
    Console.ReadKey();
  }

  private static Task UpdateHandler(ITelegramBotClient bot, Update update, CancellationToken token)
  {
    Console.WriteLine(JsonConvert.SerializeObject(update));
    if (update.Type != UpdateType.Message)
      return Task.CompletedTask;
    
    long chatId = update.Message.Chat.Id;

    var message = update.Message.Text;
    if (message == "/start")
    {
      bot.SendMessage(chatId, "информация", cancellationToken: token);
    }
    else if (message.StartsWith("/convert"))
    {
      string[] placeholders = message.Split(" ");
      if (placeholders.Length < 4)
      {
        bot.SendMessage(chatId, "Комманда введена неверно. Правильный формат команды: '/convert USD RUB 1000'", cancellationToken: token);
        return Task.CompletedTask;
      }
      string currency1 = placeholders[1];
      string currency2 = placeholders[2];
      double amount = double.Parse(placeholders[3]);
      
      string fixerAccessKey = "";
      var httpClient = new HttpClient();
      httpClient.BaseAddress = new Uri("http://data.fixer.io/api/latest");
      var response = httpClient.Send(new HttpRequestMessage(HttpMethod.Get, $"?access_key={fixerAccessKey}"));
      var contentString = response.Content.ReadAsStringAsync(token);
      var result = JsonConvert.DeserializeObject<Result>(contentString.Result);

      if (!result.Rates.TryGetValue(currency1, out var currency1Coefficient))
      {
        bot.SendMessage(chatId, $"Валюта с кодом {currency1} не поддерживается", cancellationToken: token);
        return Task.CompletedTask;
      }      
      if (!result.Rates.TryGetValue(currency2, out var currency2Coefficient))
      {
        bot.SendMessage(chatId, $"Валюта с кодом {currency2} не поддерживается", cancellationToken: token);
        return Task.CompletedTask;
      }
      double conversionResult = (amount / currency1Coefficient) * currency2Coefficient;

      bot.SendMessage(chatId, $"{amount} {currency1} = {conversionResult:N2} {currency2}", cancellationToken: token);
    }
    else if (message == "/current")
    {
      string fixerAccessKey = "";
      var httpClient = new HttpClient();
      httpClient.BaseAddress = new Uri("http://data.fixer.io/api/latest");
      var response = httpClient.Send(new HttpRequestMessage(HttpMethod.Get, $"?access_key={fixerAccessKey}"));
      var contentString = response.Content.ReadAsStringAsync(token);
      var result = JsonConvert.DeserializeObject<Result>(contentString.Result);
      var eurRate = result.Rates["EUR"];
      var usdRate = result.Rates["USD"];
      var rubRate = result.Rates["RUB"];
      
      var currentRates = $"Курсы валют:\nEUR: {(1 / eurRate) * rubRate:C2}\nUSD: {(1 / usdRate) * rubRate:C2}";
      bot.SendMessage(chatId, currentRates, cancellationToken: token);
    }

    return Task.CompletedTask;
  }

  private static Task ErrorHandler(ITelegramBotClient arg1, Exception arg2, HandleErrorSource arg3, CancellationToken arg4)
  {
    throw new NotImplementedException();
  }
}