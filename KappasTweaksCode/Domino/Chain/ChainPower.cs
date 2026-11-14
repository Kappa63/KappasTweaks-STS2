using KappasTweaks.KappasTweaksCode.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace KappasTweaks.KappasTweaksCode.Domino;

// Holds a player's open ends and how many dominoes in a row have Chained. Lives for the combat, so ends persist
// between turns.
public sealed class ChainPower : KappasTweaksPower
{
    public const int Blank = 0;
    public const int MaxPip = 6;
    private const string _endsKey = "Ends";
    private const string _linksKey = "Links";

    public static event Action<Player>? Changed;

    private int _left = -1;
    private int _right = -1;
    private int _links;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override int DisplayAmount => _links;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new StringVar(_endsKey, "-"), new IntVar(_linksKey, 0)];

    public bool HasEnds => _left >= 0;
    public int Left => _left;
    public int Right => _right;
    public int Links => _links;

    public bool CanChain(int a, int b) => HasEnds && FindLink(a, b) != null;

    // Returns whether the domino Chained. With no ends yet it just starts the chain.
    public bool Place(int a, int b)
    {
        if (!HasEnds)
        {
            SetEnds(a, b);
            return false;
        }

        if (FindLink(a, b) is not var (leftEnd, other))
        {
            _links = 0;
            SetEnds(a, b);
            return false;
        }

        if (leftEnd) _left = other;
        else _right = other;
        _links++;
        Refresh();
        return true;
    }

    public void SetEnds(int left, int right)
    {
        _left = left;
        _right = right;
        Refresh();
    }

    // Exact matches win over blanks, so a wild end isn't spent when a real one fits.
    private (bool LeftEnd, int Other)? FindLink(int a, int b)
    {
        foreach (bool exact in new[] { true, false })
        foreach (bool leftEnd in new[] { true, false })
        {
            int end = leftEnd ? _left : _right;
            if (Fits(end, a, exact)) return (leftEnd, b);
            if (Fits(end, b, exact)) return (leftEnd, a);
        }
        return null;
    }

    private static bool Fits(int end, int pip, bool exact) =>
        exact ? end == pip : end == Blank || pip == Blank;

    private void Refresh()
    {
        ((StringVar)DynamicVars[_endsKey]).StringValue = HasEnds ? $"{_left} | {_right}" : "-";
        DynamicVars[_linksKey].BaseValue = _links;
        InvokeDisplayAmountChanged();
        if (Owner?.Player is { } player)
            Changed?.Invoke(player);
    }
}
