using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using System;

namespace ible.Foundation.Utility
{
    /// <summary>
    /// 提供各種共用 資訊 / 運算 / 顯示 流程
    /// </summary>
    public static class GameUtility
    {
        public enum TextStyle
        {
            Normal,
            Bold,
            Italic,
            Both
        }

        public const long Billion = 1000000000;
        public const long TenBillion = Billion * 10;
        public const int Million = 1000000;
        public const int TenMillion = Million * 10;
        public const int Kilo = 1000;
        public const int TenKilo = Kilo * 10;

        private const string TenBFormate = "{0:##.#}B";
        private const string BFormate = "{0:#.##}B";
        private const string TenMFormate = "{0:##.#}M";
        private const string MFormate = "{0:#.##}M";
        private const string TenKFormate = "{0:##.#}K";
        private const string KFormate = "{0:#.##}K";

        public readonly static Dictionary<long, string> s_NumberUnitENDict = new Dictionary<long, string>()
        {
            { Billion, "B" },
            { Million, "M" },
            { Kilo, "K" },
        };

        private static Dictionary<long, string> s_numberUnitDict = new Dictionary<long, string>();

        public class CacheStringBuilder<T> where T : IEquatable<T>
        {
            private bool _dirty = true;
            private bool _updateBuffer = false;
            private bool _updateCharArray = false;
            private bool _onCacheUpdateNotifying = false;

            private T _value;
            private string _cache;
            private char[] _charArray;

            protected StringBuilder builder;
            protected Action<StringBuilder, T> action;
            protected Action<StringBuilder, T, string> actionWithFormat;
            protected string format;

            public Func<CacheStringBuilder<T>, string, string> OnCacheUpdate;

            public CacheStringBuilder(int capacity,
                Action<StringBuilder, T> toStringAction,
                Action<StringBuilder, T, string> toStringActionWithFormat = null)
            {
                builder = new StringBuilder(capacity);
                action = toStringAction;
                actionWithFormat = toStringActionWithFormat;
            }

            public int GetCharBuffer(out char[] charArray)
            {
                // update string buffer
                if (!_updateBuffer)
                {
                    UpdateBuilder();
                }

                // resize array?
                if (_charArray == null)
                {
                    _charArray = new char[builder.Capacity];
                }
                else if (_charArray.Length != builder.Capacity)
                {
                    Array.Resize(ref _charArray, builder.Capacity);
                }

                // get character array
                if (!_updateCharArray)
                {
                    builder.CopyTo(0, _charArray, 0, builder.Length);
                    _updateCharArray = true;
                }

                if (builder.Length == 0)
                {
                    charArray = null;
                    return 0;
                }
                else
                {
                    charArray = _charArray;
                    return builder.Length;
                }
            }

            protected void UpdateBuilder()
            {
                if (!_updateBuffer)
                {
                    builder.Length = 0;
                    if (action != null)
                    {
                        action(builder, _value);
                    }
                    else if (actionWithFormat != null)
                    {
                        actionWithFormat(builder, _value, format);
                    }
                    _updateBuffer = true;
                }
            }

            public void ExecCacheUpdate()
            {
                if (_dirty)
                {
                    UpdateBuilder();
                    _cache = builder.ToString();
                    _dirty = false;

                    if (OnCacheUpdate != null && !_onCacheUpdateNotifying)
                    {
                        _onCacheUpdateNotifying = true;
                        _cache = OnCacheUpdate(this, _cache);
                        _onCacheUpdateNotifying = false;
                    }
                }
            }

            public override string ToString()
            {
                if (_dirty)
                {
                    UpdateBuilder();
                    _cache = builder.ToString();
                    _dirty = false;

                    if (OnCacheUpdate != null && !_onCacheUpdateNotifying)
                    {
                        _onCacheUpdateNotifying = true;
                        _cache = OnCacheUpdate(this, _cache);
                        _onCacheUpdateNotifying = false;
                    }
                }

                return _cache;
            }

            public T Value
            {
                get
                {
                    return _value;
                }
                set
                {
                    if (!_value.Equals(value))
                    {
                        _value = value;
                        _dirty = true;
                        _updateBuffer = false;
                        _updateCharArray = false;
                    }
                }
            }

            public string Format
            {
                get
                {
                    return format;
                }
                set
                {
                    if (format != value)
                    {
                        format = value;
                        _dirty = true;
                        _updateBuffer = false;
                        _updateCharArray = false;
                    }
                }
            }
        }

        public class Digit2String
        {
            private List<string> _cacheDigitString;
            private string _format;
            private Dictionary<int, string> _digitStringMapping;
            private int _range;

            public Digit2String(int range, string format)
            {
                _digitStringMapping = new Dictionary<int, string>(range);
                _format = format;
                _range = range;
            }

            public string this[int index]
            {
                get
                {
                    if (index < 0)
                    {
                        return index.ToString();
                    }
                    if (_digitStringMapping.TryGetValue(index, out var result))
                        return result;
                    result = string.Format(_format, index);
                    _digitStringMapping[index] = result;
                    return result;
                }
            }

            public int Range => _range;
            public string Format { get { return _format; } }
        }

        private static Dictionary<float, string> s_powerCompareDict;

        public static void UpdatePowerCompare(Dictionary<float, string> powerCompareDict)
        {
            s_powerCompareDict = powerCompareDict;
        }

        public static string PowerCompare(float value)
        {
            var iter = s_powerCompareDict.GetEnumerator();
            while (iter.MoveNext())
            {
                float rate = iter.Current.Key;
                string text = iter.Current.Value;
                if (value >= rate)
                {
                    return text;
                }
            }
            iter.Dispose();
            return string.Empty;
        }

