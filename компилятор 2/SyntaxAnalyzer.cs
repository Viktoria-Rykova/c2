using System;
using System.Collections.Generic;
using System.Linq;

namespace компилятор_2
{
    internal class SyntaxAnalyzer
    {
        private string input;
        private int position;
        public List<(string Problem, int Position)> Errors = new List<(string, int)>();

        public SyntaxAnalyzer(string inputText)
        {
            input = inputText;
            position = 0;
        }

        public bool Parse()
        {
            Errors.Clear();
            bool success = Start();
            return success && position == input.Length;
        }

        private void SkipWhitespace()
        {
            while (position < input.Length && char.IsWhiteSpace(input[position]))
                position++;
        }

        private bool Match(char expected)
        {
            if (position < input.Length && input[position] == expected)
            {
                position++;
                return true;
            }
            return false;
        }

        private bool IsLetter(char c) => char.IsLetter(c);
        private bool IsDigit(char c) => char.IsDigit(c);
        private bool IsSymbol(char c)
        {
            const string extra = "!.,?№#^@$;: %&*()-=+/<>\'\"~[]{}\\|_";
            return IsLetter(c) || IsDigit(c) || extra.Contains(c);
        }

        private bool Start()
        {
            if (position >= input.Length || !IsLetter(input[position]))
            {
                Errors.Add(("Ожидалась буква в начале идентификатора", position + 1));
                return false;
            }

            position++;

            while (position < input.Length && (IsLetter(input[position]) || IsDigit(input[position])))
            {
                position++;
            }

            SkipWhitespace();

            if (position >= input.Length || input[position] != '=')
            {
                Errors.Add(("Ожидался знак '=' после идентификатора", position + 1));
                return false;
            }

            position++; // Съели первый '='

            // Проверка: есть ли лишний '=' сразу после первого
            if (position < input.Length && input[position] == '=')
            {
                Errors.Add(("Лишний знак '='", position + 1));
                return false;
            }

            return Num();
        }



        private bool IdRem()
        {
            while (position < input.Length)
            {
                if (IsLetter(input[position]) || IsDigit(input[position]))
                {
                    position++;
                }
                else if (input[position] == '=')
                {
                    position++;
                    return Num();  // Перешли к проверке после '='
                }
                else
                {
                    Errors.Add(("Ожидался знак '='", position + 1)); // Ошибка, если не найдено '='
                    return false;
                }
            }

            Errors.Add(("Ожидался знак '='", position + 1));
            return false;
        }

        private bool Num()
        {
            SkipWhitespace();
            // Проверка на '{'
            if (!Match('{'))
            {
                Errors.Add(("Ожидалась '{'", position + 1));
                return false;
            }

            if (!OpenBrace()) return false;

            return true;
        }

        private bool OpenBrace()
        {
            if (!Pairs()) return false;
            return MorePairs();
        }


        private bool MorePairs()
        {
            SkipWhitespace();

            if (position >= input.Length)
            {
                Errors.Add(("Ожидалась '}'", position + 1));
                return false;
            }

            if (input[position] == ',')
            {
                position++;
                return Pairs() && MorePairs();
            }
            else if (input[position] == '}')
            {
                position++;

                // Проверка на лишнюю закрывающую скобку
                SkipWhitespace();
                if (position < input.Length && input[position] == '}')
                {
                    Errors.Add(("Лишняя закрывающая '}'", position + 1));
                    return false;
                }

                return End();
            }
            else
            {
                Errors.Add(("Ожидалась ',' или '}'", position + 1));
                return false;
            }
        }


        private bool Pairs()
        {
            if (!Key()) return false;
            SkipWhitespace();

            // Проверка на дублирование двоеточия
            if (position < input.Length && input[position] == ':' && position + 1 < input.Length && input[position + 1] == ':')
            {
                Errors.Add(("Неверное использование двух двоеточий '::'", position + 1));
                return false;
            }

            if (!Match(':'))
            {
                Errors.Add(("Ожидалось ':'", position + 1));
                return false;
            }
            return Value();
        }


        private bool Key()
        {
            SkipWhitespace();
            if (!IsDigit(input[position]))
            {
                Errors.Add(("Ожидалась цифра в ключе", position + 1));
                return false;
            }

            while (position < input.Length && IsDigit(input[position]))
                position++;

            return true;
        }

        private bool Value()
        {
            SkipWhitespace();
            if (!Match('\''))
            {
                Errors.Add(("Ожидалась открывающая кавычка", position + 1));
                return false;
            }

            if (!Str()) return false;
            SkipWhitespace();
            if (!Match('\''))
            {
                Errors.Add(("Ожидалась закрывающая кавычка", position + 1));
                return false;
            }

            return true;
        }

        private bool Str()
        {
            while (position < input.Length && IsSymbol(input[position]))
            {
                position++;
                if (position < input.Length && input[position] == '\'') break;
            }

            return true;
        }

        private bool End()
        {
            SkipWhitespace();
            if (!Match(';'))
            {
                Errors.Add(("Ожидалась ';'", position + 1));
                return false;
            }
            return true;
        }
    }
}
