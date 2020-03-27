using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Itofinity.Bitbucket.Authentication.Helpers.Microsoft.Git.CredentialManager
{
    public class Trace : DisposableObject, ITrace
    {
        private const string SecretMask = "********";

        private readonly object _writersLock = new object();
        private readonly List<TextWriter> _writers = new List<TextWriter>();

        public bool HasListeners
        {
            get
            {
                lock (_writersLock)
                {
                    return _writers.Any();
                }
            }
        }

        public bool IsSecretTracingEnabled { get; set; }

        public void AddListener(TextWriter listener)
        {
            ThrowIfDisposed();

            lock (_writersLock)
            {
                // Try not to add the same listener more than once
                if (_writers.Contains(listener))
                    return;

                _writers.Add(listener);
            }
        }

        public void Flush()
        {
            ThrowIfDisposed();

            lock (_writersLock)
            {
                foreach (var writer in _writers)
                {
                    try
                    {
                        writer?.Flush();
                    }
                    catch
                    { /* squelch */ }
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public void WriteException(
            Exception exception,
            [System.Runtime.CompilerServices.CallerFilePath] string filePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            // Exception being null probably won't happen, but we shouldn't die because we failed to trace it.
            if (exception is null)
                return;

            WriteLine($"! error: '{exception.Message}'.", filePath, lineNumber, memberName);

            while ((exception = exception.InnerException) != null)
            {
                WriteLine($"       > '{exception.Message}'.", filePath, lineNumber, memberName);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public void WriteDictionary<TKey, TValue>(
            IDictionary<TKey, TValue> dictionary,
            [System.Runtime.CompilerServices.CallerFilePath] string filePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            foreach (KeyValuePair<TKey, TValue> entry in dictionary)
            {
                WriteLine($"\t{entry.Key}={entry.Value}", filePath, lineNumber, memberName);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public void WriteDictionarySecrets<TKey, TValue>(
            IDictionary<TKey, TValue> dictionary,
            TKey[] secretKeys,
            IEqualityComparer<TKey> keyComparer = null,
            [System.Runtime.CompilerServices.CallerFilePath] string filePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            foreach (KeyValuePair<TKey, TValue> entry in dictionary)
            {
                bool isSecretEntry = !(secretKeys is null) &&
                                     secretKeys.Contains(entry.Key, keyComparer ?? EqualityComparer<TKey>.Default);
                if (isSecretEntry && !this.IsSecretTracingEnabled)
                {
                    WriteLine($"\t{entry.Key}={SecretMask}", filePath, lineNumber, memberName);
                }
                else
                {
                    WriteLine($"\t{entry.Key}={entry.Value}", filePath, lineNumber, memberName);
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public void WriteLine(
            string message,
            [System.Runtime.CompilerServices.CallerFilePath] string filePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            ThrowIfDisposed();

            lock (_writersLock)
            {
                if (_writers.Count == 0)
                {
                    return;
                }

                string text = FormatText(message, filePath, lineNumber, memberName);

                foreach (var writer in _writers)
                {
                    try
                    {
                        writer?.Write(text);
                        writer?.Write('\n');
                        writer?.Flush();
                    }
                    catch { /* squelch */ }
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public void WriteLineSecrets(
            string format,
            object[] secrets,
            [System.Runtime.CompilerServices.CallerFilePath] string filePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            string message = this.IsSecretTracingEnabled
                           ? string.Format(format, secrets)
                           : string.Format(format, secrets.Select(_ => (object)SecretMask).ToArray());

            WriteLine(message, filePath, lineNumber, memberName);
        }

        protected override void ReleaseManagedResources()
        {
            lock (_writersLock)
            {
                try
                {
                    for (int i = 0; i < _writers.Count; i += 1)
                    {
                        using (var writer = _writers[i])
                        {
                            _writers.Remove(writer);
                        }
                    }
                }
                catch
                { /* squelch */ }
            }

            base.ReleaseManagedResources();
        }

        private static string FormatText(string message, string filePath, int lineNumber, string memberName)
        {
            const int sourceColumnMaxWidth = 23;

            EnsureArgument.NotNull(message, nameof(message));
            EnsureArgument.NotNull(filePath, nameof(filePath));
            EnsureArgument.PositiveOrZero(lineNumber, nameof(lineNumber));
            EnsureArgument.NotNull(memberName, nameof(memberName));

            // Source column format is file:line
            string source = $"{filePath}:{lineNumber}";

            if (source.Length > sourceColumnMaxWidth)
            {
                int idx = 0;
                int maxlen = sourceColumnMaxWidth - 3;
                int srclen = source.Length;

                while (idx >= 0 && (srclen - idx) > maxlen)
                {
                    idx = source.IndexOf('\\', idx + 1);
                }

                // If we cannot find a path separator which allows the path to be long enough, just truncate the file name
                if (idx < 0)
                {
                    idx = srclen - maxlen;
                }

                source = "..." + source.Substring(idx);
            }

            // Git's trace format is "{timestamp,-15} {source,-23} trace: {details}"
            string text = $"{DateTime.Now:HH:mm:ss.ffffff} {source,-23} trace: [{memberName}] {message}";

            return text;
        }
    }
}