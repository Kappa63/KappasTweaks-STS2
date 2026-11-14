using KappasTweaks.KappasTweaksCode.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace KappasTweaks.KappasTweaksCode.Domino;

public enum ChainOutcome
{
    Started,
    Chained,
    Broke,
}

// Powers and relics implementing this hear about every domino their owner plays.
public interface IChainListener
{
    Task OnChained(PlayerChoiceContext context, Cards.BonesmanCard card) => Task.CompletedTask;
    Task OnChainBroken(PlayerChoiceContext context, Cards.BonesmanCard card) => Task.CompletedTask;
    Task OnDominoPlayed(PlayerChoiceContext context, Cards.BonesmanCard card) => Task.CompletedTask;
}

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
    private int _linksThisTurn;
    private int _breaks;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override int DisplayAmount => _links;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new StringVar(_endsKey, "-"), new IntVar(_linksKey, 0)];

    public bool HasEnds => _left >= 0;
    public int Left => _left;
    public int Right => _right;
    public int Links => _links;
    public int LinksThisTurn => _linksThisTurn;
    public int Breaks => _breaks;

    public bool CanChain(int a, int b, int tolerance = 0, bool always = false) =>
        HasEnds && (always || FindLink(a, b, tolerance) != null);

    // Which half of a domino (0 = left, 1 = right) would join the chain, or -1.
    public int LinkingHalf(int a, int b, int tolerance = 0, bool always = false)
    {
        if (!HasEnds) return -1;
        if (FindLink(a, b, tolerance) is var (_, other)) return other == b ? 0 : 1;
        return always ? 0 : -1;
    }

    public ChainOutcome Place(int a, int b, int tolerance = 0, bool always = false, bool keepLinks = false)
    {
        if (!HasEnds)
        {
            SetEnds(a, b);
            return ChainOutcome.Started;
        }

        (bool LeftEnd, int Other)? link = FindLink(a, b, tolerance) ?? (always ? (true, b) : null);
        if (link is not var (leftEnd, other))
        {
            if (!keepLinks) _links = 0;
            _breaks++;
            SetEnds(a, b);
            return ChainOutcome.Broke;
        }

        if (leftEnd) _left = other;
        else _right = other;
        _links++;
        _linksThisTurn++;
        Refresh();
        return ChainOutcome.Chained;
    }

    public void SetEnds(int left, int right)
    {
        _left = left;
        _right = right;
        Refresh();
    }

    public void AddLinks(int count)
    {
        _links += count;
        Refresh();
    }

    public void ResetLinks()
    {
        _links = 0;
        Refresh();
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner.Player)
            _linksThisTurn = 0;
        return Task.CompletedTask;
    }

    // Exact matches win over blanks, and blanks over near-misses, so the most flexible end is spent last.
    private (bool LeftEnd, int Other)? FindLink(int a, int b, int tolerance)
    {
        for (int pass = 0; pass < 3; pass++)
        foreach (bool leftEnd in new[] { true, false })
        {
            int end = leftEnd ? _left : _right;
            if (Fits(end, a, pass, tolerance)) return (leftEnd, b);
            if (Fits(end, b, pass, tolerance)) return (leftEnd, a);
        }
        return null;
    }

    private static bool Fits(int end, int pip, int pass, int tolerance) => pass switch
    {
        0 => end == pip,
        1 => end == Blank || pip == Blank,
        _ => Math.Abs(end - pip) <= tolerance,
    };

    private void Refresh()
    {
        ((StringVar)DynamicVars[_endsKey]).StringValue = HasEnds ? $"{_left} | {_right}" : "-";
        DynamicVars[_linksKey].BaseValue = _links;
        InvokeDisplayAmountChanged();
        if (Owner?.Player is { } player)
            Changed?.Invoke(player);
    }
}
