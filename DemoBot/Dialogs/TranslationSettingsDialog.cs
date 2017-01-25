using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace DemoBot.Dialogs
{
    [Serializable]
    public class TranslationSettingsDialog : IDialog<TranslationSettingsDialogResult>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(HandleUserMessageAsync);
            return Task.CompletedTask;
        }

        private async Task HandleUserMessageAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            switch (message.Text)
            {
                case "//translate on":
                    PromptDialog.Choice(context, HandleLanguageChoiceAsync, new[] { "de", "en", "nl", "es", "it" }, "To what language would you like the tweets to be translated?");
                    return;
                case "//translate off":
                    await DeactivateTranslationAsync(context, message);
                    return;
                default:
                    string languageCode;
                    if (context.ConversationData.TryGetValue("translationLanguage", out languageCode))
                    {
                        await context.PostAsync($"I'm currently translating tweets to '{languageCode}'.");
                    }
                    else
                    {
                        await context.PostAsync("I'm not currently translating tweets. Use '//translate on' to enable translation.");
                    }
                    context.Done(TranslationSettingsDialogResult.NothingChanged);
                    return;
            }
        }

        private async Task HandleLanguageChoiceAsync(IDialogContext context, IAwaitable<string> result)
        {
            var languageCode = await result;
            context.ConversationData.SetValue("translationLanguage", languageCode);
            await context.PostAsync($"Okay, I will try to translate all tweets to '{languageCode}'.");
            context.Done(TranslationSettingsDialogResult.TranslationActivated);
        }

        private async Task DeactivateTranslationAsync(IDialogContext context, IMessageActivity message)
        {
            string languageCode;
            if (!context.ConversationData.TryGetValue("translationLanguage", out languageCode))
            {
                await context.PostAsync("Translation was not active.");
                context.Done(TranslationSettingsDialogResult.NothingChanged);
                return;
            }

            PromptDialog.Confirm(context, HandleDeactivateTranslationChoice, $"Are you sure that you want to disable translation to '{languageCode}'?");
        }

        private async Task HandleDeactivateTranslationChoice(IDialogContext context, IAwaitable<bool> result)
        {
            var confirm = await result;
            if (confirm)
            {
                context.ConversationData.RemoveValue("translationLanguage");
                await context.PostAsync("Okay, I will no longer translate tweets.");
                context.Done(TranslationSettingsDialogResult.TranslationDeactivated);
            }
            else
            {
                await context.PostAsync("Okay, I will keep translating...");
                context.Done(TranslationSettingsDialogResult.NothingChanged);
            }
        }
    }

    public enum TranslationSettingsDialogResult
    {
        TranslationActivated,
        TranslationDeactivated,
        NothingChanged
    }
}
