using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace компилятор_2
{
    internal class LexicalAnalyzer
    {
        public List<(int Code, string Type, string Lexeme, int StartPos, int EndPos, Color HighlightColor)> AnalyzeText(string input)
        {
            var results = new List<(int, string, string, int, int, Color)>();
            int i = 0;
            Color defaultColor = Color.Black;
            Color errorColor = Color.Pink;

            while (i < input.Length && char.IsWhiteSpace(input[i])) i++;

            int start = i;
            bool errorInId = false;

            char[] invalidChars = { '@', '№', '#', '$', '%', '^', '&', '*', '!', '"', '\'', '<', '>', '/', '\\', '|', '.', '?', '~',
                         'А', 'Б', 'В', 'Г', 'Д', 'Е', 'Ё', 'Ж', 'З', 'И', 'Й', 'К', 'Л', 'М', 'Н', 'О', 'П', 'Р', 'С',
                         'Т', 'У', 'Ф', 'Х', 'Ц', 'Ч', 'Ш', 'Щ', 'Ы', 'Э', 'Ю', 'Я', 'а', 'б', 'в', 'г', 'д', 'е', 'ё',
                         'ж', 'з', 'и', 'й', 'к', 'л', 'м', 'н', 'о', 'п', 'р', 'с', 'т', 'у', 'ф', 'х', 'ц', 'ч', 'ш',
                         'щ', 'ы', 'э', 'ю', 'я' };

            while (i < input.Length && input[i] != '=')
            {
                char c = input[i];

                if (Array.Exists(invalidChars, element => element == c))
                {
                    if (i > start)
                    {
                        results.Add((2, "Идентификатор", input.Substring(start, i - start), start + 1, i, defaultColor));
                    }
                    results.Add((-1, $"Ошибка (недопустимый символ '{c}')", c.ToString(), i + 1, i + 1, errorColor));
                    i++;
                    while (i < input.Length && input[i] != '=') i++;
                    errorInId = true;
                    break;
                }

                if (!char.IsLetterOrDigit(c) && c != '_')
                {
                    if (i == start)
                    {
                        results.Add((-1, $"Ошибка (недопустимый символ '{c}')", c.ToString(), i + 1, i + 1, errorColor));
                        i++;
                        while (i < input.Length && input[i] != '=') i++;
                        break;
                    }
                    else
                    {
                        results.Add((2, "Идентификатор", input.Substring(start, i - start), start + 1, i, defaultColor));
                        results.Add((-1, $"Ошибка (недопустимый символ '{c}')", c.ToString(), i + 1, i + 1, errorColor));
                        i++;
                        while (i < input.Length && input[i] != '=') i++;
                        errorInId = true;
                        break;
                    }
                }
                i++;
            }

            if (!errorInId && start != i)
            {
                results.Add((2, "Идентификатор", input.Substring(start, i - start), start + 1, i, defaultColor));
            }

            while (i < input.Length && char.IsWhiteSpace(input[i])) i++;

            if (i < input.Length && input[i] == '=')
            {
                results.Add((10, "Оператор присваивания", "=", i + 1, i + 1, defaultColor));
                i++;
            }

            while (i < input.Length && char.IsWhiteSpace(input[i])) i++;

            while (i < input.Length)
            {
                while (i < input.Length && char.IsWhiteSpace(input[i])) i++; 

                if (i >= input.Length) break;

                char c = input[i];

                if (c == ',')
                {
                    results.Add((15, "Оператор запятая", ",", i + 1, i + 1, defaultColor));
                    i++;
                    continue;
                }
                if (c == '{')
                {
                    results.Add((13, "Фигурная скобка открывающая", "{", i + 1, i + 1, defaultColor));
                    i++;
                    continue;
                }
                
                if (c == '}')
                {
                    results.Add((14, "Фигурная скобка закрывающая", "}", i + 1, i + 1, defaultColor));
                    i++;
                    continue;
                }
                if (c == ':')
                {
                    results.Add((11, "Оператор двоеточие", ":", i + 1, i + 1, defaultColor));
                    i++;
                    continue;
                }

                if (c == ';')
                {
                    results.Add((16, "Оператор конца", ";", i + 1, i + 1, defaultColor));
                    i++;
                    continue;
                }
                if (c == '\'' || c == '"')
                {
                    char quote = c;
                    int j = i + 1;
                    while (j < input.Length && input[j] != quote) j++;
                    if (j < input.Length)
                    {
                        string str = input.Substring(i, j - i + 1);
                        results.Add((3, "Строка", str, i + 1, j + 1, defaultColor));
                        i = j + 1;
                    }
                    else
                    {
                        results.Add((-1, "Ошибка (незакрытая строка)", input.Substring(i), i + 1, input.Length, errorColor));
                        break;
                    }
                }
                else if (char.IsDigit(c))
                {
                    int startKey = i;
                    string key = "";  
                    bool keyError = false; 

                    while (i < input.Length)
                    {
                        char ch = input[i];

                        if (Array.Exists(invalidChars, element => element == ch) || ch == '@')
                        {
                            if (key.Length > 0)
                            {
                                results.Add((1, "Ключ", key, startKey + 1, i, defaultColor));
                            }

                            results.Add((-1, $"Ошибка (недопустимый символ '{ch}' в ключе)", ch.ToString(), i + 1, i + 1, errorColor));

                            i++;  

                            if (i < input.Length && input[i] == ':')
                            {
                                results.Add((11, "Оператор двоеточие", ":", i + 1, i + 1, defaultColor));
                                i++;  
                            }

                            keyError = true;  
                            break;  
                        }
                        if (char.IsDigit(ch) || char.IsLetter(ch) || ch == '_')
                        {
                            key += ch;  
                            i++;  
                        }
                        else
                        {
                            break; 
                        }
                    }

                    if (!keyError && key.Length > 0)
                    {
                        results.Add((1, "Ключ", key, startKey + 1, i, defaultColor));
                    }

                    if (keyError)
                    {
                        while (i < input.Length && (char.IsDigit(input[i]) || char.IsLetter(input[i]) || input[i] == '_'))
                        {
                            i++;
                        }

                        continue; 
                    }
                }

                else
                {
                    results.Add((-1, $"Ошибка (недопустимый символ '{c}')", c.ToString(), i + 1, i + 1, errorColor));
                    i++;
                }
            }

            return results;
        }
    }
}
