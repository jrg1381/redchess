using System;
using System.Collections.Generic;

namespace RedChess.ChessboardRenderer
{
    internal class FenReader
    {
        private static readonly HashSet<char> ValidCharacters = new HashSet<char>("/12345678RNBQKPrnbqkp");
        private readonly string m_Position;

        internal FenReader(string fen)
        {
            m_Position = fen.Split(' ')[0];
        }

        internal void CallForPieces(Action<string, int, int> callback)
        {
            var index = 0;

            foreach (var c in m_Position)
            {
                if(!ValidCharacters.Contains(c))
                    throw new ArgumentException("Invalid character in FEN");

                if (Char.IsDigit(c))
                {
                    index += c - '0'; // Faster than Int32.Parse 
                    continue;
                }

                if (c == '/')
                {
                    continue;
                }

                callback(c.ToString(), index % 8, index / 8);
                index++;
            }
        }
    }
}