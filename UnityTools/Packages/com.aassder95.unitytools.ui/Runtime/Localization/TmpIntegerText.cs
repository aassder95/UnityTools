using TMPro;

namespace UnityTools.Ui.Localization
{
    public static class TmpIntegerText
    {
        //============================================================
        // Logic
        //============================================================
        public static bool TryWrite(TMP_Text text, long value, char[] buffer, string prefix = "", string suffix = "")
        {
            if (text == null || !TryFormat(value, buffer, prefix, suffix, out int length))
                return false;

            text.SetCharArray(buffer, 0, length);
            return true;
        }

        public static bool TryFormat(long value, char[] buffer, string prefix, string suffix, out int length)
        {
            length = 0;
            if (buffer == null || prefix == null || suffix == null)
                return false;

            bool isNegative = value < 0;
            ulong magnitude = isNegative ? (ulong)(-(value + 1)) + 1UL : (ulong)value;
            int digitCnt = 1;
            for (ulong remaining = magnitude; remaining >= 10UL; remaining /= 10UL)
            {
                digitCnt++;
            }

            long required = (long)prefix.Length + suffix.Length + digitCnt + (isNegative ? 1 : 0);
            if (required > buffer.Length)
                return false;

            length = (int)required;
            prefix.CopyTo(0, buffer, 0, prefix.Length);
            int firstDigit = prefix.Length;
            if (isNegative)
                buffer[firstDigit++] = '-';

            int endIdx = firstDigit + digitCnt;
            for (int idx = endIdx - 1; idx >= firstDigit; idx--)
            {
                buffer[idx] = (char)('0' + magnitude % 10UL);
                magnitude /= 10UL;
            }

            suffix.CopyTo(0, buffer, endIdx, suffix.Length);
            return true;
        }
    }
}
