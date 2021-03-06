// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;

namespace ServerComparison.FunctionalTests
{
    public class Helpers
    {
        public static string GetApplicationPath()
        {
            return Path.GetFullPath(Path.Combine("..", "ServerComparison.TestSites"));
        }
    }
}