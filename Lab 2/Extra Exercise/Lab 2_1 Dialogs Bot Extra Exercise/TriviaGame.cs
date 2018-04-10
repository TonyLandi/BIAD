using System;
using System.Collections.Generic;
using System.Linq;

namespace Lab_2_1_Dialogs_Bot_Extra_Exercise
{
    [Serializable]
    public class TriviaGame
    {
        private string _playersName;
        private int _currentQuestion = 0;
        private int[] _usersAnswers = new int[] { -1, -1, -1 };

        public List<TriviaQuestion> _questions = new List<TriviaQuestion>
        {
            new TriviaQuestion()
            {
                Index = 0,
                Answer = 3,
                Question = "How many pieces of contemporary art is in Microsoft's collection?",
                Choices = new string[] { "0", "1000", "3000", "5000", "10000"}
            },
            new TriviaQuestion()
            {
                Index = 1,
                Answer = 2,
                Question = "In 2016, Microsoft made a major breakthrough, equaling that of humans, in what?",
                Choices = new string[] {"Writing Song Lyrics", "Derby Car Racing", "Speech Recognition", "Predicting American Idol Winners"}
            },
            new TriviaQuestion()
            {
                Index = 2,
                Answer = 1,
                Question = "Approximately how much money does Microsoft spend on R&D?",
                Choices = new string[] {"$111 billion","$11 billion","$1 billion","$1 dollar"}
            }
        };

        public TriviaGame(string playersName)
        {
            _playersName = playersName;
        }

        public TriviaQuestion CurrentQuestion()
        {
            return _questions.Where(q => q.Index == _currentQuestion).FirstOrDefault();
        }

        public TriviaQuestion MoveToNextQuestion()
        {
            _currentQuestion++;
            if (_currentQuestion < _questions.Count())
            {
                return CurrentQuestion();
            }
            else
            {
                _currentQuestion--;
                return null;
            }
        }
        public TriviaQuestion MoveToPreviousQuestion()
        {
            _currentQuestion--;
            if (_currentQuestion > 0)
            {
                return CurrentQuestion();
            }
            else
            {
                _currentQuestion = 0;
                return null;
            }
        }
        public TriviaQuestion MoveToFirstQuestion()
        {
            _currentQuestion = 0;
            return CurrentQuestion();
        }
        public bool Answer(int answer)
        {
            _usersAnswers[_currentQuestion] = answer;
            return _usersAnswers[_currentQuestion] == _questions[_currentQuestion].Answer;
        }
        public int Score()
        {
            return _questions.Where(q => _usersAnswers[q.Index] == q.Answer).Count();
        }


    }

    [Serializable]
    public class TriviaQuestion
    {
        public int Index { get; set; }
        public int Answer { get; set; }
        public string Question { get; set; }
        public string[] Choices { get; set; }
    }
}