        private static string s_distanceFormat = "{0}KM";
        public static void UpdateDistance(string distanceFormat)
        {
            s_distanceFormat = distanceFormat;
        }

        public static readonly Digit2String DigiTable1 = new Digit2String(9, "{0:0}");
        public static readonly Digit2String DigiTable2 = new Digit2String(99, "{0:00}");
        public static readonly Digit2String DigiTable3 = new Digit2String(999, "{0:000}");

        public static Digit2String HoursTable = new Digit2String(23, "{0}h");
        public static Digit2String MinutesTable = new Digit2String(59, "{0}m");
        public static Digit2String SecondsTable = new Digit2String(59, "{0}s");
        public static Digit2String DaysTable = new Digit2String(99, "{0}d");

        public static Digit2String ItemHoursTable = new Digit2String(23, "{0}h");
        public static Digit2String ItemMinutesTable = new Digit2String(59, "{0}m");
        public static Digit2String ItemSecondsTable = new Digit2String(59, "{0}s");
        public static Digit2String ItemDaysTable = new Digit2String(99, "{0}d");

        private static System.Random s_usedRandom = new System.Random();

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = s_usedRandom.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static void UpdateTimeText(string dayText, string hourText, string minuteText, string secondText)
        {
            DaysTable = new Digit2String(99, "{0}" + dayText);
            HoursTable = new Digit2String(23, "{0}" + hourText);
            MinutesTable = new Digit2String(59, "{0}" + minuteText);
            SecondsTable = new Digit2String(59, "{0}" + secondText);
        }

        public static void UpdateItemTimeText(string dayText, string hourText, string minuteText, string secondText)
        {
            ItemDaysTable = new Digit2String(99, "{0}" + dayText);
            ItemHoursTable = new Digit2String(23, "{0}" + hourText);
            ItemMinutesTable = new Digit2String(59, "{0}" + minuteText);
            ItemSecondsTable = new Digit2String(59, "{0}" + secondText);
        }

        public static string QueryTimeTableString(int value, Digit2String table)
        {
            if (value <= table.Range)
            {
                return table[value];
            }
            else
            {
                return string.Format(table.Format, value);
            }
        }

        public static string QueryDigiTableString(int value)
        {
            if (value < 0 || value >= 100)
            {
                return value.ToString();
            }
            else if (value < 10)
            {
                return DigiTable1[value];
            }
            else
            {
                return DigiTable2[value];
            }
        }

        public static string NumberToString(int value)
        {
            if (value < 0 || value >= 1000)
            {
                return value.ToString();
            }
            else if (value < 10)
            {
                return DigiTable1[value];
            }
            else if (value < 100)
            {
                return DigiTable2[value];
            }
            else
            {
                return DigiTable3[value];
            }
        }

        public static void GetCurrentEnumValueFromString<TPropertyType>(string enumName, out TPropertyType propertyValue)
        {
            string[] propertyNames = Enum.GetNames(typeof(TPropertyType));
            Array propertyValues = Enum.GetValues(typeof(TPropertyType));
            for (int i = 0; i < propertyNames.Length; i++)
            {
                if (enumName != propertyNames[i])
                {
                    continue;
                }
                propertyValue = (TPropertyType)propertyValues.GetValue(i);
                return;
            }

            propertyValue = default(TPropertyType);
        }

        public static void GetCurrentEnumValueFromIntValue<TPropertyType>(int enumValue, out TPropertyType propertyValue)
        {
            Array propertyValues = Enum.GetValues(typeof(TPropertyType));
            for (int i = 0; i < propertyValues.Length; i++)
            {
                if (enumValue != (int)propertyValues.GetValue(i))
                {
                    continue;
                }

                propertyValue = (TPropertyType)propertyValues.GetValue(i);
                return;
            }

            propertyValue = default(TPropertyType);
        }

        public static string ConvertIntToStringWithCommas(int value)
        {
            return value == 0 ? value.ToString()
                : (value > 1000 ? value.ToString("#,#", CultureInfo.InvariantCulture) : NumberToString(value));
        }

        public const float GBSize = 1024 * 1024 * 1024;
        public const float MBSize = 1024 * 1024;
        public const float KBSize = 1024;
        public static string SizeToString(long bytes)
        {
            if (bytes > GBSize)
                return $"{bytes / GBSize}gb";
            else if (bytes > MBSize)
                return $"{bytes / MBSize}mb";
            else
                return $"{bytes / KBSize}kb";
        }

        public static string SizeToString(ulong bytes)
        {
            if (bytes > GBSize)
                return $"{bytes / GBSize}gb";
            else if (bytes > MBSize)
                return $"{bytes / MBSize}mb";
            else
                return $"{bytes / KBSize}kb";
        }

        public static string ConvertIntToStringWithCommas(long value)
        {
            return value == 0 ? value.ToString() : value.ToString("#,#", CultureInfo.InvariantCulture);
        }

        public static string ConvertIntToStringWithCommas(ulong value)
        {
            return value == 0 ? value.ToString() : value.ToString("#,#", CultureInfo.InvariantCulture);
        }

        public static DateTime GetLinkerTime(this Assembly assembly, TimeZoneInfo target = null)
        {
            var filePath = assembly.Location;
            const int peHeaderOffset = 60;
            const int linkerTimestampOffset = 8;

            var buffer = new byte[2048];

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                stream.Read(buffer, 0, 2048);

            var offset = BitConverter.ToInt32(buffer, peHeaderOffset);
            var secondsSince1970 = BitConverter.ToInt32(buffer, offset + linkerTimestampOffset);
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var linkTimeUtc = epoch.AddSeconds(secondsSince1970);
            var localTime = linkTimeUtc.AddHours(8);

            return localTime;
        }

#if UNITY_5_3_OR_NEWER

