using Medicines.Interfaces;
using Medicines.Models;
using Medicines.Utils;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Extensions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static System.Net.Mime.MediaTypeNames;

namespace Medicines.Services
{
    public class TelegramBotService: ITelegramBotService
    {
        private readonly CancellationTokenSource _cts;
        private readonly TelegramBotClient _bot;
        private readonly ILogger<TelegramBotService> _logger;
        private readonly TelegramBotOptions _options;
        private readonly IMedicinesService _medicinesService;

        public TelegramBotService(ILogger<TelegramBotService> logger, IOptions<TelegramBotOptions> options, IMedicinesService medicinesService)
        {
            _logger = logger;
            _cts = new CancellationTokenSource();
            _options = options.Value;
            _bot = new TelegramBotClient(_options.Token, cancellationToken: _cts.Token);

            _bot.OnError += OnError;
            _bot.OnMessage += OnMessage;
            _bot.OnUpdate += OnUpdate;
            _medicinesService = medicinesService;
        }

        public async Task Help(Chat chat)
        {
            await _bot.SendHtml(chat, """
            Os seguintes comandos estão disponíveis:

            • <b>/add [remédio] [quantidade]</b> - Adiciona um remédio com a quantidade especificada.
            • <b>/remove [remédio]</b> - Remove um remédio da lista.
            • <b>/lookup [remédio]</b> - Procura por um remédio específico.
            • <b>/list</b> - Exibe a lista de remédios.
            • <b>/help</b> - Exibe esta mensagem de ajuda.
            """);
        }

        public async Task StartAsync(Message msg, UpdateType type)
        {
            if (msg is null)
                return;

            var username = msg.From?.Username ?? "Usuário";

            await _bot.SendMessage(msg.Chat, $"Seja bem vindo ao Medicines, {username}!");
        }

        public async Task AddAsync(Message msg, UpdateType type)
        {
            if (msg is null)
                return;

            var text = msg.Text!.Trim();

            var match = Regex.Match(text, @"^/add\s+(\w+)\s+(\d+)\s+(\d{2}):(\d{2})$");

            var username = msg.From?.Username ?? "Usuário";

            if (match.Success)
            {
                string medicine = match.Groups[1].Value;
                int quantity = int.Parse(match.Groups[2].Value);
                int hour = int.Parse(match.Groups[3].Value);
                int minutes = int.Parse(match.Groups[4].Value);
                var scheduledTime = new DateTimeOffset(DateTimeOffset.UtcNow.Date, TimeSpan.FromHours(-3)) + new TimeSpan(hour, minutes, 0);
                

                await _medicinesService.AddMedicineAsync(medicine, quantity, scheduledTime.ToLocalTime(), msg.From!.Id);

                await _bot.SendMessage(msg.Chat, $"{username}, você adicionou o remédio {medicine} com quantidade {quantity} no horário {scheduledTime}");
            }
            else
            {
                await Help(msg.Chat);
            }
        }

        public async Task RemoveAsync(Message msg, UpdateType type)
        {
            if (msg is null)
                return;

            var text = msg.Text!.Trim();

            var match = Regex.Match(text, @"^/remove\s+(\w+)$");

            var username = msg.From?.Username ?? "Usuário";

            if (match.Success)
            {
                string medicine = match.Groups[1].Value;

                await _medicinesService.DeleteMedicineAsync(medicine, msg.From.Id);

                await _bot.SendMessage(msg.Chat, $"{username}, você removeu o remédio {medicine}");
            }
            else
            {
                await Help(msg.Chat);
            }
        }

        public async Task LookupAsync(Message msg, UpdateType type)
        {
            if (msg is null)
                return;

            var text = msg.Text!.Trim();

            var match = Regex.Match(text, @"^/lookup\s+(\w+)$");

            var username = msg.From?.Username ?? "Usuário";

            if (match.Success)
            {
                string medicine = match.Groups[1].Value;

                var medicineInfo = await _medicinesService.GetMedicineByNameAsync(medicine, msg.From!.Id);

                await _bot.SendHtml(msg.Chat, $"""
                   {username}, você procurou pelo remédio {medicine}:
                   • <b>Nome:</b> {medicineInfo?.Name}
                   • <b>Quantidade:</b> {medicineInfo?.PillsQuantity}
                   • <b>Horário:</b> {medicineInfo?.ScheduledTime:HH:mm}
                """);
            }
            else
            {
                await Help(msg.Chat);
            }
        }

        public async Task ListAsync(Message msg, UpdateType type)
        {
            if (msg is null)
                return;

            var username = msg.From?.Username ?? "Usuário";

            var medicines = await _medicinesService.GetAllMedicinesAsync(msg.From!.Id);

            var details = medicines.Select(med =>
                $"""
                  • <b>Nome</b>: {med.Name}
                  • <b>Quantidade</b>: {med.PillsQuantity}
                  • <b>Horário</b>: {med.ScheduledTime:HH:mm}
                """
            ).ToList();

            var result = string.Join("\n", details);

            await _bot.SendHtml(msg.Chat, 
            $"""
                {username}, foram encontrados {details.Count} remédios disponíveis:
                {result}
            """);
        }

        public async Task OnError(Exception exception, HandleErrorSource source)
        {
            _logger.LogError(exception.Message, exception);
            await Task.CompletedTask;
        }
        public async Task OnMessage(Message msg, UpdateType type)
        {
            if (msg.Text is null)
                return;
            
            string text = msg.Text.Trim();
            
            if (text == "/start")
            {
                await StartAsync(msg, type);
            }
            else if (text.StartsWith("/add"))
            {
                await AddAsync(msg, type);
            }
            else if (text.StartsWith("/remove"))
            {
                await RemoveAsync(msg, type);
            }
            else if (text.StartsWith("/lookup"))
            {
               await LookupAsync(msg, type);
            }
            else if (text.StartsWith("/list"))
            {
                await ListAsync(msg, type);
            }
            else if(text.StartsWith("/help"))
            {
                await Help(msg.Chat);
            }
            else
            {
                await Help(msg.Chat);
            }
        }

        public async Task OnUpdate(Update update)
        {
            await Task.CompletedTask;
        }
    }
}
