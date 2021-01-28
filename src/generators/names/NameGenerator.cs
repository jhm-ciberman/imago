using System.Text;
using System;
using LifeSim.Generation;

namespace LifeSim
{
    public class NameGenerator
    {
        public enum Gender
        {
            Other,
            Male,
            Female,
        }

        private readonly Random _random;
        private readonly RandomGauss _randomGauss;  

        private readonly CharGenerator _male;
        private readonly CharGenerator _female;
        private readonly CharGenerator _other;

        private readonly EnglishAlphabetProvider _provider;

        public NameGenerator(System.Random random, EnglishAlphabetProvider provider)
        {
            this._random = random;
            this._randomGauss = new RandomGauss(random);

            this._provider = provider;
            this._male   = new CharGenerator(provider.GetMaleNameLetters(random));
            this._female = new CharGenerator(provider.GetFemaleNameLetters(random));
            this._other  = new CharGenerator(provider.GetOtherNameLetters(random));
        }

        public string MakeName(Gender gender = Gender.Other, float legibility = 0.8f)
        {
            int tries = 0;
            StringBuilder str;
            string name;
            do
            {
                str = new StringBuilder();
                int numberOfLetters = this._GetNumberOfLetters(7);
                this._CompleteName(str, this._GetGenerator(gender), numberOfLetters, legibility);
                name = str.ToString();
                tries++;
            } while (this._CheckErrorPercentage(name) > 1 - legibility && tries < 5);

            return name;
        }

        private int _GetNumberOfLetters(int max)
        {
            return System.Math.Max(3, (int) this._randomGauss.NextGaussian(2, max));
        }

        public string MakeSurname(float legibility = 0.8f)
        {
            StringBuilder str;
            int tries = 0;
            do {
                string word = this._GetRandomWord();

                int numberOfLetters = Math.Min(0, this._GetNumberOfLetters(10) - word.Length);

                bool wordIsAtTheEnd = (this._random.NextDouble() > 0.8f); // More probability of the word at the start
                str = new StringBuilder();
                if (! wordIsAtTheEnd) str.Append(word);
                this._CompleteName(str, this._other, numberOfLetters, legibility);
                if (wordIsAtTheEnd) str.Append(word);

                tries++;

            } while (this._CheckErrorPercentage(str.ToString()) > 1 - legibility && tries < 5);

            return this._Capitalize(str.ToString());
        }

        private string _GetRandomWord()
        {
            return this._provider.syllabes[(int) (this._random.NextDouble() * this._provider.syllabes.Length)];
        }

        private string _Capitalize(string str)
        {
            return char.ToUpper(str[0]) + str.Substring(1).ToLower();
        }

        public string MakeFullName(Gender gender = Gender.Other, float legibility = 0.8f)
        {
            return this.MakeName(gender, legibility) + " " + this.MakeSurname(legibility);
        }

        private CharGenerator _GetGenerator(Gender gender)
        {
            switch(gender)
            {
                case Gender.Female: return this._female;
                case Gender.Male: return this._male;
                default: return this._other;
            }
        }

        private float _CheckErrorPercentage(string name)
        {
            if (name.Length < 2) return 1f; // 100% bad

            int faultsCount = 0;
            bool wasBowel = CharGenerator.IsBowelLike(name[1]);
            bool wasHardToPronounce = CharGenerator.IsHardToPronounce(name[1]);

            if (Char.ToLower(name[0]) == Char.ToLower(name[1])) // penalize names starting with same letter (e.g.: Aade, Eeulo, Iiop)
            {
                faultsCount += 3;
            } 
            else if (wasHardToPronounce  == CharGenerator.IsHardToPronounce(name[0])) // Penalize names with the first two consecutive letters hard to pronounce
            {
                faultsCount += 4;
            }

            int repetitionCount = 0;
            for (int i = 1; i < name.Length; i++)
            {
                bool isBowel = CharGenerator.IsBowelLike(name[i]);
                bool isHardToPronounce = CharGenerator.IsHardToPronounce(name[i]);
                repetitionCount = (isBowel == wasBowel) ? repetitionCount + 1 : 0;

                bool hasThreeBowels = (isBowel && repetitionCount >= 3);
                bool hasTwoConsonants = (! isBowel && repetitionCount >= 2);
                bool hasTwoHardToPronounceLetters = (wasHardToPronounce && isHardToPronounce);

                if (hasThreeBowels || hasTwoConsonants || hasTwoHardToPronounceLetters)
                {
                    faultsCount += 3;
                }

                if (name[i] != 'h') 
                {
                    wasBowel = isBowel;
                    wasHardToPronounce = isHardToPronounce;
                }
            }

            return ((float) faultsCount / (float) name.Length);
        }
        
        private void _CompleteName(StringBuilder str, CharGenerator generator, int numberOfLetters, float legibility = 0.8f)
        {
            char c;
            bool lastWasBowel;
            if (str.Length == 0)
            {
                c = generator.GetStartingChar();
                str.Append(System.Char.ToUpper(c));
                numberOfLetters -= 1;

                lastWasBowel = CharGenerator.IsBowel(c);

                if (!lastWasBowel && ! CharGenerator.IsHardToPronounce(c) && this._random.NextDouble() > 0.8f) 
                    lastWasBowel = !lastWasBowel;
            }
            else
            {
                c = str[str.Length - 1];
                lastWasBowel = CharGenerator.IsBowel(c);
            }

            lastWasBowel = !lastWasBowel;
            for (int i = 1; i < numberOfLetters; i++) // Exclude first and last letter from the loop
            {
                c = generator.GetMiddleChar(c, lastWasBowel);
                str.Append(c);
                if (c != 'h') lastWasBowel = CharGenerator.IsBowelLike(c);

                if (this._random.NextDouble() > 1 - legibility)
                    lastWasBowel = !lastWasBowel;
            }

            char endC = generator.GetEndingChar();
            
            lastWasBowel = CharGenerator.IsBowelLike(c);
            if (CharGenerator.IsBowelLike(endC) == lastWasBowel)
            {
                c = generator.GetMiddleChar(c, !lastWasBowel);
                if (c != endC) str.Append(c);
            }

            str.Append(endC);
        }

    }
}