        public static RuntimePlatform GetRuntimePlatform()
        {
            RuntimePlatform platform = Application.platform;
#if UNITY_EDITOR
            //overwritten by editor build setting
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.Android:
                    platform = RuntimePlatform.Android;
                    break;

                case BuildTarget.iOS:
                    platform = RuntimePlatform.IPhonePlayer;
                    break;

                default:
                    platform = RuntimePlatform.WindowsPlayer;
                    break;
            }
#endif
            return platform;
        }

#endif
        private static System.Text.StringBuilder s_pathCollector = new StringBuilder();
        private static List<string> s_pathSegment = new List<string>();
#if UNITY_5_3_OR_NEWER

        public static string PathOfGameObject(GameObject go, Transform root = null)
        {
            s_pathCollector.Length = 0;
            s_pathSegment.Clear();
            //string path = go.name;
            s_pathSegment.Add(go.name);

            Transform parent = go.transform.parent;
            while (parent != null && parent != root)
            {
                s_pathSegment.Add(parent.name);
                parent = parent.parent;
            }

            // connect strings
            for (int i = s_pathSegment.Count - 1; i >= 0; i--)
            {
                s_pathCollector.Append(s_pathSegment[i]);
                if (i > 0)
                {
                    s_pathCollector.Append("/");
                }
            }

            return s_pathCollector.ToString();
        }
#endif

        public class TimeSpanStringCache : CacheStringBuilder<TimeSpan>
        {
            public enum FormatMode
            {
                FullRemainTime,
                ShortTime,
            }

            public FormatMode Mode { get; private set; }
            public Action<StringBuilder, TimeSpan, TimeSpanStringCache> OnCustomUpdate;

            public TimeSpanStringCache(int capacity = 30, FormatMode mode = FormatMode.FullRemainTime,
                System.Action<StringBuilder, TimeSpan, string> toStringActionWithFormat = null) : base(capacity,
                    null, toStringActionWithFormat)
            {
                action = UpdateTimeString;
                Mode = mode;
            }

            public int MaxDayLimit { get; set; }

            public string ToShortTimeString()
            {
                return ToShortTimeString(Value);
            }

            private void UpdateTimeString(StringBuilder builder, TimeSpan ts)
            {
                if (OnCustomUpdate != null)
                {
                    OnCustomUpdate(builder, ts, this);
                }
                else
                {
                    UpdateTimeString();
                }
            }

            public void UpdateTimeString()
            {
                switch (Mode)
                {
                    case FormatMode.FullRemainTime:
                        UpdateFullTimeBuilder(builder, Value);
                        break;

                    case FormatMode.ShortTime:
                        UpdateShortTimeBuilder(builder, Value);
                        break;
                }
            }

            public static string ToShortTimeString(TimeSpan duration, int maxDayLimit = 7)
            {
                if (duration.Days > 0) // 一天
                {
                    int days;
                    if (maxDayLimit != 0 && duration.Days > maxDayLimit)
                        days = maxDayLimit;
                    else
                        days = duration.Days;
                    return QueryTimeTableString(days, DaysTable);
                }
                if (duration.Hours > 0) // 1小時
                {
                    return QueryTimeTableString(duration.Hours, HoursTable);
                }
                if (duration.Minutes > 0) // 1分鐘
                {
                    return QueryTimeTableString(duration.Minutes, MinutesTable);
                }
                return QueryTimeTableString(0, MinutesTable);
            }

            public static void UpdateShortTimeBuilder(StringBuilder builder, TimeSpan ts, int maxDayLimit = 7)
            {
                builder.Append(ToShortTimeString(ts, maxDayLimit));
            }

            public static void UpdateFullTimeBuilder(StringBuilder builder, TimeSpan ts)
            {
                if (ts.Seconds < 0 || ts.Minutes < 0 || ts.Hours < 0 || ts.Days < 0)
                {
                    builder.Append("---");
                    return;
                }

                if (ts.Days > 0)
                {
                    // days
                    builder.Append(QueryTimeTableString(ts.Days, DaysTable));
                    builder.Append(" ");
                    // hours
                    builder.Append(DigiTable2[ts.Hours]);
                    builder.Append(":");
                    //minutes
                    builder.Append(DigiTable2[ts.Minutes]);
                    builder.Append(":");
                    //seconds
                    builder.Append(DigiTable2[ts.Seconds]);
                }
                else
                {
                    // hours
                    builder.Append(DigiTable2[ts.Hours]);
                    builder.Append(":");
                    //minutes
                    builder.Append(DigiTable2[ts.Minutes]);
                    builder.Append(":");
                    //seconds
                    builder.Append(DigiTable2[ts.Seconds]);
                }
            }

            public static void UpdateDayOrTimeInDayBuilder(StringBuilder builder, TimeSpan ts)
            {
                if (ts.Seconds < 0 || ts.Minutes < 0 || ts.Hours < 0 || ts.Days < 0)
                {
                    builder.Append("---");
                    return;
                }

                if (ts.Days > 0)
                {
                    builder.Append(QueryTimeTableString(ts.Days, DaysTable));
                }
                else
                {
                    // hours
                    builder.Append(DigiTable2[ts.Hours]);
                    builder.Append(":");
                    //minutes
                    builder.Append(DigiTable2[ts.Minutes]);
                    builder.Append(":");
                    //seconds
                    builder.Append(DigiTable2[ts.Seconds]);
                }
            }

