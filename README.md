# DemoBot

A chatbot, written with the Microsoft Bot Framework SDK, for demo purposes (first shown in the [Azure Ruhrgebiet Meetup](http://azure-ruhrgebiet.de)).  
It answers to any query with the latest Twitter tweet returned from the Twitter search API.  
It can optionally translate these tweets to a number of languages using the Microsoft Translator Text API from the Cognitive Services Suite.

## Using the Bot

### Prerequisites

- You will need to register the application in the Twitter developer portal to retrieve the required consumer key and consumer secret (it is free for small amounts of calls). You can do that [here](https://apps.twitter.com/). Enter the key and secret into the corresponding string constants in the `Services/TwitterService.cs` file.
- To test interaction with the bot locally, you should install the [Bot Framework Emulator](https://docs.botframework.com/en-us/tools/bot-framework-emulator/).
- If you want to use the translation service, you will need to create a translation API instance in the Azure Portal (details [here](https://docs.microsoft.com/en-us/azure/cognitive-services/cognitive-services-text-analytics-quick-start), there is a free tier available) and enter the subscription key to the constant in `Services/TranslationService.cs`.

### Running the Bot

1. Open the solution in Visual Studio (at least 2017 RC - not a bot framework requirement, but I used it for this demo) and hit `F5`. It should restore the referenced NuGet packages, build the project and open it in your selected browser, showing the default page. Make note of the port that the webservice is running on.
1. Open the Bot Framework Emulator and enter the URL to your web service using the port noted earlier. This should look similar to `http://localhost:<port>/api/messages`. Click `Connect`.
1. Enter a message. The bot should reply first with a TYPING indicator and afterwards with the latest tweet it found.
1. If you entered the required subscription key for the translation service, you can try to activate it by typing `//translate on`.

If you want to deploy this demo to Azure App Service and register it with the Bot Framework, you can use the steps explained in the official [Bot Framework Getting Started Guide](https://docs.botframework.com/en-us/csharp/builder/sdkreference/gettingstarted.html).

## Features used

- Dialogs SDK
- Bot State Service
- Azure App Service (shown in demo)
- Bot Framework Service and integration with Skype (shown in demo)

Not shown (due to time constraints) are for example FormFlow, Calling and integration with the Language Understanding Intelligent Service (LUIS).