// Guids.cs
// MUST match guids.h
using System;

namespace Bldr.Win8Notifier
{
    static class GuidList
    {
        public const string guidBldr_AddInPkgString = "0daa1106-308b-4d07-8839-d623fb79d5a2";
        public const string guidBldr_AddInCmdSetString = "34304516-8495-4cd0-ab3d-eb914f811c24";

        public static readonly Guid guidBldr_AddInCmdSet = new Guid(guidBldr_AddInCmdSetString);
    };
}