            public static void UpdateDayOrHourOrTimeInHourBuilder(StringBuilder builder, TimeSpan ts)
            {
                if (ts.Seconds < 0 || ts.Minutes < 0 || ts.Hours < 0 || ts.Days < 0)
                {
                    builder.Append("---");
                    return;
                }

                if (ts.Days > 0)
                {
                    builder.Append(QueryTimeTableString(ts.Days, DaysTable));
                }
                else if (ts.Hours > 0)
                {
                    builder.Append(HoursTable[ts.Hours]);
                }
                else
                {
                    //minutes
                    builder.Append(DigiTable2[ts.Minutes]);
                    builder.Append(":");
                    //seconds
                    builder.Append(DigiTable2[ts.Seconds]);
                }
            }
        }

        public class TimeSecondStringCache : CacheStringBuilder<long>
        {
            public TimeSpanStringCache.FormatMode Mode { get; private set; }
            public Action<StringBuilder, long, TimeSecondStringCache> OnCustomUpdate;

            public TimeSecondStringCache(int capacity = 30, TimeSpanStringCache.FormatMode mode = TimeSpanStringCache.FormatMode.FullRemainTime,
                System.Action<StringBuilder, long, string> toStringActionWithFormat = null) : base(capacity, null, toStringActionWithFormat)
            {
                Mode = mode;
                action = UpdateTimeString;
            }

            private void UpdateTimeString(StringBuilder builder, long duration)
            {
                if (duration < 0)
                {
                    duration = 0;
                }

                if (OnCustomUpdate != null)
                {
                    OnCustomUpdate.Invoke(builder, duration, this);
                }
                else
                {
                    UpdateTimeString();
                }
            }

            public void UpdateTimeString()
            {
                switch (Mode)
                {
                    case TimeSpanStringCache.FormatMode.FullRemainTime:
                        UpdateFullTimeBuilder(builder, Value);
                        break;

                    case TimeSpanStringCache.FormatMode.ShortTime:
                        UpdateSortTimeBuilder(builder, Value);
                        break;
                }
            }

            public static void UpdateFullTimeBuilder(StringBuilder builder, long duration)
            {
                TimeSpanStringCache.UpdateFullTimeBuilder(builder, TimeSpan.FromTicks(TimeSpan.TicksPerSecond * duration));
            }

            public static void UpdateSortTimeBuilder(StringBuilder builder, long duration)
            {
                TimeSpanStringCache.UpdateShortTimeBuilder(builder, TimeSpan.FromTicks(TimeSpan.TicksPerSecond * duration));
            }

            public static void UpdateDayOrTimeInDayBuilder(StringBuilder builder, long duration)
            {
                TimeSpanStringCache.UpdateDayOrTimeInDayBuilder(builder, TimeSpan.FromTicks(TimeSpan.TicksPerSecond * duration));
            }

            public static void UpdateDayOrHourOrTimeInHourBuilder(StringBuilder builder, long duration)
            {
                TimeSpanStringCache.UpdateDayOrHourOrTimeInHourBuilder(builder, TimeSpan.FromTicks(TimeSpan.TicksPerSecond * duration));
            }
        }

        public static void UpdateFullDateTimeBuilder(StringBuilder builder, DateTime dt)
        {
            int year = dt.Year;
            int y0 = year / 100;
            int y1 = year % 100;
            //year
            builder.Append(DigiTable2[y0]);
            builder.Append(DigiTable2[y1]);
            builder.Append('/');
            //mon
            builder.Append(QueryDigiTableString(dt.Month));
            builder.Append('/');
            //day
            builder.Append(QueryDigiTableString(dt.Day));
            builder.Append(' ');
            //hour
            builder.Append(DigiTable2[dt.Hour]);
            builder.Append(':');
            //min
            builder.Append(DigiTable2[dt.Minute]);
        }

        public static void UpdateFullDateTimeOfYearBuilder(StringBuilder builder, DateTime dt, bool withSecond = false)
        {
            //mon
            builder.Append(QueryDigiTableString(dt.Month));
            builder.Append('-');
            //day
            builder.Append(QueryDigiTableString(dt.Day));
            builder.Append(' ');
            //hour
            builder.Append(DigiTable2[dt.Hour]);
            builder.Append(':');
            //min
            builder.Append(DigiTable2[dt.Minute]);
            if (withSecond)
            {
                // second
                builder.Append(':');
                builder.Append(DigiTable2[dt.Second]);
            }
        }

        public static string SecondsToShortTimeString(long seconds, int maxDayLimit = 7)
        {
            if (seconds < 0)
            {
                seconds = 0;
            }

            TimeSpan duration = TimeSpan.FromTicks(TimeSpan.TicksPerSecond * seconds);
            return TimeSpanStringCache.ToShortTimeString(duration, maxDayLimit);
        }

        /// <summary>
        /// 通用倒數時間
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static string SecondsToRemainingTimeString(long seconds)
        {
            return SecondsToFullRemainingString(seconds);
        }
        /// <summary>
        /// 消費相關顯示完整時間
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static string SecondsToFullRemainingString(long seconds)
        {
            sharedFullRemainingTimeStringBuilder.Value = seconds;
            return sharedFullRemainingTimeStringBuilder.ToString();
        }

        /// <summary>
        /// 通用倒數時間Cache
        /// </summary>
        internal static CacheStringBuilder<long> sharedFullRemainingTimeStringBuilder = new TimeSecondStringCache(30);
        internal static CacheStringBuilder<TimeSpan> sharedFullRemainingTimeSpanStringBuilder = new TimeSpanStringCache(30);

