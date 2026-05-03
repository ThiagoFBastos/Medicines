using Medicines.Commands;
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
        private readonly ICommandExtraction _commandExtraction;

        public TelegramBotService(ILogger<TelegramBotService> logger, IOptions<TelegramBotOptions> options, IMedicinesService medicinesService, IUserService userService, ICommandExtraction commandExtraction)
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
            _commandExtraction = commandExtraction;
        }

        private async Task Help(Chat chat, UpdateType type, HelpCommand helpCommand)
        {
            await _bot.SendHtml(chat, """
            Os seguintes comandos estão disponíveis:

            • <b>/start [username]</b> - Inicia a interação com o bot e registra o usuário com o nome de usuário especificado.
            • <b>/add [remédio] [quantidade] [horário]</b> - Adiciona um remédio com a quantidade especificada e um horário para tomar.
            • <b>/remove [remédio]</b> - Remove um remédio da lista.
            • <b>/lookup [remédio]</b> - Procura por um remédio específico.
            • <b>/list</b> - Exibe a lista de remédios.
            • <b>/pills [remédio] [quantidade]</b> - Adiciona uma quantidade de comprimidos a um remédio existente.
            • <b>/schedule [remédio] [horário]</b> - Atualiza o horário de um remédio existente.
            • <b>/update [remédio] [quantidade] [horário]</b> - Atualiza a quantidade e o horário de um remédio existente.
            • <b>/help</b> - Exibe esta mensagem de ajuda.
            """);
        }

        private async Task StartAsync(Message msg, UpdateType type, StartCommand startCommand)
        {
            if (msg is null)
                return;

            var username = startCommand.Username;

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
                    var errorDetails = UserStatusCodeTranslator.TranslateUserStatusCode(result.Error);

                    await _bot.SendMessage(msg.Chat, $"{user!.Username}, {errorDetails}!");
                }
                else
                {
                    var errorDetails = UserStatusCodeTranslator.TranslateUserStatusCode(result.Error);
                    await _bot.SendMessage(msg.Chat, $"Ocorreu um erro ao recuperar as informações do usuário com id {msg.From.Id}\nMotivo: {errorDetails}");
                }
            }
        }

        private async Task AddAsync(Message msg, UpdateType type, AddCommand addCommand)
        {
            if (msg is null)
                return;

            var result = await _userService.GetUserByUserIdAsync(msg.From!.Id);

            if (!result.IsSuccess)
            {
                var errorDetails = UserStatusCodeTranslator.TranslateUserStatusCode(result.Error);

                await _bot.SendMessage(msg.Chat, $"Ocorreu um erro ao recuperar as informações do usuário com id {msg.From.Id}\nMotivo: {errorDetails}");
                return;
            }

            var user = result.Value;

            var username = user?.Username ?? "Usuário";

            string medicine = addCommand.Medicine;
            int quantity = addCommand.PillsQuantity;
            int hours = addCommand.Hours;
            int minutes = addCommand.Minutes;
            var scheduledTime = new DateTimeOffset(DateTimeOffset.UtcNow.Date, TimeSpan.FromHours(-3)) + new TimeSpan(hours, minutes, 0);

            var addResult = await _medicinesService.AddMedicineAsync(medicine, quantity, scheduledTime.ToUniversalTime(), msg.From!.Id);

            if(addResult.IsSuccess)
            {
                await _bot.SendMessage(msg.Chat, $"{username}, você adicionou o remédio {medicine} com quantidade {quantity} no horário {scheduledTime:HH:mm}");
            }
            else
            {
                var errorDetails = MedicinesStatusCodeTranslator.TranslateStatusCode(addResult.Error, medicine);
                await _bot.SendMessage(msg.Chat, $"{username}, {errorDetails}");
            }   
        }

        private async Task RemoveAsync(Message msg, UpdateType type, RemoveCommand removeCommand)
        {
            if (msg is null)
                return;

            var result = await _userService.GetUserByUserIdAsync(msg.From!.Id);

            if (!result.IsSuccess)
            {
                var errorDetails = UserStatusCodeTranslator.TranslateUserStatusCode(result.Error);

                await _bot.SendMessage(msg.Chat, $"Ocorreu um erro ao recuperar as informações do usuário com id {msg.From.Id}\nMotivo: {errorDetails}");
                return;
            }

            var user = result.Value;

            var username = user?.Username ?? "Usuário";

            string medicine = removeCommand.Medicine;

            var deleteResult = await _medicinesService.DeleteMedicineAsync(medicine, msg.From.Id);

            if (deleteResult.IsSuccess)
            {
                await _bot.SendMessage(msg.Chat, $"{username}, você removeu o remédio {medicine}");
            }
            else
            {
                var errorDetails = MedicinesStatusCodeTranslator.TranslateStatusCode(deleteResult.Error, medicine);
                await _bot.SendMessage(msg.Chat, $"{username}, {errorDetails}");
            }
        }

        private async Task LookupAsync(Message msg, UpdateType type, LookupCommand lookupCommand)
        {
            if (msg is null)
                return;

            var result = await _userService.GetUserByUserIdAsync(msg.From!.Id);

            if (!result.IsSuccess)
            {
                var errorDetails = UserStatusCodeTranslator.TranslateUserStatusCode(result.Error);

                await _bot.SendMessage(msg.Chat, $"Ocorreu um erro ao recuperar as informações do usuário com id {msg.From.Id}\nMotivo: {errorDetails}");
                return;
            }

            var user = result.Value;

            var username = user?.Username ?? "Usuário";
           
            string medicine = lookupCommand.Medicine;

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
                var errorDetails = MedicinesStatusCodeTranslator.TranslateStatusCode(getResult.Error, medicine);
                await _bot.SendMessage(msg.Chat, $"{username}, {errorDetails}");
            }
        }

        private async Task ListAsync(Message msg, UpdateType type, ListCommand listCommand)
        {
            if (msg is null)
                return;

            var result = await _userService.GetUserByUserIdAsync(msg.From!.Id);

            if (!result.IsSuccess)
            {
                var errorDetails = UserStatusCodeTranslator.TranslateUserStatusCode(result.Error);

                await _bot.SendMessage(msg.Chat, $"Ocorreu um erro ao recuperar as informações do usuário com id {msg.From.Id}\nMotivo: {errorDetails}");
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
                var errorDetails = MedicinesStatusCodeTranslator.TranslateStatusCode(getResult.Error, null);
                await _bot.SendMessage(msg.Chat, $"{username}, {errorDetails}");
            }
        }

        private async Task PillsAsync(Message msg, UpdateType update, PillsCommand pillsCommand)
        {
            if (msg is null)
                return;

            var result = await _userService.GetUserByUserIdAsync(msg.From!.Id);

            if (!result.IsSuccess)
            {
                var errorDetails = UserStatusCodeTranslator.TranslateUserStatusCode(result.Error);

                await _bot.SendMessage(msg.Chat, $"Ocorreu um erro ao recuperar as informações do usuário com id {msg.From.Id}\nMotivo: {errorDetails}");
                return;
            }

            var user = result.Value;

            var username = user?.Username ?? "Usuário";

            string medicine = pillsCommand.Medicine;
            int pills = pillsCommand.PillsQuantity;

            var addPillsResult = await _medicinesService.AddMedicinePillsAsync(medicine, pills, msg.From!.Id);

            if (addPillsResult.IsSuccess)
            {
                await _bot.SendMessage(msg.Chat, $"{username}, {pills} comprimidos do remédio {medicine} foram adicionados");
            }
            else
            {
                var errorDetails = MedicinesStatusCodeTranslator.TranslateStatusCode(addPillsResult.Error, medicine);
                await _bot.SendMessage(msg.Chat, $"{username}, {errorDetails}");
            }
        }

        private async Task ScheduleAsync(Message msg, UpdateType update, ScheduleCommand scheduleCommand)
        {
            if (msg is null)
                return;

            var result = await _userService.GetUserByUserIdAsync(msg.From!.Id);

            if (!result.IsSuccess)
            {
                var errorDetails = UserStatusCodeTranslator.TranslateUserStatusCode(result.Error);

                await _bot.SendMessage(msg.Chat, $"Ocorreu um erro ao recuperar as informações do usuário com id {msg.From.Id}\nMotivo: {errorDetails}");
                return;
            }

            var user = result.Value;

            var username = user?.Username ?? "Usuário";

            string medicine = scheduleCommand.Medicine;
            int hours = scheduleCommand.Hours;
            int minutes = scheduleCommand.Minutes;
            var scheduledTime = new DateTimeOffset(DateTimeOffset.UtcNow.Date, TimeSpan.FromHours(-3)) + new TimeSpan(hours, minutes, 0);

            var updateScheduleResult = await _medicinesService.UpdateMedicineScheduledTime(medicine, scheduledTime.ToUniversalTime(), msg.From!.Id);

            if (updateScheduleResult.IsSuccess)
            {
                await _bot.SendMessage(msg.Chat, $"{username}, o horário do remédio {medicine} foi atualizado para {scheduledTime:HH:mm}");
            }
            else
            {
                var errorDetails = MedicinesStatusCodeTranslator.TranslateStatusCode(updateScheduleResult.Error, medicine);
                await _bot.SendMessage(msg.Chat, $"{username}, {errorDetails}");
            }
        }

        private async Task UpdateAsync(Message msg, UpdateType update, UpdateCommand updateCommand)
        {
            if (msg is null)
                return;

            var result = await _userService.GetUserByUserIdAsync(msg.From!.Id);

            if (!result.IsSuccess)
            {
                var errorDetails = UserStatusCodeTranslator.TranslateUserStatusCode(result.Error);

                await _bot.SendMessage(msg.Chat, $"Ocorreu um erro ao recuperar as informações do usuário com id {msg.From.Id}\nMotivo: {errorDetails}");
                return;
            }

            var user = result.Value;

            var username = user?.Username ?? "Usuário";

            string medicine = updateCommand.Medicine;
            int pillsQuantity = updateCommand.PillsQuantity;
            int hours = updateCommand.Hours;
            int minutes = updateCommand.Minutes;

            var scheduledTime = new DateTimeOffset(DateTimeOffset.UtcNow.Date, TimeSpan.FromHours(-3)) + new TimeSpan(hours, minutes, 0);

            var updateResult = await _medicinesService.UpdateMedicineAsync(medicine, pillsQuantity, scheduledTime.ToUniversalTime(), msg.From!.Id);

            if (updateResult.IsSuccess)
            {
                await _bot.SendMessage(msg.Chat, $"{username}, o remédio {medicine} foi atualizado com quantidade {pillsQuantity} e horário {scheduledTime:HH:mm}");
            }
            else
            {
                var errorDetails = MedicinesStatusCodeTranslator.TranslateStatusCode(updateResult.Error, medicine);
                await _bot.SendMessage(msg.Chat, $"{username}, {errorDetails}");
            }
        }

        private async Task OnError(Exception exception, HandleErrorSource source)
        {
            _logger.LogError(exception, exception.Message);
            await Task.CompletedTask;
        }

        private async Task OnMessage(Message msg, UpdateType type)
        {
            if (msg?.Text is null)
            {
                _logger.LogWarning("Received a message with no text.");
                return;
            }

            try
            {
                await (_commandExtraction.Extract(msg.Text) switch
                {   
                    StartCommand start => StartAsync(msg, type, start),
                    AddCommand add => AddAsync(msg, type, add),
                    RemoveCommand remove => RemoveAsync(msg, type, remove),
                    LookupCommand lookup => LookupAsync(msg, type, lookup),
                    ListCommand list => ListAsync(msg, type, list),
                    PillsCommand pills => PillsAsync(msg, type, pills),
                    ScheduleCommand schedule => ScheduleAsync(msg, type, schedule),
                    UpdateCommand update => UpdateAsync(msg, type, update),
                    HelpCommand help => Help(msg.Chat, type, help),
                    _ => Help(msg.Chat, type, new HelpCommand())
                });
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"An error occurred while processing the message: {exception.Message}");
            }
        }

        private async Task OnUpdate(Update update)
        {
            await Task.CompletedTask;
        }
    }
}
