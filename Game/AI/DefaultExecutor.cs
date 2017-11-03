﻿using System;
using System.Collections.Generic;
using YGOSharp.OCGWrapper.Enums;
using WindBot;
using WindBot.Game;
using WindBot.Game.AI;

namespace WindBot.Game.AI
{
    public abstract class DefaultExecutor : Executor
    {
        private enum CardId
        {
            JizukirutheStarDestroyingKaiju = 63941210,
            GadarlatheMysteryDustKaiju = 36956512,
            GamecieltheSeaTurtleKaiju = 55063751,
            RadiantheMultidimensionalKaiju = 28674152,
            KumongoustheStickyStringKaiju = 29726552,
            ThunderKingtheLightningstrikeKaiju = 48770333,
            DogorantheMadFlameKaiju = 93332803,
            SuperAntiKaijuWarMachineMechaDogoran = 84769941,

            MysticalSpaceTyphoon = 5318639,
            CosmicCyclone = 8267140,
            ChickenGame = 67616300,

            CastelTheSkyblasterMusketeer = 82633039
        }

        protected DefaultExecutor(GameAI ai, Duel duel)
            : base(ai, duel)
        {
            AddExecutor(ExecutorType.Activate, (int)CardId.ChickenGame, DefaultChickenGame);
        }

        /// <summary>
        /// Destroy face-down cards first, in our turn.
        /// </summary>
        protected bool DefaultMysticalSpaceTyphoon()
        {
            foreach (ClientCard card in CurrentChain)
                if (card.Id == (int)CardId.MysticalSpaceTyphoon)
                    return false;

            List<ClientCard> spells = Enemy.GetSpells();
            if (spells.Count == 0)
                return false;

            ClientCard selected = Enemy.SpellZone.GetFloodgate();

            if (selected == null)
            {
                foreach (ClientCard card in spells)
                {
                    if (Duel.Player == 1 && !card.HasType(CardType.Continuous))
                        continue;
                    selected = card;
                    if (Duel.Player == 0 && card.IsFacedown())
                        break;
                }
            }

            if (selected == null)
                return false;
            AI.SelectCard(selected);
            return true;
        }

        /// <summary>
        /// Destroy face-down cards first, in our turn.
        /// </summary>
        protected bool DefaultCosmicCyclone()
        {
            foreach (ClientCard card in CurrentChain)
                if (card.Id == (int)CardId.CosmicCyclone)
                    return false;
            return (Duel.LifePoints[0] > 1000) && DefaultMysticalSpaceTyphoon();
        }

        /// <summary>
        /// Activate if avail.
        /// </summary>
        protected bool DefaultGalaxyCyclone()
        {
            List<ClientCard> spells = Enemy.GetSpells();
            if (spells.Count == 0)
                return false;

            ClientCard selected = null;

            if (Card.Location == CardLocation.Grave)
            {
                selected = Enemy.SpellZone.GetFloodgate();
                if (selected == null)
                {
                    foreach (ClientCard card in spells)
                    {
                        if (!card.IsFacedown())
                        {
                            selected = card;
                            break;
                        }
                    }
                }
            }
            else
            {
                foreach (ClientCard card in spells)
                {
                    if (card.IsFacedown())
                    {
                        selected = card;
                        break;
                    }
                }
            }

            if (selected == null)
                return false;

            AI.SelectCard(selected);
            return true;
        }