        /// <summary>
        /// 通用累積時間Cache
        /// </summary>
        internal static CacheStringBuilder<long> cacheAccumulationTimeStringBuilder = new CacheStringBuilder<long>(30,
            (builder, seconds) =>
            {
                TimeSpan ts = TimeSpan.FromTicks(TimeSpan.TicksPerSecond * seconds);
                if (ts.Days > 0)
                {
                    builder.Append(DaysTable[ts.Days]);
                }

                if (ts.Hours > 0)
                {
                    builder.Append(HoursTable[ts.Hours]);
                }

                if (ts.Minutes > 0)
                {
                    builder.Append(MinutesTable[ts.Minutes]);
                }

                builder.Append(SecondsTable[ts.Seconds]);
            });

        /// <summary>
        /// 通用累積時間Cache
        /// </summary>
        internal static CacheStringBuilder<long> cacheAccumulationTimeShortStringBuilder = new CacheStringBuilder<long>(30,
            (builder, seconds) =>
            {
                int addCount = 2;

                TimeSpan ts = TimeSpan.FromTicks(TimeSpan.TicksPerSecond * seconds);

                if (ts.Days > 0)
                {
                    builder.Append(DaysTable[ts.Days]);
                    addCount--;
                    if (addCount == 0)
                        return;
                }

                if (ts.Hours > 0)
                {
                    builder.Append(HoursTable[ts.Hours]);
                    addCount--;
                    if (addCount == 0)
                        return;
                }

                if (ts.Minutes > 0)
                {
                    builder.Append(MinutesTable[ts.Minutes]);
                    addCount--;
                    if (addCount == 0)
                        return;
                }

                builder.Append(SecondsTable[ts.Seconds]);
            });
        public static string DataTimeToChatMessageString(DateTime dataTime)
        {
            return dataTime.ToString("yyyy/M/d HH:mm:ss");
        }

        public static string ItemSecondsFormat(int seconds, bool onlyNumber)
        {
            if (seconds == 0 || seconds % 60 > 0)
            {
                if (onlyNumber)
                    return QueryDigiTableString(seconds);
                else
                    return QueryTimeTableString(seconds, ItemSecondsTable);
            }

            int minutes = seconds / 60;

            int hours = minutes / 60;
            if (minutes % 60 > 0 || hours == 1)
            {
                if (onlyNumber)
                    return QueryDigiTableString(minutes);
                else
                    return QueryTimeTableString(minutes, ItemMinutesTable);
            }

            int days = hours / 24;
            if (hours % 24 > 0 || days == 1)
            {
                if (onlyNumber)
                    return QueryDigiTableString(hours);
                else
                    return QueryTimeTableString(hours, ItemHoursTable);
            }

            if (onlyNumber)
                return QueryDigiTableString(days);
            else
                return QueryTimeTableString(days, ItemDaysTable);
        }

        /// <summary>
        /// 伺服器時間顯示Cache
        /// </summary>
        internal static CacheStringBuilder<DateTime> cacheDateTimeStringBuilder = new CacheStringBuilder<DateTime>(30,
            (builder, dateTime) =>
            {
                //builder.AppendFormat("{0:00}:{1:00}:{2:00}", dateTime.Hour, dateTime.Minute, dateTime.Second);
                builder.AppendFormat("{0}:{1}:{2}", DigiTable2[dateTime.Hour], DigiTable2[dateTime.Minute], DigiTable2[dateTime.Second]);
            });

        /// <summary>
        /// 伺服器時間顯示 H:M:S
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string DateTimeToTimeString(DateTime dt)
        {
            cacheDateTimeStringBuilder.Value = dt;
            return cacheDateTimeStringBuilder.ToString();
        }

        /// <summary>
        /// 伺服器時間顯示 Y/M/D
        /// </summary>
        internal static CacheStringBuilder<DateTime> cacheDateTimeToDateStringBuilder = new CacheStringBuilder<DateTime>(30,
        (builder, dateTime) =>
        {
            builder.AppendFormat("{0}/{1}/{2}", dateTime.Year.ToString(), QueryDigiTableString(dateTime.Month), QueryDigiTableString(dateTime.Day));
        });

        /// <summary>
        /// 伺服器日期顯示
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string DateTimeToDateString(DateTime dt)
        {
            cacheDateTimeToDateStringBuilder.Value = dt;
            return cacheDateTimeToDateStringBuilder.ToString();
        }
        /// <summary>
        /// RichText處理
        /// </summary>
        /// <param name="content"></param>
        /// <param name="colorCode"></param>
        /// <param name="textStyle"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static string RichTextEffect(string content, string colorCode, TextStyle textStyle = TextStyle.Normal, int size = -1)
        {
            string result = string.Format("<color=#{0}>{1}</color>", colorCode, content);
            switch (textStyle)
            {
                case TextStyle.Bold:
                    result = string.Format("<b>{0}</b>", result);
                    break;

                case TextStyle.Italic:
                    result = string.Format("<i>{0}</i>", result);
                    break;

                case TextStyle.Both:
                    result = string.Format("<b><i>{0}</i></b>", result);
                    break;
            }
            if (size != -1)
            {
                result = string.Format("<size={0}>{1}</size> ", size, result);
            }
            return result;
        }

#if UNITY_5_3_OR_NEWER

        /// <summary>
        /// RichText處理 使用 UnityEngine.Color
        /// </summary>
        /// <param name="content"></param>
        /// <param name="color"></param>
        /// <param name="textStyle"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static string RichTextEffect(string content, Color color, TextStyle textStyle = TextStyle.Normal, int size = -1)
        {
            return RichTextEffect(content, ColorUtility.ToHtmlStringRGB(color), textStyle, size);
        }

#endif

        /// <summary>
        /// 轉化字串 (現有數值 / 需求數值)
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="needAmount"></param>
        /// <returns></returns>
        public static string ItemNeedAmountText(int amount, int needAmount)
        {
            if (amount < needAmount)
                return string.Format("<color=#F47C7CFF>{0}</color>", amount);
            else
                return string.Format("{0}", amount);
        }
        /// <summary>
        /// 顯示座標字串
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static string CoordinateString(int x, int y)
        {
            return CoordinateString(0, x, y);
        }

