namespace FeedbackBot
{
    using System;
    using System.Collections.Generic;

    public class Bot
    {
        public const string DialogFlowAccessToken = "9394786d9d8040d4864c4ea1ef49c71e";

        private readonly DialogFlow dialogFlowMapper;

        private const string RatingIntent = "RatingIntent";
        private const string SpecialEventIntent = "SpecialEventIntent";
        private const string YesNoIntent = "YesNoIntent";

        public Bot(DialogFlow dialogFlowMapper)
        {
            this.dialogFlowMapper = dialogFlowMapper;

            State = new Dictionary<BotStatus, Func<BotState, string, BotReponse>>
            {
                { BotStatus.Greeting, GreetCustomerState },
                { BotStatus.Participate, ParticipateState  },
                { BotStatus.SpecialEvent, SpecialEventState  },
                { BotStatus.SpecialEventResponse, SpecialEventResponseState },
                { BotStatus.RateExperience, RateExperienceState }
            };
        }

        private Dictionary<BotStatus, Func<BotState, string, BotReponse>> State;

        public BotReponse MessageReceived(BotState botState, string text)
        {
            return State[botState.BotStatus](botState, text);
        }

        public BotReponse Say(BotState state, BotStatus status, string text)
        {
            state.BotStatus = status;

            return new BotReponse
            {
                BotState = state,
                ResponseText = text
            };
        }

        public BotReponse End(BotState state, string text)
        {
            state.BotStatus = BotStatus.End;

            return new BotReponse
            {
                BotState = state,
                ResponseText = text
            };
        }

        public BotReponse GreetCustomerState(BotState state, string text)
        {
            return Say(state, BotStatus.Participate, "Hi, you recently visited the hogwarts virtual wizard school, would you like to be entered into a prize draw by answering a couple of quick questions ?");
        }

        private BotReponse ParticipateState(BotState state, string arg)
        {
            var result = dialogFlowMapper.TextToResponse(arg, YesNoIntent, DialogFlowEntityName.YesNoEntity);

            if (result == "Yes")
            {
                state.ParticipationAgreed = true;
                return Say(state, BotStatus.SpecialEvent, "Thanks, when you attended was it for a special event ?");
            }
            else if (result == "No")
            {
                return End(state, "Ok, thanks for your visit, we hope to see you again soon, farewell.");
            }

            return Say(state, BotStatus.Participate, "Oh dear, I didn't understand what you said. Please say yes or no ?");
        }

        private BotReponse SpecialEventState(BotState state, string arg)
        {
            var allowedIntents = new List<string> { SpecialEventIntent, YesNoIntent };
            var allowedEntities = new List<DialogFlowEntityName> { DialogFlowEntityName.SpecialEventEntity, DialogFlowEntityName.YesNoEntity };

            var result = dialogFlowMapper.TextToResponses(arg, allowedIntents, allowedEntities);

            if (result.ContainsKey(DialogFlowEntityName.SpecialEventEntity.ToString()))
            {
                state.VisitSpecialEvent = result[DialogFlowEntityName.SpecialEventEntity.ToString()].ToString();
                return Say(state, BotStatus.RateExperience, "Great, Please could you rate your visit on a scale of poor, average, good or excellent ?");
            }

            if (result.ContainsKey(DialogFlowEntityName.YesNoEntity.ToString()))
            {
                var value = result[DialogFlowEntityName.YesNoEntity.ToString()];
                if (value == "Yes")
                {
                    return Say(state, BotStatus.SpecialEventResponse, "Ok, what was the special event you came for ?");
                }
                else
                {
                    state.VisitSpecialEvent = "no";
                    return Say(state, BotStatus.RateExperience, "Please could you rate your visit on a scale of of poor, average, good or excellent?");
                }
            }

            return Say(state, BotStatus.SpecialEvent, "Oh dear, I didn't understand what you said. Were you here for a special event ?");
        }

        private BotReponse SpecialEventResponseState(BotState state, string arg)
        {
            var result = dialogFlowMapper.TextToResponse(arg, SpecialEventIntent, DialogFlowEntityName.SpecialEventEntity);

            if (string.IsNullOrEmpty(result))
            {
                return Say(state, BotStatus.SpecialEvent, "Oh dear, I didn't understand what you said. What was the special event you came for ?");
            }

            state.VisitSpecialEvent = result.ToString();
            return Say(state, BotStatus.RateExperience, "Great, Please could you rate your visit on a scale of poor, average, good or excellent?");
        }

        private BotReponse RateExperienceState(BotState state, string arg)
        {
            var result = dialogFlowMapper.TextToResponse(arg, RatingIntent, DialogFlowEntityName.RatingEntity);

            if (!string.IsNullOrEmpty(result))
            {
                state.VisitRating = result.ToString();

                state.BotStatus = BotStatus.End;

                return End(state, "Ok, thanks for your visit, we hope to see you again soon, farewell.");
            }

            return Say(state, BotStatus.RateExperience, "Oh dear I didn't understand you. Please could you rate your visit on a scale of 1 to 10, 10 being best ?");
        }
    }
}