using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AoC2023.Days.Day07
{
    internal class Day07Challenge : AoCChallengeBase
    {
        enum Card
        {
            A, K, Q, J, T, C9, C8, C7, C6, C5, C4, C3, C2
        }

        enum HandType
        {
            HighCard, Pair, TwoPair, ThreeOfAKind, FullHouse, FourOfAKind, FiveOfAKind
        }

        class Hand
        {
            public HandType Type { get; set; }
            public Card[] Cards { get; } = new Card[5];
            public int Bid { get; set; }
            public int Rank { get; set; }
        }

        class PartOneHandComparer : IComparer<Hand>
        {
            public int Compare(Hand? x, Hand? y)
            {
                if (x == null)
                {
                    return 1;
                }
                if (y == null)
                {
                    return -1;
                }
                if (x.Type != y.Type)
                {
                    return -x.Type.CompareTo(y.Type);
                }
                else
                {
                    for (var i = 0; i < 5; i++)
                    {
                        if (x.Cards[i] != y.Cards[i])
                        {
                            return x.Cards[i].CompareTo(y.Cards[i]);
                        }
                    }
                }

                return 0;
            }
        }

        class PartTwoHandComparer : IComparer<Hand>
        {
            public int Compare(Hand? x, Hand? y)
            {
                var cardSort = new List<Card> { Card.A, Card.K, Card.Q, Card.T, Card.C9, Card.C8, Card.C7, Card.C6, Card.C5, Card.C4, Card.C3, Card.C2, Card.J };
                if (x == null)
                {
                    return 1;
                }
                if (y == null)
                {
                    return -1;
                }
                if (x.Type != y.Type)
                {
                    return -x.Type.CompareTo(y.Type);
                }
                else
                {
                    for (var i = 0; i < 5; i++)
                    {
                        if (x.Cards[i] != y.Cards[i])
                        {
                            return cardSort.IndexOf(x.Cards[i]).CompareTo(cardSort.IndexOf(y.Cards[i]));
                        }
                    }
                }

                return 0;
            }
        }

        public override int Day => 7;
        public override string Name => "Camel Cards";

        protected override object? ExpectedTestResultPartOne => 6440;
        protected override object? ExpectedTestResultPartTwo => 5905;

        protected override object SolvePartOneInternal(string[] inputData)
        {
            var hands = ReadHands(inputData, false);

            // order by rank
            var ordered = hands.ToList();
            ordered.Sort(new PartOneHandComparer());
            for (var i = 0; i < ordered.Count; i++)
            {
                ordered[i].Rank = ordered.Count - i;
            }

            var winning = hands.Sum(h => h.Rank * h.Bid);
            return winning;
        }

        protected override object SolvePartTwoInternal(string[] inputData)
        {
            var hands = ReadHands(inputData, true);

            // order by rank
            var ordered = hands.ToList();
            ordered.Sort(new PartTwoHandComparer());
            for (var i = 0; i < ordered.Count; i++)
            {
                ordered[i].Rank = ordered.Count - i;
            }

            var winning = hands.Sum(h => h.Rank * h.Bid);
            return winning;
        }

        private List<Hand> ReadHands(string[] inputData, bool withJokers)
        {
            var hands = new List<Hand>();
            foreach (var line in inputData)
            {
                var hand = new Hand();
                var cardsBid = line.Split(' ');
                for (int i = 0; i < 5; i++)
                {
                    var c = "" + cardsBid[0][i];
                    if (c[0] >= '0' && c[0] <= '9')
                    {
                        hand.Cards[i] = (Card)Enum.Parse(typeof(Card), "C" + c);
                    }
                    else
                    {
                        hand.Cards[i] = (Card)Enum.Parse(typeof(Card), c);
                    }
                }
                hand.Bid = int.Parse(cardsBid[1]);
                this.CalcHandType(hand, withJokers);
                hands.Add(hand);
            }
            return hands;
        }

        private void CalcHandType(Hand hand, bool withJokers)
        {
            var cardCounts = new Dictionary<Card, int>();
            foreach (var card in hand.Cards)
            {
                if (cardCounts.ContainsKey(card))
                {
                    cardCounts[card]++;
                }
                else
                {
                    cardCounts[card] = 1;
                }
            }

            var jokers = withJokers ? (cardCounts.TryGetValue(Card.J, out var jCount) ? jCount : 0) : 0;
            if (cardCounts.Count == 1 || (cardCounts.Count == 2 && jokers >= 1))
            {
                hand.Type = HandType.FiveOfAKind;
            }
            else if (
                (cardCounts.ContainsValue(4)) || 
                (cardCounts.ContainsValue(3) && jokers >= 1) || 
                (cardCounts.Count(c => c.Value == 2) >= 2 && jokers >= 2))
            {
                hand.Type = HandType.FourOfAKind;
            } 
            else if(cardCounts.Count == 2 
                || (cardCounts.Count == 3 && jokers >= 1))
            {
                hand.Type = HandType.FullHouse;
            }
            else if (
                (cardCounts.ContainsValue(3)) ||
                (cardCounts.ContainsValue(2) && jokers == 1) ||
                (jokers >= 2))
            {
                hand.Type = HandType.ThreeOfAKind;
            }
            else if(
                (cardCounts.Count(c => c.Value == 2) == 2) ||
                (cardCounts.Count(c => c.Value == 2) == 1 && jokers >= 1))
            {
                hand.Type = HandType.TwoPair;
            }
            else if (cardCounts.Count == 4 || jokers >= 1)
            {
                hand.Type = HandType.Pair;
            }
            else
            {
                hand.Type = HandType.HighCard;
            }
        }
    }
}
