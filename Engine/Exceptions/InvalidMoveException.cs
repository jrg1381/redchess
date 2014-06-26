using System;

namespace Redchess.Engine.Exceptions
{
    [Serializable]
    internal sealed class InvalidMoveException : Exception
	{
		internal InvalidMoveException(string message) : base(message)
		{
		}
	}
}
