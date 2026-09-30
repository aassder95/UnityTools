using System.Collections.Generic;

namespace UnityTools.Benchmark.Samples
{
    public class UiLabScenario
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly List<string> _rows;
        private readonly string[] _insertedRows;
        private readonly int _baseItemCnt;
        private readonly int _itemStep;
        private readonly int _mutationIntervalFrames;
        private readonly EUiLabScenario _mode;

        //============================================================
        // Fields
        //============================================================
        private uint _randomState;
        private int _frameIdx;
        private int _targetIdx;
        private int _mutationDelta;
        private bool _hasInserted;

        //============================================================
        // Properties
        //============================================================
        public int ItemCnt => _rows.Count;
        public int TargetIdx => _targetIdx;
        public int MutationDelta => _mutationDelta;

        //============================================================
        // Constructors
        //============================================================
        private UiLabScenario(int itemCnt, int seed, EUiLabScenario mode, int itemStep, int mutationCnt, int mutationIntervalFrames)
        {
            _baseItemCnt = itemCnt;
            _mode = mode;
            _itemStep = itemStep;
            _mutationIntervalFrames = mutationIntervalFrames;
            _randomState = unchecked((uint)seed) ^ 0x9E3779B9u;
            if (_randomState == 0)
                _randomState = 1;

            _rows = new List<string>(itemCnt + mutationCnt);
            for (int idx = 0; idx < itemCnt; ++idx)
            {
                _rows.Add("ITEM " + idx.ToString("D6") + "   /   " + NextRandom().ToString("X8"));
            }

            _insertedRows = new string[mutationCnt];
            for (int idx = 0; idx < mutationCnt; ++idx)
            {
                _insertedRows[idx] = "INSERTED " + idx.ToString("D4");
            }
        }

        //============================================================
        // Logic
        //============================================================
        public static bool TryCreate(int itemCnt, int seed, EUiLabScenario mode, int itemStep, int mutationCnt, int mutationIntervalFrames, out UiLabScenario scenario)
        {
            scenario = null;
            if (itemCnt <= 0 || itemCnt > 100000 || itemStep <= 0 || mutationCnt <= 0 || mutationCnt > itemCnt || mutationIntervalFrames <= 0 || mode < EUiLabScenario.Sweep || mode > EUiLabScenario.InsertRemove)
                return false;

            scenario = new UiLabScenario(itemCnt, seed, mode, itemStep, mutationCnt, mutationIntervalFrames);
            return true;
        }

        public string RowAt(int idx) => _rows[idx];

        public void Advance()
        {
            _mutationDelta = 0;
            if (_mode == EUiLabScenario.RandomJump)
            {
                _targetIdx = (int)(NextRandom() % (uint)_rows.Count);
            }
            else if (_mode == EUiLabScenario.Sweep)
            {
                long cycle = System.Math.Max(1, (_baseItemCnt - 1) * 2L);
                long pos = _frameIdx * (long)_itemStep % cycle;
                _targetIdx = (int)(pos < _baseItemCnt ? pos : cycle - pos);
            }
            else
            {
                _targetIdx = _baseItemCnt / 2 + (_hasInserted ? _insertedRows.Length : 0);
                if (_frameIdx > 0 && _frameIdx % _mutationIntervalFrames == 0)
                {
                    if (_hasInserted)
                    {
                        _rows.RemoveRange(0, _insertedRows.Length);
                        _mutationDelta = -_insertedRows.Length;
                    }
                    else
                    {
                        _rows.InsertRange(0, _insertedRows);
                        _mutationDelta = _insertedRows.Length;
                    }

                    _hasInserted = !_hasInserted;
                }
            }

            ++_frameIdx;
        }

        //============================================================
        // Utilities
        //============================================================
        private uint NextRandom()
        {
            _randomState ^= _randomState << 13;
            _randomState ^= _randomState >> 17;
            _randomState ^= _randomState << 5;
            return _randomState;
        }
    }
}
