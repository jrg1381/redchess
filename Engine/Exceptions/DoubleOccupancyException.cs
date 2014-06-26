using System;

namespace Redchess.Engine.Exceptions
{
    /// <summary>
    ///     Thrown when a piece attempts to occupy a square already occupied by another piece of the same colour.
    ///     In general this won't happen because of move validity checking, so this exception indicates a bug.
    /// </summary>
    [Serializable]
    internal sealed class DoubleOccupancyException : Exception
    {
        internal DoubleOccupancyException(string message)
            : base(message)
        {
        }
    }
}