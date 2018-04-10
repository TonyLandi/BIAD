using System;
using System.Collections.Generic;

namespace Lab_2_1_Dialogs_Bot_Extra_Exercise
{
    [Serializable]
    public class KnockKnockJokes
    {
        private static Random rnd = new Random();

        public static List<KnockKnockJoke> Jokes = new List<KnockKnockJoke>()
        {
            new KnockKnockJoke()
            {
                FirstName = "Yah",
                PunchLine = "I’m sorry I don’t understand what you meant by Yahoo.  Did you mean Bing?"
            },
            new KnockKnockJoke()
            {
                FirstName = "Orange",
                PunchLine = "Orange you gonna let me in?"
            },
            new KnockKnockJoke()
            {
                FirstName = "Iva",
                PunchLine = "Iva sore hand from knocking!"
            },
            new KnockKnockJoke()
            {
                FirstName = "Dozen",
                PunchLine = "Dozen anybody wanna let me in?"
            },
            new KnockKnockJoke()
            {
                FirstName = "Harry",
                PunchLine = "Harry up it's cold out here!"
            }
        };

        public static KnockKnockJoke GetRandomJoke()
        {
            int jokeIndex = rnd.Next(5);
            return Jokes[jokeIndex];
        }
    }
    [Serializable]
    public class KnockKnockJoke
    {
        public string Introduction = "Knock! Knock!";
        public string FirstName { get; set; }
        public string PunchLine { get; set; }
    }
}