        /// <summary>
        /// 顯示座標字串
        /// </summary>
        /// <param name="k"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static string CoordinateString(int k, int x, int y)
        {
            if (k > 0)
                return string.Format("{0} {1} {2}", CoordinateSymbolString("K", k), CoordinateSymbolString("X", x), CoordinateSymbolString("Y", y));
            return string.Format("{0} {1}", CoordinateSymbolString("X", x), CoordinateSymbolString("Y", y));
        }

        /// <summary>
        /// 顯示座標字串
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static string CoordinateSymbolString(string symbol, int v)
        {
            return string.Format("{0}:{1}", symbol, v);
        }

        public static Digit2String LevelsTable = new Digit2String(99, "LV:{0}");

        public static void UpdateLevelString(string format)
        {
            LevelsTable = new Digit2String(99, format);
        }

        public static string LVString(int level)
        {
            if (level <= LevelsTable.Range)
            {
                return LevelsTable[level];
            }
            else
            {
                return string.Format(LevelsTable.Format, level);
            }
        }


        private static string OutOfRankge = "--";
        /// <summary>
        ///排行榜排名字串
        /// </summary>
        /// <param name="rank"></param>
        /// <returns></returns>
        public static string RankString(int rank)
        {
            if (rank == 0 || rank == -1)
                return OutOfRankge;
            return QueryDigiTableString(rank);
        }

        public static void Digits(this StringBuilder builder, int number)
        {
            if (number >= 100000000)
            {
                // Use system ToString.
                builder.Append(number.ToString());
                return;
            }
            if (number < 0)
            {
                // Negative.
                builder.Append(number.ToString());
                return;
            }
            int copy;
            int digit;
            if (number >= 10000000)
            {
                // 8.
                copy = number % 100000000;
                digit = copy / 10000000;
                builder.Append((char)(digit + 48));
            }
            if (number >= 1000000)
            {
                // 7.
                copy = number % 10000000;
                digit = copy / 1000000;
                builder.Append((char)(digit + 48));
            }
            if (number >= 100000)
            {
                // 6.
                copy = number % 1000000;
                digit = copy / 100000;
                builder.Append((char)(digit + 48));
            }
            if (number >= 10000)
            {
                // 5.
                copy = number % 100000;
                digit = copy / 10000;
                builder.Append((char)(digit + 48));
            }
            if (number >= 1000)
            {
                // 4.
                copy = number % 10000;
                digit = copy / 1000;
                builder.Append((char)(digit + 48));
            }
            if (number >= 100)
            {
                // 3.
                copy = number % 1000;
                digit = copy / 100;
                builder.Append((char)(digit + 48));
            }
            if (number >= 10)
            {
                // 2.
                copy = number % 100;
                digit = copy / 10;
                builder.Append((char)(digit + 48));
            }
            if (number >= 0)
            {
                // 1.
                copy = number % 10;
                digit = copy / 1;
                builder.Append((char)(digit + 48));
            }
        }

        public static void RoundDigits(this StringBuilder builder, float number, int digits)
        {
            float val = (float)Math.Round(number, digits);
            if (val < 0 || val > 10000 || digits > 3)
            {
                // Negative.
                builder.Append(val.ToString());
                return;
            }

            int multiplyBase = 1;
            switch (digits)
            {
                case 1: multiplyBase = 10; break;
                case 2: multiplyBase = 100; break;
                case 3: multiplyBase = 1000; break;
            }

            int shift = (int)(val * multiplyBase);
            int prefix = (int)val;
            int surfix = shift % multiplyBase;

            Digits(builder, prefix);
            if (surfix > 0)
            {
                builder.Append('.');
                int divider = multiplyBase / 10;
                int digit = 0;
                for (int i = 0; i < digits; ++i)
                {
                    digit = (surfix / divider) % 10;
                    builder.Append((char)(digit + 48));
                    divider = divider / 10;
                }
            }
        }

        /// <summary>
        /// 組成版本號字串
        /// </summary>
        /// <param name="ver"></param>
        /// <param name="majorVersion"></param>
        /// <param name="minorVersion"></param>
        /// <param name="patchVersion"></param>
        public static void ConvertToVersion(string ver,
            out int majorVersion, out int minorVersion, out int patchVersion)
        {
            majorVersion = 0;
            minorVersion = 0;
            patchVersion = 0;

            if (string.IsNullOrEmpty(ver))
                return;

            string[] versions = ver.Split('.');
            if (versions.Length > 0)
                int.TryParse(versions[0], out majorVersion);
            if (versions.Length > 1)
                int.TryParse(versions[1], out minorVersion);
            if (versions.Length > 2)
                int.TryParse(versions[2], out patchVersion);
        }

        /// <summary>
        /// 組成版本號字串
        /// </summary>
        /// <param name="majorVersion"></param>
        /// <param name="minorVersion"></param>
        /// <param name="patchVersion"></param>
        public static void GetApplicationVersion(out int majorVersion, out int minorVersion, out int patchVersion)
        {
            majorVersion = 0;
            minorVersion = 0;
            patchVersion = 0;
        }