        /// <summary>
        /// Set the highest ATK level 4+ effect enemy monster.
        /// </summary>
        protected bool DefaultBookOfMoon()
        {
            if (AI.Utils.IsAllEnemyBetter(true))
            {
                ClientCard monster = Enemy.GetMonsters().GetHighestAttackMonster();
                if (monster != null && monster.HasType(CardType.Effect) && (monster.HasType(CardType.Xyz) || monster.Level > 4))
                {
                    AI.SelectCard(monster);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Return problematic monster, and if this card become target, return any enemy monster.
        /// </summary>
        protected bool DefaultCompulsoryEvacuationDevice()
        {
            ClientCard target = AI.Utils.GetProblematicEnemyMonster();
            if (target != null)
            {
                AI.SelectCard(target);
                return true;
            }
            if (AI.Utils.IsChainTarget(Card))
            {
                ClientCard monster = AI.Utils.GetBestEnemyMonster();
                if (monster != null)
                {
                    AI.SelectCard(monster);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Revive the best monster when we don't have better one in field.
        /// </summary>
        protected bool DefaultCallOfTheHaunted()
        {
            if (!AI.Utils.IsAllEnemyBetter(true))
                return false;
            ClientCard selected = null;
            int BestAtk = 0;
            foreach (ClientCard card in Bot.Graveyard)
            {
                if (card.Attack > BestAtk)
                {
                    BestAtk = card.Attack;
                    selected = card;
                }
            }
            AI.SelectCard(selected);
            return true;
        }

        /// <summary>
        /// Chain the enemy monster.
        /// </summary>
        protected bool DefaultBreakthroughSkill()
        {
            ClientCard LastChainCard = GetLastChainCard();

            if (LastChainCard == null)
                return false;

            return LastChainCard.Controller == 1 && LastChainCard.Location == CardLocation.MonsterZone && DefaultUniqueTrap();
        }

        /// <summary>
        /// Activate only except this card is the target or we summon monsters.
        /// </summary>
        protected bool DefaultSolemnJudgment()
        {
            return !AI.Utils.IsChainTargetOnly(Card) && !(Duel.Player == 0 && LastChainPlayer == -1) && DefaultTrap();
        }

        /// <summary>
        /// Activate only except we summon monsters.
        /// </summary>
        protected bool DefaultSolemnWarning()
        {
            return (Duel.LifePoints[0] > 2000) && !(Duel.Player == 0 && LastChainPlayer == -1) && DefaultTrap();
        }

        /// <summary>
        /// Activate only except we summon monsters.
        /// </summary>
        protected bool DefaultSolemnStrike()
        {
            return (Duel.LifePoints[0] > 1500) && !(Duel.Player == 0 && LastChainPlayer == -1) && DefaultTrap();
        }

        /// <summary>
        /// Activate when all enemy monsters have better ATK.
        /// </summary>
        protected bool DefaultTorrentialTribute()
        {
            return !HasChainedTrap(0) && AI.Utils.IsAllEnemyBetter(true);
        }

        /// <summary>
        /// Activate enemy have more S&T.
        /// </summary>
        protected bool DefaultHeavyStorm()
        {
            return Bot.GetSpellCount() < Enemy.GetSpellCount();
        }

        /// <summary>
        /// Activate before other winds, if enemy have more than 2 S&T.
        /// </summary>
        protected bool DefaultHarpiesFeatherDusterFirst()
        {
            return Enemy.GetSpellCount() >= 2;
        }

        /// <summary>
        /// Activate when one enemy monsters have better ATK.
        /// </summary>
        protected bool DefaultHammerShot()
        {
            return AI.Utils.IsOneEnemyBetter(true);
        }

        /// <summary>
        /// Activate when one enemy monsters have better ATK or DEF.
        /// </summary>
        protected bool DefaultDarkHole()
        {
            return AI.Utils.IsOneEnemyBetter();
        }

        /// <summary>
        /// Activate when one enemy monsters have better ATK or DEF.
        /// </summary>
        protected bool DefaultRaigeki()
        {
            return AI.Utils.IsOneEnemyBetter();
        }

        /// <summary>
        /// Activate when one enemy monsters have better ATK or DEF.
        /// </summary>
        protected bool DefaultSmashingGround()
        {
            return AI.Utils.IsOneEnemyBetter();
        }

        /// <summary>
        /// Activate when we have more than 15 cards in deck.
        /// </summary>
        protected bool DefaultPotOfDesires()
        {
            return Bot.Deck.Count > 15;
        }

        /// <summary>
        /// Set traps only and avoid block the activation of other cards.
        /// </summary>
        protected bool DefaultSpellSet()
        {
            return (Card.IsTrap() || Card.HasType(CardType.QuickPlay)) && Bot.GetSpellCountWithoutField() < 4;
        }

        /// <summary>
        /// Summon with tributes ATK lower.
        /// </summary>
        protected bool DefaultTributeSummon()
        {
            foreach (ClientCard card in Bot.MonsterZone)
            {
                if (card != null &&
                    card.Id == Card.Id &&
                    card.HasPosition(CardPosition.FaceUp))
                    return false;
            }
            int tributecount = (int)Math.Ceiling((Card.Level - 4.0d) / 2.0d);
            for (int j = 0; j < 7; ++j)
            {
                ClientCard tributeCard = Bot.MonsterZone[j];
                if (tributeCard == null) continue;
                if (tributeCard.GetDefensePower() < Card.Attack)
                    tributecount--;
            }
            return tributecount <= 0;
        }

        /// <summary>
        /// Activate when we have no field.
        /// </summary>
        protected bool DefaultField()
        {
            return Bot.SpellZone[5] == null;
        }

        /// <summary>
        /// Turn if all enemy is better.
        /// </summary>
        protected bool DefaultMonsterRepos()
        {
            bool enemyBetter = AI.Utils.IsAllEnemyBetter(true);

            if (Card.IsAttack() && enemyBetter)
                return true;
            if (Card.IsDefense() && !enemyBetter && Card.Attack >= Card.Defense)
                return true;
            return false;
        }

        /// <summary>
        /// Chain enemy activation or summon.
        /// </summary>
        protected bool DefaultTrap()
        {
            return (LastChainPlayer == -1 && Duel.LastSummonPlayer != 0) || LastChainPlayer == 1;
        }

        /// <summary>
        /// Activate when avail and no other our trap card in this chain or face-up.
        /// </summary>
        protected bool DefaultUniqueTrap()
        {
            if (HasChainedTrap(0))
                return false;

            foreach (ClientCard card in Bot.SpellZone)
            {
                if (card != null &&
                    card.Id == Card.Id &&
                    card.HasPosition(CardPosition.FaceUp))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Dumb way to avoid the bot chain in mess.
        /// </summary>
        protected bool DefaultDontChainMyself()
        {
            foreach (CardExecutor exec in Executors)
            {
                if (exec.Type == Type && exec.CardId == Card.Id)
                    return false;
            }
            return LastChainPlayer != 0;
        }

        /// <summary>
        /// Draw when we have lower LP, or destroy it. Can be overrided.
        /// </summary>
        protected bool DefaultChickenGame()
        {
            int count = 0;
            foreach (CardExecutor exec in Executors)
            {
                if (exec.Type == Type && exec.CardId == Card.Id)
                    count++;
            }
            if (count > 1 || Duel.LifePoints[0] <= 1000)
                return false;
            if (Duel.LifePoints[0] <= Duel.LifePoints[1] && ActivateDescription == AI.Utils.GetStringId((int)CardId.ChickenGame, 0))
                return true;
            if (Duel.LifePoints[0] > Duel.LifePoints[1] && ActivateDescription == AI.Utils.GetStringId((int)CardId.ChickenGame, 1))
                return true;
            return false;
        }

        /// <summary>
        /// Clever enough.
        /// </summary>
        protected bool DefaultDimensionalBarrier()
        {
            const int RITUAL = 0;
            const int FUSION = 1;
            const int SYNCHRO = 2;
            const int XYZ = 3;
            const int PENDULUM = 4;
            if (Duel.Player != 0)
            {
                List<ClientCard> monsters = Enemy.GetMonsters();
                int[] levels = new int[13];
                bool tuner = false;
                bool nontuner = false;
                foreach (ClientCard monster in monsters)
                {
                    if (monster.HasType(CardType.Tuner))
                        tuner = true;
                    else if (!monster.HasType(CardType.Xyz))
                        nontuner = true;
                    if (monster.IsOneForXyz())
                    {
                        AI.SelectOption(XYZ);
                        return true;
                    }
                    levels[monster.Level] = levels[monster.Level] + 1;
                }
                if (tuner && nontuner)
                {
                    AI.SelectOption(SYNCHRO);
                    return true;
                }
                for (int i=1; i<=12; i++)
                {
                    if (levels[i]>1)
                    {
                        AI.SelectOption(XYZ);
                        return true;
                    }
                }
                ClientCard l = Enemy.SpellZone[6];
                ClientCard r = Enemy.SpellZone[7];
                if (l != null && r != null && l.LScale != r.RScale)
                {
                    AI.SelectOption(PENDULUM);
                    return true;
                }
            }
            ClientCard lastchaincard = GetLastChainCard();
            if (LastChainPlayer == 1 && lastchaincard != null && !lastchaincard.IsDisabled())
            {
                if (lastchaincard.HasType(CardType.Ritual))
                {
                    AI.SelectOption(RITUAL);
                    return true;
                }
                if (lastchaincard.HasType(CardType.Fusion))
                {
                    AI.SelectOption(FUSION);
                    return true;
                }
                if (lastchaincard.HasType(CardType.Synchro))
                {
                    AI.SelectOption(SYNCHRO);
                    return true;
                }
                if (lastchaincard.HasType(CardType.Xyz))
                {
                    AI.SelectOption(XYZ);
                    return true;
                }
                if (lastchaincard.IsFusionSpell())
                {
                    AI.SelectOption(FUSION);
                    return true;
                }
            }
            if (AI.Utils.IsChainTarget(Card))
            {
                AI.SelectOption(XYZ);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Clever enough.
        /// </summary>
        protected bool DefaultInterruptedKaijuSlumber()
        {
            if (Card.Location == CardLocation.Grave)
            {
                AI.SelectCard(new[]
                {
                    (int)CardId.GamecieltheSeaTurtleKaiju,
                    (int)CardId.KumongoustheStickyStringKaiju,
                    (int)CardId.RadiantheMultidimensionalKaiju,
                    (int)CardId.GadarlatheMysteryDustKaiju
                });
                return true;
            }
            AI.SelectCard(new[]
                {
                    (int)CardId.JizukirutheStarDestroyingKaiju,
                    (int)CardId.RadiantheMultidimensionalKaiju,
                    (int)CardId.GadarlatheMysteryDustKaiju,
                    (int)CardId.KumongoustheStickyStringKaiju
                });
            AI.SelectNextCard(new[]
                {
                    (int)CardId.GamecieltheSeaTurtleKaiju,
                    (int)CardId.KumongoustheStickyStringKaiju,
                    (int)CardId.GadarlatheMysteryDustKaiju,
                    (int)CardId.RadiantheMultidimensionalKaiju
                });
            return DefaultDarkHole();
        }

        /// <summary>
        /// Clever enough.
        /// </summary>
        protected bool DefaultKaijuSpsummon()
        {
            IList<int> kaijus = new[] {
                (int)CardId.JizukirutheStarDestroyingKaiju,
                (int)CardId.GadarlatheMysteryDustKaiju,
                (int)CardId.GamecieltheSeaTurtleKaiju,
                (int)CardId.RadiantheMultidimensionalKaiju,
                (int)CardId.KumongoustheStickyStringKaiju,
                (int)CardId.ThunderKingtheLightningstrikeKaiju,
                (int)CardId.DogorantheMadFlameKaiju,
                (int)CardId.SuperAntiKaijuWarMachineMechaDogoran
            };
            foreach (ClientCard monster in Enemy.GetMonsters())
            {
                if (kaijus.Contains(monster.Id))
                    return Card.GetDefensePower() > monster.GetDefensePower();
            }
            ClientCard card = Enemy.MonsterZone.GetFloodgate();
            if (card != null)
            {
                AI.SelectCard(card);
                return true;
            }
            card = Enemy.MonsterZone.GetDangerousMonster();
            if (card != null)
            {
                AI.SelectCard(card);
                return true;
            }
            card = AI.Utils.GetOneEnemyBetterThanValue(Card.GetDefensePower());
            if (card != null)
            {
                AI.SelectCard(card);
                return true;
            }
            return false;
        }

        protected bool DefaultNumberS39UtopiaTheLightningSummon()
        {
            int bestBotAttack = AI.Utils.GetBestAttack(Bot);
            return AI.Utils.IsOneEnemyBetterThanValue(bestBotAttack, false);
        }

        protected bool DefaultEvilswarmExcitonKnightSummon()
        {
            int selfCount = Bot.GetMonsterCount() + Bot.GetSpellCount() + Bot.GetHandCount();
            int oppoCount = Enemy.GetMonsterCount() + Enemy.GetSpellCount() + Enemy.GetHandCount();
            return (selfCount - 1 < oppoCount) && DefaultEvilswarmExcitonKnightEffect();
        }

        protected bool DefaultEvilswarmExcitonKnightEffect()
        {
            int selfCount = Bot.GetMonsterCount() + Bot.GetSpellCount();
            int oppoCount = Enemy.GetMonsterCount() + Enemy.GetSpellCount();

            int selfAttack = 0;
            List<ClientCard> monsters = Bot.GetMonsters();
            foreach (ClientCard monster in monsters)
            {
                selfAttack += monster.GetDefensePower();
            }

            int oppoAttack = 0;
            monsters = Enemy.GetMonsters();
            foreach (ClientCard monster in monsters)
            {
                oppoAttack += monster.GetDefensePower();
            }

            return (selfCount < oppoCount) || (selfAttack < oppoAttack);
        }

        protected bool DefaultStardustDragonSummon()
        {
            int selfBestAttack = AI.Utils.GetBestAttack(Bot);
            int oppoBestAttack = AI.Utils.GetBestPower(Enemy);
            return (selfBestAttack <= oppoBestAttack && oppoBestAttack <= 2500) || AI.Utils.IsTurn1OrMain2();
        }

        protected bool DefaultStardustDragonEffect()
        {
            return (Card.Location == CardLocation.Grave) || LastChainPlayer == 1;
        }

        protected bool DefaultCastelTheSkyblasterMusketeerSummon()
        {
            return AI.Utils.GetProblematicEnemyCard() != null;
        }

        protected bool DefaultCastelTheSkyblasterMusketeerEffect()
        {
            if (ActivateDescription == AI.Utils.GetStringId((int)CardId.CastelTheSkyblasterMusketeer, 0))
                return false;
            ClientCard target = AI.Utils.GetProblematicEnemyCard();
            if (target != null)
            {
                AI.SelectNextCard(target);
                return true;
            }
            return false;
        }

        protected bool DefaultScarlightRedDragonArchfiendSummon()
        {
            int selfBestAttack = AI.Utils.GetBestAttack(Bot);
            int oppoBestAttack = AI.Utils.GetBestPower(Enemy);
            return (selfBestAttack <= oppoBestAttack && oppoBestAttack <= 3000) || DefaultScarlightRedDragonArchfiendEffect();
        }

        protected bool DefaultScarlightRedDragonArchfiendEffect()
        {
            int selfCount = 0;
            List<ClientCard> monsters = Bot.GetMonsters();
            foreach (ClientCard monster in monsters)
            {
                // The bot don't know if the card is special summoned, so let we assume all monsters are special summoned
                if (!monster.Equals(Card) && monster.HasType(CardType.Effect) && monster.Attack <= Card.Attack)
                    selfCount++;
            }

            int oppoCount = 0;
            monsters = Enemy.GetMonsters();
            foreach (ClientCard monster in monsters)
            {
                if (monster.HasType(CardType.Effect) && monster.Attack <= Card.Attack)
                    oppoCount++;
            }

            return (oppoCount > 0 && selfCount <= oppoCount) || oppoCount > 2;
        }

    }
}
