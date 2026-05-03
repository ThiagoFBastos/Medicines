using Medicines.Commands;
using Medicines.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Medicines.Services
{
    public class CommandExtraction: ICommandExtraction
    {
        private Command ExtractStartCommand(string text)
        {
            var match = Regex.Match(text, @"^/start\s+([A-Za-z0-9\._]+)$");

            if(!match.Success)
                return new UnknownCommand();

            string username = match.Groups[1].Value;

            return new StartCommand(username);
        }

        private Command ExtractAddCommand(string text)
        {
            var match = Regex.Match(text, @"^/add\s+([\p{L}][\p{L}\d]*(?:\s+[\p{L}][\p{L}\d]*)*)\s+(\d+)\s+(\d{2}):(\d{2})$");

            if (!match.Success)
                return new UnknownCommand();

            string medicine = match.Groups[1].Value;
            int quantity = int.Parse(match.Groups[2].Value);
            int hours = int.Parse(match.Groups[3].Value);
            int minutes = int.Parse(match.Groups[4].Value);

            return new AddCommand(medicine, quantity, hours, minutes);
        }

        private Command ExtractRemoveCommand(string text)
        {
            var match = Regex.Match(text, @"^/remove\s+([\p{L}][\p{L}\d]*(?:\s+[\p{L}][\p{L}\d]*)*)$");

            if(!match.Success)
                return new UnknownCommand();

            string medicine = match.Groups[1].Value;

            return new RemoveCommand(medicine);
        }

        private Command ExtractLookupCommand(string text)
        {
            var match = Regex.Match(text, @"^/lookup\s+([\p{L}][\p{L}\d]*(?:\s+[\p{L}][\p{L}\d]*)*)$");

            if(!match.Success)
                return new UnknownCommand();

            string medicine = match.Groups[1].Value;

            return new LookupCommand(medicine);
        }

        private Command ExtractPillsCommand(string text)
        {
            var match = Regex.Match(text, @"^/pills\s+([\p{L}][\p{L}\d]*(?:\s+[\p{L}][\p{L}\d]*)*)\s+(\d+)$");

            if(!match.Success)
                return new UnknownCommand();

            string medicine = match.Groups[1].Value;
            int quantity = int.Parse(match.Groups[2].Value);

            return new PillsCommand(medicine, quantity);
        }

        private Command ExtractScheduleCommand(string text)
        {
            var match = Regex.Match(text, @"^/schedule\s+([\p{L}][\p{L}\d]*(?:\s+[\p{L}][\p{L}\d]*)*)\s+(\d{2}):(\d{2})$");

            if(!match.Success)
                return new UnknownCommand();

            string medicine = match.Groups[1].Value;
            int hours = int.Parse(match.Groups[2].Value);
            int minutes = int.Parse(match.Groups[3].Value);

            return new ScheduleCommand(medicine, hours, minutes);
        }

        private Command ExtractUpdateCommand(string text)
        {
            var match = Regex.Match(text, @"^/update\s+([\p{L}][\p{L}\d]*(?:\s+[\p{L}][\p{L}\d]*)*)\s+(\d+)\s+(\d{2}):(\d{2})$");

            if(!match.Success)
                return new UnknownCommand();

            string medicine = match.Groups[1].Value;
            int quantity = int.Parse(match.Groups[2].Value);
            int hours = int.Parse(match.Groups[3].Value);
            int minutes = int.Parse(match.Groups[4].Value);

            return new UpdateCommand(medicine, quantity, hours, minutes);
        }

        public Command Extract(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new UnknownCommand();

            text = text.Trim();

            if (text.StartsWith("/start"))
            {
                return ExtractStartCommand(text);
            }
            else if (text.StartsWith("/add"))
            {
               return ExtractAddCommand(text);
            }
            else if (text.StartsWith("/remove"))
            {
                return ExtractRemoveCommand(text);
            }
            else if (text.StartsWith("/lookup"))
            {
               return ExtractLookupCommand(text);
            }
            else if (text == "/list")
            {
                return new ListCommand();
            }
            else if (text.StartsWith("/pills"))
            {
                return ExtractPillsCommand(text);
            }
            else if (text.StartsWith("/schedule"))
            {
                return ExtractScheduleCommand(text);
            }
            else if (text.StartsWith("/update"))
            {
               return ExtractUpdateCommand(text);
            }
            else if (text == "/help")
            {
                return new HelpCommand();   
            }

            return new UnknownCommand();
        }
    }
}