        /// <summary>
        /// 依 language code 顯示不同的時間字串
        /// </summary>
        /// <param name="content"></param>
        /// <param name="languageCode"></param>
        /// <returns></returns>
        public static string ParseTimeTag(string content, string languageCode)
        {
            string preTag = "<%t";
            string postTag = ">";
            string result = string.Empty;
            int startIndex = 0;
            int findIndex = -1;
            do
            {
                findIndex = content.IndexOf(preTag, startIndex);
                if (findIndex == -1)
                {
                    result += content.Substring(startIndex, content.Length - startIndex);
                    break;
                }
                int endIndex = content.IndexOf(postTag, findIndex + preTag.Length);
                if (endIndex == -1)
                {
                    result += content.Substring(startIndex, content.Length - startIndex);
                    break;
                }

                long time;
                if (long.TryParse(content.Substring(findIndex + preTag.Length, endIndex - (findIndex + preTag.Length)), out time))
                {
                    result += content.Substring(startIndex, findIndex - startIndex);

                    DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                    dateTime = dateTime.AddSeconds(time).ToLocalTime();
                    result += GetCultureDateTimeString(dateTime, languageCode);
                    startIndex = endIndex + postTag.Length;
                }
                else
                {
                    result += content.Substring(startIndex, content.Length - startIndex);
                    break;
                }
            }
            while (findIndex != -1);
            return result;
        }

        /// <summary>
        /// 依 language code 顯示不同的時間字串
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="langaugeCode"></param>
        /// <returns></returns>
        public static string GetCultureDateTimeString(DateTime dt, string langaugeCode)
        {
            switch (langaugeCode)
            {
                case "en":
                    return dt.ToString("MM/dd/yyyy HH:mm");

                case "zh-TW":
                case "zh-CN":
                default:
                    return dt.ToString("yyyy/MM/dd HH:mm");
            }
        }

        /// <summary>
        /// 場景座標距離文字
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="pos2"></param>
        /// <returns></returns>
        public static string GetDistanceText(Vector3 pos, Vector3 pos2)
        {
            float distance = Vector3.Distance(pos * 0.1f, pos2 * 0.1f);
            return GetDistanceText(Mathf.CeilToInt(distance));
        }

        /// <summary>
        /// 顯示座標距離文字
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="pos2"></param>
        /// <returns></returns>
        public static string GetDistanceText(Vector2 pos, Vector2 pos2)
        {
            float distance = Vector2.Distance(pos, pos2);
            return GetDistanceText(Mathf.CeilToInt(distance));
        }

        public static string GetDistanceText(int distance)
        {
            return string.Format(s_distanceFormat, distance);
        }

        public static void ExceptWith<T>(this HashSet<T> set, HashSet<T> other)
        {
            if (other == null || other.Count == 0) return;
            if (set.Count == 0) return;
            if (other == set)
            {
                set.Clear();
                return;
            }

            foreach (var o in other)
            {
                set.Remove(o);
            }
        }

        public static void SetTextWithEllipsis(this Text textComponent, string value)
        {
            if (value.Length == 0)
                return;
            var generator = new TextGenerator();
            var rectTransform = textComponent.GetComponent<RectTransform>();
            var settings = textComponent.GetGenerationSettings(rectTransform.rect.size);
            generator.Populate(value, settings);
            var characterCountVisible = generator.characterCountVisible;
            var updatedText = value;
            if (value.Length > characterCountVisible && characterCountVisible > 1)
            {
                updatedText = value.Substring(0, characterCountVisible - 1);
                updatedText += "…";
            }
            textComponent.text = updatedText;
        }

        /// <summary>
        /// 檢查版本是否要更新
        /// </summary>
        /// <param name="applicationVer">*.*.*</param>
        /// <param name="compareVer">*.*.*</param>
        /// <returns>是否要更新</returns>
        public static bool CompareVersion(string applicationVer, string compareVer)
        {
            var regex = new Regex(@"^(\d+).(\d+).(\d+)$");
            var compareVerValid = regex.IsMatch(compareVer);
            if (!compareVerValid)
                return false;
            var applicationVerValid = regex.IsMatch(applicationVer);
            if (!applicationVerValid)
                return true;

            var avIds = applicationVer.Split('.');
            var cvIds = compareVer.Split('.');
            for (int i = 0; i < 3; i++)
            {
                var avid = int.Parse(avIds[i]);
                var cvid = int.Parse(cvIds[i]);
                var compare = avid.CompareTo(cvid);

                switch (compare)
                {
                    case 0: continue;
                    case 1: return false;
                    case -1: return true;
                }
            }
            return false;
        }

        public static string FormatDateTimeToSeparatePart(DateTime dateTime)
        {
            return string.Format("{0} {1}",
                          DateTimeToDateString(dateTime),
                          DateTimeToTimeString(dateTime)
                          );
        }

        private static readonly StringBuilder _txtBuilder = new StringBuilder();
        private static DateTime _systemDT = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static void CheckConvertServerMaintenanceText(ref string targetText)
        {
            string strStartIdentifier = "[Ts]";
            string strEndIdentifier = "[Te]";

            int indexFrom0 = targetText.IndexOf(strStartIdentifier);
            int indexTo = targetText.IndexOf(strEndIdentifier);
            int indexEnd = indexTo - indexFrom0 + strEndIdentifier.Length;

            bool needConvert = (indexFrom0 > 0 && indexTo > 0);

            if (needConvert == false)
                return;

            string tS = string.Empty;
            string tE = string.Empty;

            _txtBuilder.Clear();

            //[Ts] 前
            _txtBuilder.Append(targetText.Substring(0, indexFrom0 - 1));

            //中間轉換段
            string subStr = targetText.Substring(indexFrom0, indexEnd);
            var matchs = Regex.Matches(subStr, "\\d{10}");

            if (matchs.Count > 1)
            {
                long timeL;

                if (long.TryParse(matchs[0].ToString(), out timeL) == true)
                    tS = FormatDateTimeToSeparatePart(_systemDT.AddSeconds(timeL).ToLocalTime());

                if (long.TryParse(matchs[1].ToString(), out timeL) == true)
                    tE = FormatDateTimeToSeparatePart(_systemDT.AddSeconds(timeL).ToLocalTime());
            }

            var newStr = Regex.Replace(subStr, matchs[0].ToString(), "0");
            newStr = Regex.Replace(newStr, matchs[1].ToString(), "1");
            newStr = newStr.Remove(newStr.IndexOf(strStartIdentifier), strStartIdentifier.Length);
            newStr = newStr.Remove(newStr.IndexOf(strEndIdentifier), strEndIdentifier.Length);
            newStr = string.Format(newStr, tS, tE);

            _txtBuilder.Append(newStr);
            int idxEndBegin = indexTo + strEndIdentifier.Length;
            _txtBuilder.Append(targetText.Substring(idxEndBegin, targetText.Length - idxEndBegin));
            targetText = _txtBuilder.ToString();
        }

