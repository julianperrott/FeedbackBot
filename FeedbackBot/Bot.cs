using System;
using System.Collections.Generic;

namespace FeedbackBot
{
    public class Bot
    {
        private static Dictionary<BotState, Func<string, BotReponse>> Step = new Dictionary<BotState, Func<string, BotReponse>>
        {
            { BotState.Greeting, GreetCustomer },
            { BotState.Participate,Participate  },
            { BotState.SpecialEvent,SpecialEvent  },
            { BotState.SpecialEventResponse,SpecialEventResponse },
            { BotState.RateExperience, RateExperience }
        };

        public static BotReponse MessageReceived(BotState botState, string text)
        {
            return Step[botState](text);
        }

        public static BotReponse Say(BotState state, string text)
        {
            return new BotReponse
            {
                BotState = state,
                ResponseAction = ResponseAction.Say,
                ResponseText = text
            };
        }

        public static BotReponse GreetCustomer(string text)
        {
            return Say(BotState.Participate, "Hi, you recently visited the hogwarts virtual wizard school, would you like to be entered into a draw to win a wand by answering a couple of quick questions ?");
        }

        private static BotReponse Participate(string arg)
        {
            var result = new DialogFlow().TextToResponse(arg, DialogFlow.EntityName.YesNoEntity);

            if (result == DialogFlowResponse.Yes)
            {
                return Say(BotState.SpecialEvent, "Thanks, when you attended was it for a special event ?");
            }
            else
            {
                return new BotReponse
                {
                    BotState = BotState.End,
                    ResponseAction = ResponseAction.EndCall,
                    ResponseText = "Ok, thanks for your visit, we hope to see you again soon, farewell."
                };
            }
        }

        private static BotReponse SpecialEvent(string arg)
        {
            var result = new DialogFlow().TextToResponse(arg, new List<DialogFlow.EntityName> { DialogFlow.EntityName.SpecialEventEntity, DialogFlow.EntityName.YesNoEntity });

            if (result.ContainsKey(DialogFlow.EntityName.SpecialEventEntity))
            {
                return Say(BotState.RateExperience, "Great, Please could you rate your visit on a scale of 1 to 10, 10 being best ?");
            }

            if (result.ContainsKey(DialogFlow.EntityName.YesNoEntity))
            {
                var value = result[DialogFlow.EntityName.YesNoEntity];
                if (value == DialogFlowResponse.Yes)
                {
                    return Say(BotState.SpecialEventResponse, "Ok, what was the special event you came for ?");
                }
                else
                {
                    return Say(BotState.RateExperience, "Please could you rate your visit on a scale of 1 to 10, 10 being best?");
                }
            }

            return Say(BotState.SpecialEvent, "Oh dear, I didn't understand what you said. Were you here for a special event ?");
        }

        private static BotReponse SpecialEventResponse(string arg)
        {
            var result = new DialogFlow().TextToResponse(arg, DialogFlow.EntityName.YesNoEntity);

            if (result == DialogFlowResponse.NotRecognised)
            {
                return Say(BotState.SpecialEvent, "Oh dear, I didn't understand what you said. What was the special event you came for ?");
            }

            return Say(BotState.RateExperience, "Great, Please could you rate your visit on a scale of 1 to 10, 10 being best ?");
        }

        private static BotReponse RateExperience(string arg)
        {
            var result = new DialogFlow().TextToResponse(arg, DialogFlow.EntityName.NumberEntity);

            if (result != DialogFlowResponse.UnknownResponse)
            {
                return new BotReponse
                {
                    BotState = BotState.End,
                    ResponseAction = ResponseAction.EndCall,
                    ResponseText = "Ok, thanks for your visit, we hope to see you again soon, farewell."
                };
            }

            return Say(BotState.RateExperience, "Oh dear I didn't understand you. Please could you rate your visit on a scale of 1 to 10, 10 being best ?");
        }
    }
}