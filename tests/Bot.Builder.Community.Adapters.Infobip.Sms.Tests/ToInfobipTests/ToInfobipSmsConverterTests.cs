﻿using System;
using System.Collections.Generic;
using System.Linq;
using Bot.Builder.Community.Adapters.Infobip.Core;
using Bot.Builder.Community.Adapters.Infobip.Core.Models;
using Bot.Builder.Community.Adapters.Infobip.Sms.ToInfobip;
using Microsoft.Bot.Schema;
using Microsoft.Rest;
using Newtonsoft.Json;
using Xunit;

namespace Bot.Builder.Community.Adapters.Infobip.Sms.Tests.ToInfobipTests
{
    public class ToInfobipSmsConverterTest
    {
        private Activity _activity;

        public ToInfobipSmsConverterTest()
        {
            _activity = new Activity
            {
                Type = ActivityTypes.Message,
                Id = "message id",
                Timestamp = DateTimeOffset.Parse("2020-02-26T10:15:48.734+0000"),
                Conversation = new ConversationAccount { Id = "subscriber-number" },
                From = new ChannelAccount { Id = "whatsapp-number" },
                Recipient = new ChannelAccount { Id = "subscriber-number" }
            };
        }

        [Fact]
        public void ConvertTextActivityToOmniFailoverMessage()
        {
            Activity activity = null;

            Assert.Throws<ArgumentNullException>(() => ToInfobipSmsConverter.Convert(activity, TestOptions.Get()));
        }

        [Fact]
        public void ConvertTextActivityWithCallbackData_Success()
        {
            var callbackData = new Dictionary<string, string>
            {
                {"BoolProperty", "true"},
                {"NumberProperty", "12"},
                {"StringProperty", "string"},
                {"DateProperty", DateTimeOffset.MinValue.ToString()}
            };

            _activity.Text = "Activity with callback data";
            _activity.AddInfobipCallbackData(callbackData);

            var message = ToInfobipSmsConverter.Convert(_activity, TestOptions.Get()).Single();
            Assert.Equal(message.CallbackData, JsonConvert.SerializeObject(callbackData));
        }

        [Fact]
        public void ConvertTextActivityWithCallbackDataAddedWithExtensionMethod_Success()
        {
            var callbackData = new Dictionary<string, string>
            {
                {"BoolProperty", "true"},
                {"NumberProperty", "12"},
                {"StringProperty", "string"},
                {"DateProperty", DateTimeOffset.MinValue.ToString()}
            };

            _activity.Text = "Activity with callback data";
            _activity.AddInfobipCallbackData(callbackData);

            var message = ToInfobipSmsConverter.Convert(_activity, TestOptions.Get()).Single();
            Assert.Equal(message.CallbackData, JsonConvert.SerializeObject(callbackData));
        }

        [Fact]
        public void ConvertEmptyActivityWithCallbackData_Success()
        {
            var callbackData = new Dictionary<string, string>
            {
                {"BoolProperty", "true"},
                {"NumberProperty", "12"},
                {"StringProperty", "string"},
                {"DateProperty", DateTimeOffset.MinValue.ToString()}
            };

            _activity.AddInfobipCallbackData(callbackData);

            var messages = ToInfobipSmsConverter.Convert(_activity, TestOptions.Get());
            Assert.Empty(messages);
        }

        [Fact]
        public void ConvertActivityWithRecipient_ThrowException()
        {
            _activity.Recipient = null;

            Assert.Throws<ValidationException>(() => ToInfobipSmsConverter.Convert(_activity, TestOptions.Get()));
        }

        [Fact]
        public void ConvertActivityWithSmsChannelIdToOmniSmsFailoverMessage()
        {
            _activity.Text = "Test text";
            _activity.ChannelId = InfobipSmsConstants.ChannelName;

            var omniFailoverMessages = ToInfobipSmsConverter.Convert(_activity, TestOptions.Get());
            Assert.NotNull(omniFailoverMessages);
            Assert.Single(omniFailoverMessages);

            var omniFailoverMessage = omniFailoverMessages.First();
            Assert.Equal(omniFailoverMessage.ScenarioKey, TestOptions.SmsScenarioKey);
            CheckDestinations(omniFailoverMessage.Destinations);

            Assert.NotNull(omniFailoverMessage);
            Assert.NotNull(omniFailoverMessage.Sms);
        }

        private void CheckDestinations(InfobipDestination[] destinations)
        {
            Assert.NotNull(destinations);
            Assert.Single(destinations);
            
            var destination = destinations.First();
            Assert.NotNull(destination.To);
            Assert.Equal(destination.To.PhoneNumber, _activity.Recipient.Id);
        }
    }
}
