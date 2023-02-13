using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Net;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot;
using Newtonsoft.Json;

var botClient = new TelegramBotClient("5606653542:AAHzpBel_JoCbPa2uOeQv9kZMF9EwJoC8NI");
using var cts = new CancellationTokenSource();
string url = "https://api.openweathermap.org/data/2.5/weather?q=Moscow&units=metric&appid=2eec61c6b82ecc52dd8349e111e747a7";
HttpWebRequest httpWebRequest = (HttpWebRequest) WebRequest.Create(url);
HttpWebResponse webResponse = (HttpWebResponse) httpWebRequest.GetResponse();
string response;
using (StreamReader sr = new StreamReader(webResponse.GetResponseStream()))
response = sr.ReadToEnd();
WeatherResponse? weather = JsonConvert.DeserializeObject<WeatherResponse>(response);
ReceiverOptions receiverOptions = new()
{
    AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
};

botClient.StartReceiving(
    updateHandler: HandleUpdateAsync,
    pollingErrorHandler: HandlePollingErrorAsync,
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token
);

var me = await botClient.GetMeAsync();
Console.WriteLine($"Start listening for @{me.Username}");
Console.ReadLine();
cts.Cancel();
async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    if (update.Message is not { } message)
    {
        return;
    }

    if (message.Text is not { } messageText)
    {
        return;
    }

    var chatId = message.Chat.Id;
    ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
    {
        new KeyboardButton[] { "погода", "курс доллара" },
    })
    {
        ResizeKeyboard = true
    };

    if (message.Text == "/start")
    {
        Message sentMessage = await botClient.SendTextMessageAsync(
        chatId: chatId,
        text: "Привет",
        cancellationToken: cancellationToken);
    }

    if (message.Text == "погода")
    {
        if (weather is not null)
        {
            Message sentMessage = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: $"{weather.Name}{weather.Main?.Temp}",
            replyMarkup: replyKeyboardMarkup,
            cancellationToken: cancellationToken);
        }
    }
    else if (message.Text == "курс доллара")
    {
        Message sentMessage = await botClient.SendTextMessageAsync(
        chatId: chatId,
        text: "курс доллара 60",
        replyMarkup: replyKeyboardMarkup,
        cancellationToken: cancellationToken);
    } 
}

Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var ErrorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };

    Console.WriteLine(ErrorMessage);
    return Task.CompletedTask;
}
