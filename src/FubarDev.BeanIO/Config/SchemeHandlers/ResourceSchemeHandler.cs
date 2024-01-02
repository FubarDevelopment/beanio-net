// <copyright file="ResourceSchemeHandler.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Reflection;

namespace BeanIO.Config.SchemeHandlers
{
    /// <summary>
    /// Handles loading from a <c>resource:</c> URI.
    /// </summary>
    public class ResourceSchemeHandler : ISchemeHandler
    {
        /// <summary>
        /// Gets the schema this handler supports (e.g. file).
        /// </summary>
        public string Schema => "resource";

        /// <summary>
        /// This functions opens a stream for the given <paramref name="resource"/> <see cref="Uri"/>.
        /// </summary>
        /// <param name="resource">The resource to load the mapping from.</param>
        /// <returns>the stream to read the mapping from.</returns>
        public System.IO.Stream? Open(Uri resource)
        {
            if (resource.Scheme != Schema)
                throw new ArgumentOutOfRangeException($"Only '{Schema}' URLs are allowed");

            var resName = resource.LocalPath;
            var commaIndex = resName.IndexOf(',');
            if (commaIndex == -1)
                throw new BeanIOConfigurationException($"No assembly specified for resource name {resName}");

            var resAsmName = resName.Substring(commaIndex + 1).Trim();
            resName = resName.Substring(0, commaIndex).TrimEnd();
            var asmName = new AssemblyName(resAsmName);
            var resAssembly = Assembly.Load(asmName);
            var resStream = resAssembly.GetManifestResourceStream(resName);
            if (resStream == null)
            {
                // Partial workaround for .NET Core bug: https://github.com/dotnet/cli/issues/3247
                var commonLength = LongestCommonSubstring(asmName.Name, resName, out var commonSequence);
                if (commonLength > 1)
                {
                    var asmNameName = asmName.Name ??
                                      throw new InvalidOperationException($"Assembly has no name {asmName}");
                    commonSequence = commonSequence.TrimEnd('.');
                    var asmNameEndsWithResName =
                        asmNameName.EndsWith($".{commonSequence}", StringComparison.OrdinalIgnoreCase)
                        || asmNameName.EndsWith($".{commonSequence}.Test", StringComparison.OrdinalIgnoreCase)
                        || asmNameName.EndsWith($".{commonSequence}.Tests", StringComparison.OrdinalIgnoreCase);
                    if (asmNameEndsWithResName && resName.StartsWith($"{commonSequence}.", StringComparison.OrdinalIgnoreCase))
                    {
                        var resNameWithoutNamespace = resName.Substring(commonSequence.Length + 1);
                        resName = $"{asmNameName}.{resNameWithoutNamespace}";
                        resStream = resAssembly.GetManifestResourceStream(resName);
                    }
                }
            }

            return resStream;
        }

        private static int LongestCommonSubstring(string? str1, string? str2, out string sequence)
        {
            sequence = string.Empty;
            if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2))
                return 0;

            var num = new int[str1!.Length, str2!.Length];
            int maxlen = 0;
            int lastSubsBegin = 0;
            var sequenceBuilder = new System.Text.StringBuilder();

            for (int i = 0; i < str1.Length; i++)
            {
                for (int j = 0; j < str2.Length; j++)
                {
                    if (str1[i] != str2[j])
                    {
                        num[i, j] = 0;
                    }
                    else
                    {
                        if ((i == 0) || (j == 0))
                        {
                            num[i, j] = 1;
                        }
                        else
                        {
                            num[i, j] = 1 + num[i - 1, j - 1];
                        }

                        if (num[i, j] > maxlen)
                        {
                            maxlen = num[i, j];
                            int thisSubsBegin = i - num[i, j] + 1;
                            if (lastSubsBegin == thisSubsBegin)
                            {
                                // if the current LCS is the same as the last time this block ran
                                sequenceBuilder.Append(str1[i]);
                            }
                            else
                            {
                                // this block resets the string builder if a different LCS is found
                                lastSubsBegin = thisSubsBegin;
                                sequenceBuilder.Length = 0; // clear it
                                sequenceBuilder.Append(str1.Substring(lastSubsBegin, (i + 1) - lastSubsBegin));
                            }
                        }
                    }
                }
            }

            sequence = sequenceBuilder.ToString();
            return maxlen;
        }
    }
}
