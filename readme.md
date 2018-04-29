### Using a Chat bot to phone your customers and talk to them. 

Would you like to be able to phone your customers and let a chat bot talk to them? This is relatively easy to do and I will show you how.   
There exist many reasons for communicating with your customers. 

Giving information. Some examples are: 
* Courier delivery updates 
* Login issues 
* Banking events 
* Traffic alerts 
* Schedule changes 

and requesting information (who, what, when, where, why).  Some examples are: 
* Courier delivery confirmation 
* Satisfaction / Feedback survey 
* Checking attendance 
* Status requests 
* Location requests 

Not all customers are able to or desire to use modern technologies to interact with your system, so a phone call gets around this problem. But the call must be life like and easy to use to be successful. 

The main concerns to be considered, and the service I have used are as follows: 
| Concern  | Service |
|----------|:--------:|
| How to Initiate the call. | Azure Function|
| Starting and controlling the phone call. | Twilio|
| Converting the customer speech into text. | Twilio|
| Converting the application text into speech.| Twilio or Amazon Polly| 
| Understanding the customer text | DialogFlow |
| Determining what to say to the customer. | DialogFlow |
| Persisting the data. | Twilio Session during call|
| Integrating the services | Azure function |

These concerns can be satisfied by various different online services so you don't need to develop them all yourself. I will use quite a simple implementation to show a working example.

## Example application 
![Example Application](https://raw.githubusercontent.com/julianperrott/FeedbackBot/master/post/img/Twilio_App_Overview.png "Example Application")

Our simple example application will ask our user for feedback on a visit to a theme park. And the application will flow like this: 

## The logic in the conversation

DialogFlow is a free Google product which allows the intent of human speech / text to be understood. This makes it easier to write conversational bots. The bot I have written is very basic and barely scratches the surface of what DialogFlow can do. The capabilities of DialogFlow need to be understand well as it will influence how much logic you can set up inside it, and how much needs to be coded in your bot. 

If you are new to DialogFlow then check out their tutorials: 
* https://dialogflow.com/docs/getting-started/building-your-first-agent 
* https://developers.google.com/actions/dialogflow/first-app 

The DialogFlow agent I have written asks the following 3 questions: 
1. Would you like to participate? A 'No' response ends the bot. 
1. Did you visit for a special event ? 
1. And finally get a rating of the experience. 

Three Contexts are moved through during the conversation to control which Intents can be matched by the user input. 
For example if the user said "yes" intent 2.0 would only be matched if the current context was "SpecialEvent". 

![Dialog flow contexts](https://raw.githubusercontent.com/julianperrott/FeedbackBot/master/post/img/Twilio_DialogFlow_Overview.png "Dialog flow contexts")

The agent also has the following Entities which are set up to match the possible responses to their related questions. 

 ![Dialog flow Entities](https://raw.githubusercontent.com/julianperrott/FeedbackBot/master/post/img/DialogFlow_Entities.png "Dialog flow Entities")

You can restore the agent from a file in my [GitHub repo](https://github.com/julianperrott/FeedbackBot) and you can try it out [here](https://bot.dialogflow.com/ddd79d30-0b82-4e7f-bc6d-2d74e01beb95).

Start the conversation off by saying "startcall". 

 
![DialogFlow Conversation](https://raw.githubusercontent.com/julianperrott/FeedbackBot/master/post/img/FeedbackBot_StartBot_3.png "DialogFlow Conversation")
 
## Talking to DialogFlow from a C# .net application 

We are going to use Azure functions to pass on messages from Twilio to Dialogflow and then send a response back to Twilio. To allow our function to talk to DialogFlow we are going to add a nuget package [ApiAiSDK](https://github.com/dialogflow/dialogflow-dotnet-client). We are then able to call dialog flow as follows: 

 ![C# code](https://raw.githubusercontent.com/julianperrott/FeedbackBot/master/post/img/FeedbackBot_StartBot_2.png "C# code")

The response includes the speech to say to the user and any parameters which contain the information we have collected. There is also other information such as the context and intent which can be useful for testing. 

The application solution comprises of following projects: 

| Project | Description |
|--|:--:|
|FeedbackBotAzureFunctions  | Function StartCall makes a request to Twilio to make a phone call. Function MsgIn receives a request from Twilio. It passes the user text to the bot, then returns the bot response to Twilio. Any state required to be kept is stored in a cookie passed to and from Twilio. | 
| FeedbackBot | Handles the user conversation with DialogFlow |
| IntegrationTests | Tests to run through a conversation and to check the intents and contexts are correct with both DialogFlow and the Azure function. |
| BotFrameworkAPI | Allows an alternate route into the bot so that an interactive test can be performed using [Microsoft's Bot Framework Emulator](https://github.com/Microsoft/BotFramework-Emulator) |

The azure functions will need to be published to Azure. You can learn more about that [here](https://tutorials.visualstudio.com/first-azure-function/publish)


## Setting up Twilio 

Now that we have our application code we need to set up Twilio to talk to it. Twilio allow you to sign up for a free account. 

Sign up for a Twilio Account [here](https://www.twilio.com/try-twilio)

1. Create a project. Make a note of the Account SID and auth token 
1. Choose get started with twilio for "Programmable voice" 
1. Get a phonenumber. Make a note of it. 
1. Confirm programming language as C#. 
1. Now click on Numbers and select Manage Numbers. 
1. Now click on Tools and TwiML Apps and create a new app. 
1. The Voice Request URL can then be filled in with our MsgIn azure function URL. 

We now need to go back to our function project and fill in the following in the StartCall azure function: 

* PhoneNumberToCall 
* TwilioAccountSid 
* TwilioAuthToken 
* TwilioProgVoiceApplicationSid 
* phoneNumberCallIsFrom 

We also need to set the following in Bot.cs so that the bot talks to your DialogFlow  
* DialogFlowAccessToken 

## The Audio 

The Twilio voices are a little robotic. There are more lifelike voices available from 3rd parties. Here is a recorded call using Twilio voice:  [Twilio Voice Recording](https://raw.githubusercontent.com/julianperrott/FeedbackBot/master/post/sound/TwilioVoice.wav)

One of which is Amazon Polly. You can listen to some of the voices [here](https://aws.amazon.com/polly). Here is a recorded call using Polly Brian voice:  [Amazon Polly Voice Recording](https://raw.githubusercontent.com/julianperrott/FeedbackBot/master/post/sound/PollyBrianVoice.wav)
 

Another text to speech service is available [here](https://docs.microsoft.com/en-us/azure/cognitive-services/speech/api-reference-rest/bingvoiceoutput) from Microsoft.

 

## Conclusions 

It's pretty straight forward to write a bot you can talk to once you understand how the various components work and can be put together. Using a 3rd party voice gives less robotic speech, but it does add a slight delay. 

 

This post was inspired by this [article](https://www.linkedin.com/pulse/next-generation-ivr-using-twilio-speech-recognition-chatbots-badri/).

 

 

 

 