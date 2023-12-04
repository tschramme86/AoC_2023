using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AoC2023.Days.Day04
{
    internal class Day04Challenge : AoCChallengeBase
    {
        class ScratchCard
        {
            public int CardId { get; set; }
            public required int[] WinningNumbers { init; get; }
            public required int[] MyNumbers { init; get; }
            public required int CardValue { init; get; }
        }

        override public int Day => 4;
        override public string Name => "Scratchcards";
        
        protected override object? ExpectedTestResultPartOne => 13;
        protected override object? ExpectedTestResultPartTwo => 30;
        
        protected override object SolvePartOneInternal(string[] inputData)
        {
            var cardSum = 0;
            foreach (var cardLine in inputData)
            {
                var card = this.ParseCard(cardLine);
                var cardWinningSum = card.CardValue == 0 ? 0 : (int)Math.Pow(2, card.CardValue - 1);

                cardSum += cardWinningSum;
            }

            return cardSum;
        }

        protected override object SolvePartTwoInternal(string[] inputData)
        {
            var cardList = inputData.Select(this.ParseCard).ToDictionary(c => c.CardId);
            var cardCount = new int[cardList.Count];
            Array.Fill(cardCount, 1);

            for(var c=0; c<cardCount.Length; c++)
            {
                for(var cn=c+1; cn <= c + cardList[c+1].CardValue; cn++)
                {
                    if(cn >= cardCount.Length) break;
                    cardCount[cn] += cardCount[c];
                }
            }

            return cardCount.Sum();
        }

        private static readonly Regex RxCardId = new Regex(@"^Card\s+(?<cardId>\d+):", RegexOptions.Compiled);

        private ScratchCard ParseCard(string card)
        {
            var matchCardId = RxCardId.Match(card);
            Debug.Assert(matchCardId.Success);

            var cardId = int.Parse(matchCardId.Groups["cardId"].Value);
            var cardAllNumbers = card.Split(':')[1];
            var cardWinningNumbers = cardAllNumbers.Split('|')[0].Split(' ')
                .Where(n => !string.IsNullOrEmpty(n.Trim())).Select(n => int.Parse(n.Trim()));
            var cardMyNumbers = cardAllNumbers.Split('|')[1].Split(' ')
                .Where(n => !string.IsNullOrEmpty(n.Trim())).Select(n => int.Parse(n.Trim()));

            return new ScratchCard
            {
                CardId = cardId,
                WinningNumbers = cardWinningNumbers.ToArray(),
                MyNumbers = cardMyNumbers.ToArray(),
                CardValue = cardMyNumbers.Count(n => cardWinningNumbers.Contains(n))
            };
        }
    }
}