        /// <summary>
        /// 轉換羅馬數字，目前最大值3999
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string GetNumberToRomanNumerals(int number)
        {
            if ((number < 0) || (number > 3999)) throw new ArgumentOutOfRangeException("insert value Must be betwheen 1 and 3999");

            _txtBuilder.Clear();
            while (number > 0)
            {
                if (number >= 1000)
                {
                    _txtBuilder.Append("M"); number -= 1000;
                }
                else if (number >= 900)
                {
                    _txtBuilder.Append("CM"); number -= 900;
                }
                else if (number >= 500)
                {
                    _txtBuilder.Append("D"); number -= 500;
                }
                else if (number >= 400)
                {
                    _txtBuilder.Append("CD"); number -= 400;
                }
                else if (number >= 100)
                {
                    _txtBuilder.Append("C"); number -= 100;
                }
                else if (number >= 90)
                {
                    _txtBuilder.Append("XC"); number -= 90;
                }
                else if (number >= 50)
                {
                    _txtBuilder.Append("L"); number -= 50;
                }
                else if (number >= 40)
                {
                    _txtBuilder.Append("XL"); number -= 40;
                }
                else if (number >= 10)
                {
                    _txtBuilder.Append("X"); number -= 10;
                }
                else if (number >= 9)
                {
                    _txtBuilder.Append("IX"); number -= 9;
                }
                else if (number >= 5)
                {
                    _txtBuilder.Append("V"); number -= 5;
                }
                else if (number >= 4)
                {
                    _txtBuilder.Append("IV"); number -= 4;
                }
                else if (number >= 1)
                {
                    _txtBuilder.Append("I"); number -= 1;
                }
            }

            return _txtBuilder.ToString();
        }

        private static Vector3[] _frustumCornersVector = new Vector3[4];
        public static Vector3[] GetFrustumCorners(UnityEngine.Camera camera)
        {
            var curCamera = camera;
            var leftTop = curCamera.ViewportPointToRay(new Vector3(0, 0));
            var rightTop = curCamera.ViewportPointToRay(new Vector3(1, 0));
            var leftBottom = curCamera.ViewportPointToRay(new Vector3(0, 1));
            var rightBottom = curCamera.ViewportPointToRay(new Vector3(1, 1));

            var leftTopAtY0 = GetPositionByRayAtY(camera, leftTop, 0);
            var rightTopAtY0 = GetPositionByRayAtY(camera, rightTop, 0);
            var leftBottomAtY0 = GetPositionByRayAtY(camera, leftBottom, 0);
            var rightBottomAtY0 = GetPositionByRayAtY(camera, rightBottom, 0);
            _frustumCornersVector[0] = leftTopAtY0;
            _frustumCornersVector[1] = leftBottomAtY0;
            _frustumCornersVector[2] = rightTopAtY0;
            _frustumCornersVector[3] = rightBottomAtY0;
            return _frustumCornersVector;
        }
        public static Vector3 GetPositionByRayAtY(UnityEngine.Camera camera, Ray ray, float y)
        {
            var cameraPos = camera.transform.position;
            Vector3 dir = ray.direction;
            Vector3 outPos = new Vector3(
                (y - cameraPos.y) / dir.y * dir.x + cameraPos.x,
                y,
                (y - cameraPos.y) / dir.y * dir.z + cameraPos.z);

            return outPos;
        }
        public static Vector3 ClosestPointToLine(Vector3 _lineStartPoint, Vector3 _lineEndPoint, Vector3 _testPoint)
        {
            Vector3 pointTowardStart = _testPoint - _lineStartPoint;
            Vector3 startTowardEnd = (_lineEndPoint - _lineStartPoint).normalized;

            float lengthOfLine = Vector3.Distance(_lineStartPoint, _lineEndPoint);
            float dotProduct = Vector3.Dot(startTowardEnd, pointTowardStart);

            if (dotProduct <= 0)
            {
                return _lineStartPoint;
            }

            if (dotProduct >= lengthOfLine)
            {
                return _lineEndPoint;
            }

            Vector3 thirdVector = startTowardEnd * dotProduct;

            Vector3 closestPointOnLine = _lineStartPoint + thirdVector;

            return closestPointOnLine;
        }

        public static Vector3 PointProjectToLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
        {
            Vector3 rhs = point - lineStart;
            Vector3 vector2 = lineEnd - lineStart;
            float magnitude = vector2.magnitude;
            Vector3 lhs = vector2;
            if (magnitude > 1E-06f)
            {
                lhs = (Vector3)(lhs / magnitude);
            }
            float num2 = Mathf.Clamp(Vector3.Dot(lhs, rhs), 0f, magnitude);
            return (lineStart + ((Vector3)(lhs * num2)));
        }

        public static float PointToLineDistance(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
        {
            return Vector3.Magnitude(PointProjectToLine(point, lineStart, lineEnd) - point);
        }
    }
}