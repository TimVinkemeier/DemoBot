using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DemoBot.Services;
using Humanizer;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace DemoBot.Dialogs
{
    [Serializable]
    public class TwitterSearchDialog : IDialog
    {
        protected int count = 0;

        public Task StartAsync(IDialogContext context)
        {
            context.Wait(HandleUserMessageAsync);
            return Task.CompletedTask;
        }

        private async Task HandleUserMessageAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            if (message.Text.StartsWith("//translate"))
            {
                await context.Forward(new TranslationSettingsDialog(), (ctx, r) =>
                {
                    ctx.Wait(HandleUserMessageAsync);
                    return Task.CompletedTask;
                }, message, CancellationToken.None);
                return;
            }

            switch (message.Text)
            {
                case "//reset":
                    await PromptForResetAsync(context, message);
                    return;
                case "//count":
                    await ShowCountAsync(context, message);
                    return;
                case "//last":
                    await ShowLastQueryAsync(context, message);
                    return;
                default:
                    await AnswerWithTweetAndIncreaseCountAsync(context, message);
                    break;
            }
        }

        private async Task ShowLastQueryAsync(IDialogContext context, IMessageActivity message)
        {
            if (!context.ConversationData.TryGetValue("lastQuery", out string lastQuery))
            {
                await context.PostAsync("You did not have any query yet, as it seems...");
                context.Wait(HandleUserMessageAsync);
                return;
            }

            await context.PostAsync($"Your last recorded query was '{lastQuery}'.");
            context.Wait(HandleUserMessageAsync);
        }

        private async Task ShowCountAsync(IDialogContext context, IMessageActivity message)
        {
            await context.PostAsync($"Your current request count is {count}.");
        }

        private async Task AnswerWithTweetAndIncreaseCountAsync(IDialogContext context, IMessageActivity message)
        {
            if (string.IsNullOrWhiteSpace(message.Text))
            {
                await context.PostAsync("Sorry, but I can not search for empty queries :/");
                context.Wait(HandleUserMessageAsync);
                return;
            }

            var result = await TwitterService.GetTopResultForSearchQueryAsync(message.Text);
            if (result == TwitterSearchResult.Empty)
            {
                await context.PostAsync($"No result on Twitter could be found for your query '{message.Text}'. This request will not be billed.");
            }
            else
            {
                await context.PostAsync($"Here is what I found for '{message.Text}':");

                var reply = context.MakeMessage();
                reply.Text = result.Text;
                var translated = false;
                if (context.ConversationData.TryGetValue("translationLanguage", out string languageCode))
                {
                    reply.Text = await TranslationService.TranslateAsync(reply.Text, languageCode);
                    translated = true;
                }

                reply.Attachments = new List<Attachment>
                {
                    new Attachment
                    {
                        ContentUrl = result.ImageUrl,
                        ContentType = result.ImageUrl != null && (result.ImageUrl.EndsWith("jpg") || result.ImageUrl.EndsWith("jpeg")) ? "image/jpg" : "image/png"
                    }
                };
                await context.PostAsync(reply);

                if (translated)
                {
                    await context.PostAsync($"This tweet was automatically translated to '{languageCode}'. Original text: {result.Text}");
                }

                count++;
                await context.PostAsync($"This was your {count.ToOrdinalWords()} request.");

                context.ConversationData.SetValue("lastQuery", message.Text);
            }

            context.Wait(HandleUserMessageAsync);
        }

        private Task PromptForResetAsync(IDialogContext context, IMessageActivity message)
        {
            PromptDialog.Confirm(context, ResetConfirmedAsync, "Do you really want to reset your request count?");
            return Task.CompletedTask;
        }

        private async Task ResetConfirmedAsync(IDialogContext context, IAwaitable<bool> result)
        {
            var confirmed = await result;
            if (confirmed)
            {
                count = 0;
                await context.PostAsync("Count reset.");
            }
            else
            {
                await context.PostAsync("Okay, I did not reset the count.");
            }
            context.Wait(HandleUserMessageAsync);
        }
    }
}
