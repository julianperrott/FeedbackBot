namespace FeedbackBot
{
    using System;
    using System.Collections.Generic;

    public class Bot
    {
        private static Dictionary<BotStatus, Func<BotState, string, BotReponse>> Step = new Dictionary<BotStatus, Func<BotState, string, BotReponse>>
        {
            { BotStatus.Greeting, GreetCustomer },
            { BotStatus.Participate,Participate  },
            { BotStatus.SpecialEvent,SpecialEvent  },
            { BotStatus.SpecialEventResponse,SpecialEventResponse },
            { BotStatus.RateExperience, RateExperience }
        };

        public static BotReponse MessageReceived(BotState botState, string text)
        {
            return Step[botState.BotStatus](botState, text);
        }

        public static BotReponse Say(BotState state, BotStatus status, string text)
        {
            state.BotStatus = status;

            return new BotReponse
            {
                BotState = state,
                ResponseText = text
            };
        }

        public static BotReponse End(BotState state, string text)
        {
            state.BotStatus = BotStatus.End;

            return new BotReponse
            {
                BotState = state,
                ResponseText = text
            };
        }

        public static BotReponse GreetCustomer(BotState state, string text)
        {
            return Say(state, BotStatus.Participate, "Hi, you recently visited the hogwarts virtual wizard school, would you like to be entered into a draw to win a wand by answering a couple of quick questions ?");
        }

        private static BotReponse Participate(BotState state, string arg)
        {
            var result = new DialogFlow().TextToResponse(arg, DialogFlow.EntityName.YesNoEntity);

            if (result == DialogFlowResponse.Yes)
            {
                state.ParticipationAgreed = true;
                return Say(state, BotStatus.SpecialEvent, "Thanks, when you attended was it for a special event ?");
            }
            else
            {
                return End(state, "Ok, thanks for your visit, we hope to see you again soon, farewell.");
            }
        }

        private static BotReponse SpecialEvent(BotState state, string arg)
        {
            var result = new DialogFlow().TextToResponse(arg, new List<DialogFlow.EntityName> { DialogFlow.EntityName.SpecialEventEntity, DialogFlow.EntityName.YesNoEntity });

            if (result.ContainsKey(DialogFlow.EntityName.SpecialEventEntity))
            {
                state.VisitSpecialEvent = result[DialogFlow.EntityName.SpecialEventEntity].ToString();
                return Say(state, BotStatus.RateExperience, "Great, Please could you rate your visit on a scale of 1 to 10, 10 being best ?");
            }

            if (result.ContainsKey(DialogFlow.EntityName.YesNoEntity))
            {
                var value = result[DialogFlow.EntityName.YesNoEntity];
                if (value == DialogFlowResponse.Yes)
                {
                    return Say(state, BotStatus.SpecialEventResponse, "Ok, what was the special event you came for ?");
                }
                else
                {
                    return Say(state, BotStatus.RateExperience, "Please could you rate your visit on a scale of 1 to 10, 10 being best?");
                }
            }

            return Say(state, BotStatus.SpecialEvent, "Oh dear, I didn't understand what you said. Were you here for a special event ?");
        }

        private static BotReponse SpecialEventResponse(BotState state, string arg)
        {
            var result = new DialogFlow().TextToResponse(arg, DialogFlow.EntityName.SpecialEventEntity);

            if (result == DialogFlowResponse.NotRecognised)
            {
                return Say(state, BotStatus.SpecialEvent, "Oh dear, I didn't understand what you said. What was the special event you came for ?");
            }

            state.VisitSpecialEvent = result.ToString();
            return Say(state, BotStatus.RateExperience, "Great, Please could you rate your visit on a scale of 1 to 10, 10 being best ?");
        }

        private static BotReponse RateExperience(BotState state, string arg)
        {
            var result = new DialogFlow().TextToResponse(arg, DialogFlow.EntityName.NumberEntity);

            if (result != DialogFlowResponse.UnknownResponse)
            {
                state.VisitRating = result.ToString();

                state.BotStatus = BotStatus.End;

                return End(state, "Ok, thanks for your visit, we hope to see you again soon, farewell.");
            }

            return Say(state, BotStatus.RateExperience, "Oh dear I didn't understand you. Please could you rate your visit on a scale of 1 to 10, 10 being best ?");
        }
    }
}