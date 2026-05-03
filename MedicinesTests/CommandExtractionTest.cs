using Medicines.Commands;
using Medicines.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicinesTests
{
    public class CommandExtractionTest
    {
        [Theory]
        [InlineData("/start usertest$")]
        [InlineData("/start")]
        [InlineData("/start user test")]
        [InlineData("/startusertest")]
        [InlineData("")]
        public void StartCommandInvalidTest(string command)
        {
            var commandExtraction = new CommandExtraction();

            var result = commandExtraction.Extract(command);

            Assert.IsType<UnknownCommand>(result);
        }

        [Theory]
        [InlineData("usertest")]
        [InlineData("user_test")]
        [InlineData("usertest100")]
        [InlineData("user.test100")]
        public void StartCommandSuccessTest(string username)
        {
            var commandExtraction = new CommandExtraction();
            string command = $"    /start     {username}    ";

            var result = commandExtraction.Extract(command);

            Assert.IsType<StartCommand>(result);

            var start = result as StartCommand;

            Assert.NotNull(start);
            Assert.Equal(start.Username, username);
        }

        [Theory]
        [InlineData("/add medicine abc 00:00")]
        [InlineData("/add medicine 10a 00:00")]
        [InlineData("/add 1medicine 10 00:00")]
        [InlineData("/add medicine -10 00:00")]
        [InlineData("/add medicine 10.5 00:00")]
        [InlineData("/add more than one word 10 12")]
        [InlineData("/addmedicine1012:00")]
        [InlineData("/add medicine 10 2:00")]
        [InlineData("/add medicine")]
        [InlineData("/add 10")]
        [InlineData("/add 16:00")]
        [InlineData("/add")]
        [InlineData("")]
        public void AddCommandInvalidTest(string command)
        {
            var commandExtraction = new CommandExtraction();

            var result = commandExtraction.Extract(command);

            Assert.IsType<UnknownCommand>(result);
        }

        [Theory]
        [InlineData("medicine", 10, 15, 0)]
        [InlineData("more than one word", 0, 16, 30)]
        [InlineData("more3 than1 one2 word3", 0, 16, 30)]
        [InlineData("moré3 thân1 õne2 wòrd3", 0, 16, 30)]
        public void AddCommandSuccessTest(string medicine, int pillsQuantitity, int hours, int minutes)
        {
            var commandExtraction = new CommandExtraction();
            string command = $"     /add    {medicine}   {pillsQuantitity}  {hours:D2}:{minutes:D2}     ";

            var result = commandExtraction.Extract(command);

            Assert.IsType<AddCommand>(result);

            var add = result as AddCommand;

            Assert.NotNull(add);
            Assert.Equal(add.Medicine, medicine);
            Assert.Equal(add.PillsQuantity, pillsQuantitity);
            Assert.Equal(add.Hours, hours);
            Assert.Equal(add.Minutes, minutes);
        }

        [Theory]
        [InlineData("/remove 1medicine")]
        [InlineData("/remove medic$ine")]
        [InlineData("/remove ")]
        [InlineData("/remove")]
        [InlineData("")]
        public void RemoveCommandInvalidTest(string command)
        {
            var commandExtraction = new CommandExtraction();

            var result = commandExtraction.Extract(command);

            Assert.IsType<UnknownCommand>(result);
        }

        [Theory]
        [InlineData("medicine")]
        [InlineData("more than one word")]
        [InlineData("more3 than1 one2 word3")]
        [InlineData("moré3 thân1 õne2 wòrd3")]
        public void RemoveCommandSuccessTest(string medicine)
        {
            var commandExtraction = new CommandExtraction();
            string command = $"     /remove   {medicine}     ";
            var result = commandExtraction.Extract(command);

            Assert.IsType<RemoveCommand>(result);

            var remove = result as RemoveCommand;

            Assert.NotNull(remove);
            Assert.Equal(remove.Medicine, medicine);
        }

        [Theory]
        [InlineData("/lookup 1medicine")]
        [InlineData("/lookup medic$ine")]
        [InlineData("/lookup ")]
        [InlineData("/lookup")]
        [InlineData("")]
        public void LookupCommandInvalidTest(string command)
        {
            var commandExtraction = new CommandExtraction();

            var result = commandExtraction.Extract(command);

            Assert.IsType<UnknownCommand>(result);
        }

        [Theory]
        [InlineData("medicine")]
        [InlineData("more than one word")]
        [InlineData("more3 than1 one2 word3")]
        [InlineData("moré3 thân1 õne2 wòrd3")]
        public void LookupCommandSuccessTest(string medicine)
        {
            var commandExtraction = new CommandExtraction();
            string command = $"      /lookup   {medicine}      ";
            var result = commandExtraction.Extract(command);

            Assert.IsType<LookupCommand>(result);

            var lookup = result as LookupCommand;

            Assert.NotNull(lookup);
            Assert.Equal(lookup.Medicine, medicine);
        }

        [Theory]
        [InlineData("/pills 1medicine 1")]
        [InlineData("/pills medic$ine 1")]
        [InlineData("/pills medicine -1")]
        [InlineData("/pills medicine 1.5")]
        [InlineData("/pills medicine x")]
        [InlineData("/pills medicine")]
        [InlineData("/pills")]
        [InlineData("")]
        public void PillsCommandInvalidTest(string command)
        {
            var commandExtraction = new CommandExtraction();

            var result = commandExtraction.Extract(command);

            Assert.IsType<UnknownCommand>(result);
        }

        [Theory]
        [InlineData("medicine", 1)]
        [InlineData("more than one word", 0)]
        [InlineData("more3 than1 one2 word3", 10)]
        [InlineData("moré3 thân1 õne2 wòrd3", 5)]
        public void PillsCommandSuccessTest(string medicine, int pillsQuantity)
        {
            var commandExtraction = new CommandExtraction();
            string command = $"      /pills   {medicine}   {pillsQuantity}      ";

            var result = commandExtraction.Extract(command);

            Assert.IsType<PillsCommand>(result);

            var pills = result as PillsCommand;

            Assert.NotNull(pills);
            Assert.Equal(pills.Medicine, medicine);
            Assert.Equal(pills.PillsQuantity, pillsQuantity);
        }

        [Theory]
        [InlineData("/schedule 1medicine 10:00")]
        [InlineData("/schedule medic$ine 10:00")]
        [InlineData("/schedule medicine 10:0")]
        [InlineData("/schedule medicine 1:00")]
        [InlineData("/schedule medicine 10:00a")]
        [InlineData("/schedule medicine 10:00:00")]
        [InlineData("/schedule medicine aa:bb")]
        [InlineData("/schedule medicine")]
        [InlineData("/schedule 16:00")]
        [InlineData("/schedule")]
        [InlineData("")]
        public void ScheduleCommandInvalidTest(string command)
        {
            var commandExtraction = new CommandExtraction();

            var result = commandExtraction.Extract(command);

            Assert.IsType<UnknownCommand>(result);
        }

        [Theory]
        [InlineData("medicine", 10, 0)]
        [InlineData("more than one word", 0, 16)]
        [InlineData("more3 than1 one2 word3", 16, 30)]
        [InlineData("moré3 thân1 õne2 wòrd3", 23, 59)]
        public void ScheduleCommandSuccessTest(string medicine, int hours, int minutes)
        {
            var commandExtraction = new CommandExtraction();
            string command = $"      /schedule   {medicine}   {hours:D2}:{minutes:D2}      ";

            var result = commandExtraction.Extract(command);

            Assert.IsType<ScheduleCommand>(result);

            var schedule = result as ScheduleCommand;

            Assert.NotNull(schedule);
            Assert.Equal(schedule.Medicine, medicine);
            Assert.Equal(schedule.Hours, hours);
            Assert.Equal(schedule.Minutes, minutes);
        }


        [Theory]
        [InlineData("/update 1medicine 30 10:00")]
        [InlineData("/update medic$ine 30 10:00")]
        [InlineData("/update medicine -30 10:00")]
        [InlineData("/update medicine 30.5 10:00")]
        [InlineData("/update medicine 30 10:0")]
        [InlineData("/update medicine 30 1:00")]
        [InlineData("/update medicine 30 10:00a")]
        [InlineData("/update medicine 30 10:00:00")]
        [InlineData("/update medicine 30 aa:bb")]
        [InlineData("/update medicine 30")]
        [InlineData("/update medicine 10:00")]
        [InlineData("/update medicine")]
        [InlineData("/update")]
        [InlineData("")]
        public void UpdateCommandInvalidTest(string command)
        {
            var commandExtraction = new CommandExtraction();

            var result = commandExtraction.Extract(command);

            Assert.IsType<UnknownCommand>(result);
        }

        [Theory]
        [InlineData("medicine", 30, 10, 0)]
        [InlineData("more than one word", 0, 16, 30)]
        [InlineData("more3 than1 one2 word3", 10, 16, 30)]
        [InlineData("moré3 thân1 õne2 wòrd3", 5, 23, 59)]

        public void UpdateCommandSuccessTest(string medicine, int pillsQuantity, int hours, int minutes)
        {
            var commandExtraction = new CommandExtraction();

            string command = $"      /update   {medicine}   {pillsQuantity}   {hours:D2}:{minutes:D2}      ";

            var result = commandExtraction.Extract(command);

            Assert.IsType<UpdateCommand>(result);

            var update = result as UpdateCommand;

            Assert.NotNull(update);
            Assert.Equal(update.Medicine, medicine);
            Assert.Equal(update.PillsQuantity, pillsQuantity);
            Assert.Equal(update.Hours, hours);
            Assert.Equal(update.Minutes, minutes);
        }

        [Theory]
        [InlineData("/list extra")]
        [InlineData("")]
        public void ListCommandInvalidTest(string command)
        {
            var commandExtraction = new CommandExtraction();

            var result = commandExtraction.Extract(command);

            Assert.IsType<UnknownCommand>(result);
        }

        [Fact]
        public void ListCommandSuccessTest()
        {
            var commandExtraction = new CommandExtraction();
            string command = $"      /list      ";

            var result = commandExtraction.Extract(command);

            Assert.IsType<ListCommand>(result);
        }

        [Theory]
        [InlineData("/help extra")]
        [InlineData("")]
        public void HelpCommandInvalidTest(string command)
        {
            var commandExtraction = new CommandExtraction();

            var result = commandExtraction.Extract(command);

            Assert.IsType<UnknownCommand>(result);
        }

        [Fact]
        public void HelpCommandSuccessTest()
        {
            var commandExtraction = new CommandExtraction();
            string command = $"      /help      ";

            var result = commandExtraction.Extract(command);

            Assert.IsType<HelpCommand>(result);
        }
    }
}
