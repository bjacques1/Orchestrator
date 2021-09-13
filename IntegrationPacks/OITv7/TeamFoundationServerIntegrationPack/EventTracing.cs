﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing;
using System.Text;

namespace TeamFoundationServerIntegrationPack
{
    /// <summary>
    ///     ETW singleton.  Needs to have AppResource.resx in the project.
    /// </summary>
    class EventTracing
    {
        private static Guid providerId;
        private static TraceListener listener;
        private static TraceSource source;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static EventTracing()
        {
            providerId = new Guid(AppResource.EtwProviderId);

            if (Environment.Version.Major < 6)
            {
                // Do not use ETW on Windows Server 2003 and below.
                listener = new DefaultTraceListener();
            }
            else
            {
                listener = new EventProviderTraceListener(
                    providerId.ToString(),
                    AppResource.EtwListenerName,
                    "::");
            }

            source = new TraceSource(
                AppResource.EtwProviderName,
                SourceLevels.All);
            source.Listeners.Add(listener);
        }

        /// <summary>
        ///     Suppress the default constructor generated by the compiler.
        /// </summary>
        private EventTracing()
        {
        }

        /// <summary>
        ///     Writes a trace event to ETW.
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="id"></param>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public static void TraceEvent(
            TraceEventType eventType,
            int id,
            string format,
            params object[] args)
        {
            source.TraceEvent(eventType, id, format, args);
        }

        /// <summary>
        ///     Writes an informational message to ETW.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public static void TraceInfo(
            string format,
            params object[] args)
        {
            source.TraceInformation(format, args);
        }

        /// <summary>
        ///     Returns a string representation of an enumerable.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static string ToString<T>(
            IEnumerable<T> values)
        {
            var sb = new StringBuilder();
            foreach (T val in values)
            {
                if (val == null)
                {
                    sb.Append(",");
                }
                else
                {
                    sb.Append(val.ToString() + ", ");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        ///     Returns a string representation of a dictionary.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict"></param>
        /// <returns></returns>
        public static string ToString<TKey, TValue>(
            Dictionary<TKey, TValue> dict)
        {
            var sb = new StringBuilder();
            foreach (KeyValuePair<TKey, TValue> kv in dict)
            {
                string val = kv.Value == null ? " " : kv.Value.ToString();
                sb.Append(kv.Key.ToString() + ":" + val + ", ");
            }

            return sb.ToString();
        }
    }
}
