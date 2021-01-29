using System;
using System.Collections.Generic;

namespace LifeSim.Generation
{
    public class CharGenerator
    {
        public class MonogramProvider
        {
            public WeightedRandom<char> startingLetters;  
            public WeightedRandom<char> middleLetters;
            public WeightedRandom<char> endingLetters;

            public MonogramProvider(System.Random random, Monogram[] monograms)
            {
                this.startingLetters = new WeightedRandom<char>(random);    
                this.middleLetters   = new WeightedRandom<char>(random);
                this.endingLetters   = new WeightedRandom<char>(random);

                foreach (var monogram in monograms)
                {
                    char c = Char.ToLower(monogram.character);
                    this.startingLetters.Add(c, monogram.startFrecuency);
                    this.middleLetters.Add(c, monogram.middleFrecuency);
                    this.endingLetters.Add(c, monogram.endFrecuency);
                }
            }
        }

        public struct Monogram
        {
            public char character;
            public float startFrecuency;
            public float middleFrecuency;
            public float endFrecuency;

            public Monogram(char character, float frecuencyStart, float frecuencyMiddle, float frecuencyEnd)
            {
                this.character = character;
                this.startFrecuency = frecuencyStart;
                this.middleFrecuency = frecuencyMiddle;
                this.endFrecuency = frecuencyEnd;
            }
        }

        private readonly MonogramProvider _monograms;

        public CharGenerator(MonogramProvider monograms)
        {
            this._monograms = monograms;
        }

        public char GetMiddleChar(char prevChar, bool preferBowel)
        {
            prevChar = Char.ToLower(prevChar);
            char c;
            int tries = 0;
            bool isValid;
            do 
            {
                c = this._monograms.middleLetters.Next();
                isValid = (CharGenerator.IsBowelLike(c) == preferBowel && c != prevChar);
                tries++;
            } while (!isValid && tries < 20);

            return c;
        }

        public char GetStartingChar()
        {
            return this._monograms.startingLetters.Next();
        }

        public char GetEndingChar()
        {
            return this._monograms.endingLetters.Next();
        }

        public static bool IsBowel(char c)
        {
            c = System.Char.ToLower(c);
            return (c == 'a' || c == 'e' || c == 'i' || c == 'o' || c == 'u');
        }

        public static bool IsBowelLike(char c)
        {
            c = System.Char.ToLower(c);
            return (c == 'a' || c == 'e' || c == 'i' || c == 'o' || c == 'u' || c == 'y' || c == 'h');
        }

        public static bool IsHardToPronounce(char c)
        {
            c = System.Char.ToLower(c);
            return (c == 'q' || c == 'w' || c == 'j' || c == 'z' || c == 'x' || c == 'k' || c == 'l' || c == 's' 
                 || c == 'p' || c == 't' || c == 'f' || c == 'h' || c == 'n' || c == 'v' || c == 'm' || c == 'b' );
        }
    }
}