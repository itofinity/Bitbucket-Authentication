using System.Collections.Generic;
using System.IO;

namespace Itofinity.Bitbucket.Authentication.Helpers.Microsoft.Git.CredentialManager
{
    public interface ITrace
    {
        /// <summary>
        /// Writes a message to the trace writer followed by a line terminator.
        /// </summary>
        /// <param name="message">The message to write.</param>
        /// <param name="filePath">Path of the file this method is called from.</param>
        /// <param name="lineNumber">Line number of file this method is called from.</param>
        /// <param name="memberName">Name of the member in which this method is called.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        void WriteLine(
            string message,
            [System.Runtime.CompilerServices.CallerFilePath] string filePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "");


        /// <summary>
        /// Write the contents of a dictionary that contains sensitive information to the trace writer.
        /// <para/>
        /// Calls <see cref="object.ToString"/> on all keys and values, except keys specified as secret.
        /// </summary>
        /// <param name="dictionary">The dictionary to write.</param>
        /// <param name="secretKeys">Dictionary keys that contain secrets/sensitive information.</param>
        /// <param name="keyComparer">Comparer to use for <paramref name="secretKeys"/>.</param>
        /// <param name="filePath">Path of the file this method is called from.</param>
        /// <param name="lineNumber">Line number of file this method is called from.</param>
        /// <param name="memberName">Name of the member in which this method is called.</param>
        void WriteDictionarySecrets<TKey, TValue>(
            IDictionary<TKey, TValue> dictionary,
            TKey[] secretKeys,
            IEqualityComparer<TKey> keyComparer = null,
            [System.Runtime.CompilerServices.CallerFilePath] string filePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "");
        void AddListener(TextWriter error);
    }
}