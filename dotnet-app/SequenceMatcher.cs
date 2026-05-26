namespace DocxDiffTool;

public record Opcode(string Tag, int I1, int I2, int J1, int J2);

public class SequenceMatcher<T> where T : notnull
{
    private readonly T[] _a;
    private readonly T[] _b;

    public SequenceMatcher(T[] a, T[] b) { _a = a; _b = b; }

    public double Ratio()
    {
        int matches = 0;
        foreach (var (_, _, len) in GetMatchingBlocks())
            matches += len;
        int total = _a.Length + _b.Length;
        return total == 0 ? 1.0 : (2.0 * matches) / total;
    }

    private List<(int aIndex, int bIndex, int length)> GetMatchingBlocks()
    {
        var result = new List<(int, int, int)>();
        FindBlocks(0, _a.Length, 0, _b.Length, result);
        result.Sort((x, y) => x.Item1.CompareTo(y.Item1));
        return result!;
    }

    private void FindBlocks(int alo, int ahi, int blo, int bhi, List<(int, int, int)> result)
    {
        var (i, j, len) = FindLongestMatch(alo, ahi, blo, bhi);
        if (len > 0)
        {
            if (alo < i && blo < j)
                FindBlocks(alo, i, blo, j, result);
            result.Add((i, j, len));
            if (i + len < ahi && j + len < bhi)
                FindBlocks(i + len, ahi, j + len, bhi, result);
        }
    }

    private (int ai, int bj, int len) FindLongestMatch(int alo, int ahi, int blo, int bhi)
    {
        int bestAi = alo, bestBj = blo, bestLen = 0;

        var bIndex = new Dictionary<T, List<int>>();
        for (int j = blo; j < bhi; j++)
        {
            if (!bIndex.ContainsKey(_b[j]))
                bIndex[_b[j]] = new List<int>();
            bIndex[_b[j]].Add(j);
        }

        for (int i = alo; i < ahi; i++)
        {
            if (!bIndex.TryGetValue(_a[i], out var positions)) continue;
            foreach (var j in positions)
            {
                if (i + bestLen >= ahi || j + bestLen >= bhi) continue;
                if (!EqualityComparer<T>.Default.Equals(_a[i + bestLen], _b[j + bestLen])) continue;
                int k = 0;
                while (i + k < ahi && j + k < bhi &&
                       EqualityComparer<T>.Default.Equals(_a[i + k], _b[j + k]))
                    k++;
                if (k > bestLen) { bestAi = i; bestBj = j; bestLen = k; }
            }
        }

        return (bestAi, bestBj, bestLen);
    }

    public List<Opcode> GetOpcodes()
    {
        var blocks = GetMatchingBlocks();
        blocks.Add((_a.Length, _b.Length, 0));

        var opcodes = new List<Opcode>();
        int i = 0, j = 0;

        foreach (var (ai, bj, len) in blocks)
        {
            if (i < ai && j < bj)
                opcodes.Add(new Opcode("replace", i, ai, j, bj));
            else if (i < ai)
                opcodes.Add(new Opcode("delete", i, ai, j, j));
            else if (j < bj)
                opcodes.Add(new Opcode("insert", i, i, j, bj));

            if (len > 0)
                opcodes.Add(new Opcode("equal", ai, ai + len, bj, bj + len));

            i = ai + len;
            j = bj + len;
        }

        return opcodes;
    }
}
