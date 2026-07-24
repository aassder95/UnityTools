using System;
using System.Globalization;
using UnityTools.Timer.Persistence;

namespace UnityTools.Timer.Editor
{
    public class StorageDebugValueReader
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly IStorage _storage;

        //============================================================
        // Constructors
        //============================================================
        public StorageDebugValueReader(IStorage storage)
        {
            _storage = storage;
        }

        //============================================================
        // Logic
        //============================================================
        public string ReadDateKey(string key)
        {
            if (!StorageValueUtils.TryHasKey(_storage, key, out bool hasKey))
                return "(조회 실패)";
            if (!hasKey)
                return "(없음)";

            if (!StorageValueUtils.TryLoadStringOrDefault(_storage, key, out string raw))
                return "(로드 실패)";

            if (!long.TryParse(raw, out long ticks))
                return $"잘못된 ticks 값: {raw}";

            if (ticks == DateTime.MinValue.Ticks)
                return $"{raw} (DateTime.MinValue)";
            if (ticks < DateTime.MinValue.Ticks || ticks > DateTime.MaxValue.Ticks)
                return $"범위 초과 ticks: {raw}";

            DateTime time = new DateTime(ticks, DateTimeKind.Utc);
            return $"{raw} ({time:yyyy-MM-dd HH:mm:ss} UTC)";
        }

        public string ReadDoubleKey(string key, string unit = "")
        {
            if (!StorageValueUtils.TryHasKey(_storage, key, out bool hasKey))
                return "(조회 실패)";
            if (!hasKey)
                return "(없음)";

            if (!StorageValueUtils.TryLoadStringOrDefault(_storage, key, out string raw))
                return "(로드 실패)";

            bool isParsed = double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out double value) || double.TryParse(raw, NumberStyles.Float, CultureInfo.CurrentCulture, out value);
            if (!isParsed)
                return $"잘못된 값: {raw}";

            return string.IsNullOrWhiteSpace(unit) ? $"{raw} ({value:F2})" : $"{raw} ({value:F2} {unit})";
        }

        public string ReadEnumKey<TEnum>(string key) where TEnum : struct, Enum
        {
            if (!StorageValueUtils.TryHasKey(_storage, key, out bool hasKey))
                return "(조회 실패)";
            if (!hasKey)
                return "(없음)";

            if (!StorageValueUtils.TryLoadStringOrDefault(_storage, key, out string raw))
                return "(로드 실패)";

            if (!int.TryParse(raw, out int intValue))
                return $"잘못된 state 값: {raw}";

            TEnum type = (TEnum)Enum.ToObject(typeof(TEnum), intValue);
            return Enum.IsDefined(typeof(TEnum), type) ? $"{raw} ({type})" : $"{raw} (정의되지 않은 상태)";
        }

        public string ReadFlagKey(string key, string trueRaw = "1", string trueText = "참", string falseText = "거짓")
        {
            if (!StorageValueUtils.TryHasKey(_storage, key, out bool hasKey))
                return "(조회 실패)";
            if (!hasKey)
                return "(없음)";

            if (!StorageValueUtils.TryLoadStringOrDefault(_storage, key, out string raw))
                return "(로드 실패)";

            return raw == trueRaw ? $"{raw} ({trueText})" : $"{raw} ({falseText})";
        }
    }
}
