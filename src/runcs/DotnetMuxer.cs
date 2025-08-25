// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Devlooped
{
    static class DotnetMuxer
    {
        public static FileInfo? Path { get; }

        static DotnetMuxer()
        {
            var muxerFileName = ExecutableName("dotnet");
            var fxDepsFile = GetDataFromAppDomain("FX_DEPS_FILE");

            if (string.IsNullOrEmpty(fxDepsFile))
                return;

            var muxerDir = new FileInfo(fxDepsFile).Directory?.Parent?.Parent?.Parent;
            if (muxerDir == null)
                return;

            var muxerCandidate = new FileInfo(System.IO.Path.Combine(muxerDir.FullName, muxerFileName));
            if (muxerCandidate.Exists)
                Path = muxerCandidate;
        }

        public static string? GetDataFromAppDomain(string propertyName)
            => AppContext.GetData(propertyName) as string;

        public static string ExecutableName(this string withoutExtension)
            => RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? withoutExtension + ".exe"
                : withoutExtension;
    }
}
