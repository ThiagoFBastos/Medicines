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
        private readonly IUserService _userService;

        public TelegramBotService(ILogger<TelegramBotService> logger, IOptions<TelegramBotOptions> options, IMedicinesService medicinesService, IUserService userService)
        {
            _logger = logger;
            _cts = new CancellationTokenSource();
            _options = options.Value;
            _bot = new TelegramBotClient(_options.Token, cancellationToken: _cts.Token);

            _bot.OnError += OnError;
            _bot.OnMessage += OnMessage;
            _bot.OnUpdate += OnUpdate;
            _medicinesService = medicinesService;
            _userService = userService;
        }

        private async Task Help(Chat chat)
        {
            await _bot.SendHtml(chat, """
            Os seguintes comandos estão disponíveis:

            • <b>/start [username]</b> - Inicia a interação com o bot e registra o usuário com o nome de usuário especificado.
            • <b>/add [remédio] [quantidade] [horário] </b> - Adiciona um remédio com a quantidade especificada e um horário para tomar.
            • <b>/remove [remédio]</b> - Remove um remédio da lista.
            • <b>/lookup [remédio]</b> - Procura por um remédio específico.
            • <b>/list</b> - Exibe a lista de remédios.
            • <b>/pills [remédio] [quantidade]</b> - Adiciona uma quantidade de comprimidos a um remédio existente.
            • <b>/schedule [remédio] [horário]</b> - Atualiza o horário de um remédio existente.
            • <b>/help</b> - Exibe esta mensagem de ajuda.
            """);
        }

        private async Task StartAsync(Message msg, UpdateType type)
        {
            if (msg is null)
                return;

            var text = msg.Text!.Trim();

            var match = Regex.Match(text, @"^/start\s+([A-Za-z0-9\._]+)$");

            if (match.Success)
            {
                var username = match.Groups[1].Value;

                var result = await _userService.AddUserAsync(msg.From!.Id, username);

                if (result.IsSuccess)
                {
                    await _bot.SendMessage(msg.Chat, $"Seja bem vindo ao Medicines, {username}!");
                }
                else
                {
                    var resultUser = await _userService.GetUserByUserIdAsync(msg.From!.Id);

                    if (resultUser.IsSuccess)
                    {
                        var user = resultUser.Value;
                        await _bot.SendMessage(msg.Chat, $"{user!.Username}, {result.Error}!");
                    }
                    else
                    {
                        await _bot.SendMessage(msg.Chat, $"Ocorreu um erro ao recuperar as informações do usuário com id {msg.From.Id}\nMotivo: {resultUser.Error}");
                    }
                }
            }
            else
            {
                await Help(msg.Chat);
            }
        }

        private async Task AddAsync(Message msg, UpdateType type)
        {
            if (msg is null)
                return;

            var text = msg.Text!.Trim();

            var match = Regex.Match(text, @"^/add\s+(\w+)\s+(\d+)\s+(\d{2}):(\d{2})$");

            var result = await _userService.GetUserByUserIdAsync(msg.From!.Id);

            if (!result.IsSuccess)
            {
                await _bot.SendMessage(msg.Chat, $"Ocorreu um erro ao recuperar as informações do usuário com id {msg.From.Id}\nMotivo: {result.Error}");
                return;
            }

            var user = result.Value;

            var username = user?.Username ?? "Usuário";

            if (match.Success)
            {
                string medicine = match.Groups[1].Value;
                int quantity = int.Parse(match.Groups[2].Value);
                int hour = int.Parse(match.Groups[3].Value);
                int minutes = int.Parse(match.Groups[4].Value);
                var scheduledTime = new DateTimeOffset(DateTimeOffset.UtcNow.Date, TimeSpan.FromHours(-3)) + new TimeSpan(hour, minutes, 0);

                var addResult = await _medicinesService.AddMedicineAsync(medicine, quantity, scheduledTime.ToLocalTime(), msg.From!.Id);

                if(addResult.IsSuccess)
                {
                    await _bot.SendMessage(msg.Chat, $"{username}, você adicionou o remédio {medicine} com quantidade {quantity} no horário {scheduledTime:HH:mm}");
                }
                else
                {
                    await _bot.SendMessage(msg.Chat, $"{username}, {addResult.Error}");
                }
            }
            else
            {
                await Help(msg.Chat);
            }
        }

        private async Task RemoveAsync(Message msg, UpdateType type)
        {
            if (msg is null)
                return;

            var text = msg.Text!.Trim();

            var match = Regex.Match(text, @"^/remove\s+(\w+)$");

            var result = await _userService.GetUserByUserIdAsync(msg.From!.Id);

            if (!result.IsSuccess)
            {
                await _bot.SendMessage(msg.Chat, $"Ocorreu um erro ao recuperar as informações do usuário com id {msg.From.Id}\nMotivo: {result.Error}");
                return;
            }

            var user = result.Value;

            var username = user?.Username ?? "Usuário";

            if (match.Success)
            {
                string medicine = match.Groups[1].Value;

                var deleteResult = await _medicinesService.DeleteMedicineAsync(medicine, msg.From.Id);

                if (deleteResult.IsSuccess)
                {
                    await _bot.SendMessage(msg.Chat, $"{username}, você removeu o remédio {medicine}");
                }
                else
                {
                    await _bot.SendMessage(msg.Chat, $"{username}, {deleteResult.Error}");
                }
            }
            else
            {
                await Help(msg.Chat);
            }
        }

        private async Task LookupAsync(Message msg, UpdateType type)
        {
            if (msg is null)
                return;

            var text = msg.Text!.Trim();

            var match = Regex.Match(text, @"^/lookup\s+(\w+)$");

            var result = await _userService.GetUserByUserIdAsync(msg.From!.Id);

            if (!result.IsSuccess)
            {
                await _bot.SendMessage(msg.Chat, $"Ocorreu um erro ao recuperar as informações do usuário com id {msg.From.Id}\nMotivo: {result.Error}");
                return;
            }

            var user = result.Value;

            var username = user?.Username ?? "Usuário";

            if (match.Success)
            {
                string medicine = match.Groups[1].Value;

                var getResult = await _medicinesService.GetMedicineByNameAsync(medicine, msg.From!.Id);

                if (getResult.IsSuccess)
                {
                    var medicineInfo = getResult.Value;

                    await _bot.SendHtml(msg.Chat, $"""
                       {username}, você procurou pelo remédio {medicine}:
                       • <b>Nome:</b> {medicineInfo?.Name}
                       • <b>Quantidade:</b> {medicineInfo?.PillsQuantity}
                       • <b>Horário:</b> {medicineInfo?.ScheduledTime:HH:mm}
                    """);
                }
                else
                {
                    await _bot.SendMessage(msg.Chat, $"{username}, {getResult.Error}");
                }
            }
            else
            {
                await Help(msg.Chat);
            }
        }

        private async Task ListAsync(Message msg, UpdateType type)
        {
            if (msg is null)
                return;

            var result = await _userService.GetUserByUserIdAsync(msg.From!.Id);

            if (!result.IsSuccess)
            {
                await _bot.SendMessage(msg.Chat, $"Ocorreu um erro ao recuperar as informações do usuário com id {msg.From.Id}\nMotivo: {result.Error}");
                return;
            }

            var user = result.Value;

            var username = user?.Username ?? "Usuário";

            var getResult = await _medicinesService.GetAllMedicinesAsync(msg.From!.Id);

            if (getResult.IsSuccess)
            {
                var medicines = getResult.Value!;

                var details = medicines.Select(med =>
                    $"""
                  • <b>Nome</b>: {med.Name}
                  • <b>Quantidade</b>: {med.PillsQuantity}
                  • <b>Horário</b>: {med.ScheduledTime:HH:mm}
                """
                ).ToList();

                var detailsResult = string.Join("\n\n", details);

                await _bot.SendHtml(msg.Chat,
                $"""
                {username}, foram encontrados {details.Count} remédios disponíveis:
                {detailsResult}
                """);
            }
            else
            {
                await _bot.SendMessage(msg.Chat, $"{username}, {getResult.Error}");
            }
        }

        private async Task PillsAsync(Message msg, UpdateType update)
        {
            if (msg is null)
                return;

            var result = await _userService.GetUserByUserIdAsync(msg.From!.Id);

            if (!result.IsSuccess)
            {
                await _bot.SendMessage(msg.Chat, $"Ocorreu um erro ao recuperar as informações do usuário com id {msg.From.Id}\nMotivo: {result.Error}");
                return;
            }

            var user = result.Value;

            var username = user?.Username ?? "Usuário";

            var text = msg.Text!.Trim();

            var match = Regex.Match(text, @"^/pills\s+(\w+)\s+(\d+)$");

            if (match.Success)
            {
                string medicine = match.Groups[1].Value;
                int pills = int.Parse(match.Groups[2].Value);

                var addPillsResult = await _medicinesService.AddMedicinePillsAsync(medicine, pills, msg.From!.Id);

                if (addPillsResult.IsSuccess)
                {
                    await _bot.SendMessage(msg.Chat, $"{username}, {pills} comprimidos do remédio {medicine} foram adicionados");
                }
                else
                {
                    await _bot.SendMessage(msg.Chat, $"{username}, {addPillsResult.Error}");
                }
            }
            else
            {
                await Help(msg.Chat);
            }
        }

        public async Task ScheduleAsync(Message msg, UpdateType update)
        {
            if (msg is null)
                return;

            var result = await _userService.GetUserByUserIdAsync(msg.From!.Id);

            if (!result.IsSuccess)
            {
                await _bot.SendMessage(msg.Chat, $"Ocorreu um erro ao recuperar as informações do usuário com id {msg.From.Id}\nMotivo: {result.Error}");
                return;
            }

            var user = result.Value;

            var username = user?.Username ?? "Usuário";

            var text = msg.Text!.Trim();

            var match = Regex.Match(text, @"^/schedule\s+(\w+)\s+(\d{2}):(\d{2})$");

            if (match.Success)
            {
                string medicine = match.Groups[1].Value;
                int hour = int.Parse(match.Groups[2].Value);
                int minutes = int.Parse(match.Groups[3].Value);
                var scheduledTime = new DateTimeOffset(DateTimeOffset.UtcNow.Date, TimeSpan.FromHours(-3)) + new TimeSpan(hour, minutes, 0);

                var updateScheduleResult = await _medicinesService.UpdateMedicineScheduledTime(medicine, scheduledTime.ToLocalTime(), msg.From!.Id);

                if (updateScheduleResult.IsSuccess)
                {
                    await _bot.SendMessage(msg.Chat, $"{username}, o horário do remédio {medicine} foi atualizado para {scheduledTime:HH:mm}");
                }
                else
                {
                    await _bot.SendMessage(msg.Chat, $"{username}, {updateScheduleResult.Error}");
                }
            }
            else
            {
                await Help(msg.Chat);
            }
        }

        public async Task OnError(Exception exception, HandleErrorSource source)
        {
            _logger.LogError(exception, exception.Message);
            await Task.CompletedTask;
        }
        public async Task OnMessage(Message msg, UpdateType type)
        {
            if (msg?.Text is null)
            {
                _logger.LogWarning("Received a message with no text.");
                return;
            }

            try
            {
                string text = msg.Text.Trim();

                if (text.StartsWith("/start"))
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
                else if (text.StartsWith("/pills"))
                {
                    await PillsAsync(msg, type);
                }
                else if(text.StartsWith("/schedule"))
                {
                    await ScheduleAsync(msg, type);
                }
                else if (text.StartsWith("/help"))
                {
                    await Help(msg.Chat);
                }
                else
                {
                    await Help(msg.Chat);
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"An error occurred while processing the message: {exception.Message}");
            }
        }

        public async Task OnUpdate(Update update)
        {
            await Task.CompletedTask;
        }
    }
}
