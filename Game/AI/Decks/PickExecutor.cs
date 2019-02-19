using YGOSharp.OCGWrapper.Enums;
using System.Collections.Generic;
using WindBot;
using WindBot.Game;
using WindBot.Game.AI;

namespace WindBot.Game.AI.Decks
{
    [Deck("Pick", "AI_Trickstar")]
    public class PickExecutor : DefaultExecutor
    {
        public class CardId
        {
            public const int LeoWizard = 4392470;
            public const int Bunilla = 69380702;
        }

        public PickExecutor(GameAI ai, Duel duel)
            : base(ai, duel)
        {
            AddExecutor(ExecutorType.SpSummon);
            AddExecutor(ExecutorType.Activate, DefaultDontChainMyself);
            AddExecutor(ExecutorType.SummonOrSet);
            AddExecutor(ExecutorType.Repos, DefaultMonsterRepos);
            AddExecutor(ExecutorType.SpellSet);
        }

        IList<ClientCard> picked = new List<ClientCard>();
        public override IList<ClientCard> OnSelectCard(IList<ClientCard> cards, int min, int max, int hint, bool cancelable)
        {
            if (hint == 507 && Duel.Turn == 1)
            {
                Logger.DebugWriteLine("Call SelectCard with hint 507");
                if (cancelable)
                {
                    IList<ClientCard> submit = picked;
                    picked.Clear();
                    return submit;
                }
                ClientCard thispick = cards[Program.Rand.Next(cards.Count)];
                IList <ClientCard> group = new List<ClientCard>();
                group.Add(thispick);
                picked.Add(thispick);
                return group;
            }
            if (Duel.Phase == DuelPhase.BattleStart)
                return null;
            IList<ClientCard> selected = new List<ClientCard>();

            // select the last cards
            for (int i = 1; i <= max; ++i)
                selected.Add(cards[cards.Count - i]);

            return selected;
        }

        public override int OnSelectOption(IList<int> options)
        {
            return Program.Rand.Next(options.Count);
        }

    }
}