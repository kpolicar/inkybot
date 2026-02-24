"""
Profiler insights — reads logs/profiler.jsonl and prints summary stats + optional plots.

Usage:
    python profiler_insights.py [--log PATH] [--plot] [--last N]

    --log PATH   path to profiler.jsonl  (default: logs/profiler.jsonl)
    --plot       show time-series plots with matplotlib
    --last N     only analyse the last N ticks
"""

import argparse
import json
import sys
from pathlib import Path

import pandas as pd


def load(path: Path) -> pd.DataFrame:
    rows = []
    with path.open() as f:
        for line in f:
            line = line.strip()
            if line:
                rows.append(json.loads(line))
    if not rows:
        sys.exit(f"No data found in {path}")
    df = pd.DataFrame(rows)
    df["ts"] = pd.to_datetime(df["ts"], unit="ms", utc=True)
    df = df.set_index("ts").sort_index()
    return df


def summary(df: pd.DataFrame) -> None:
    metrics = [c for c in df.columns]
    stats = df[metrics].agg(["mean", "median", lambda x: x.quantile(0.95), lambda x: x.quantile(0.99), "max", "std"])
    stats.index = ["mean", "median", "p95", "p99", "max", "std"]
    stats = stats.round(1).T.sort_values("p95", ascending=False)

    print("\n=== Per-metric summary (ms) ===")
    print(stats.to_string())


def slow_ticks(df: pd.DataFrame, top_n: int = 10) -> None:
    # Use the highest-level "total" column as the overall tick cost proxy
    total_cols = [c for c in df.columns if c.endswith(".total")]
    if not total_cols:
        return

    # Pick the column with the highest max as the primary total
    primary = df[total_cols].max().idxmax()

    top = df.nlargest(top_n, primary)
    print(f"\n=== Slowest {top_n} ticks by [{primary}] ===")
    print(top[total_cols].to_string())


def correlations(df: pd.DataFrame) -> None:
    total_cols = [c for c in df.columns if c.endswith(".total")]
    if len(total_cols) < 2:
        return
    corr = df[total_cols].corr().round(2)
    print("\n=== Correlations between totals ===")
    print(corr.to_string())


def breakdown(df: pd.DataFrame) -> None:
    """Show how much each sub-step contributes to its section's total on average."""
    sections: dict[str, list[str]] = {}
    for col in df.columns:
        if "." in col:
            sec = col.split(".")[0]
            sections.setdefault(sec, []).append(col)

    print("\n=== Average breakdown per section (ms) ===")
    for sec, cols in sorted(sections.items()):
        total_col = next((c for c in cols if c.endswith(".total")), None)
        if total_col is None:
            continue
        total_mean = df[total_col].mean()
        if total_mean == 0:
            continue
        print(f"  [{sec}]  avg total: {total_mean:.1f} ms")
        sub = [(c, df[c].mean()) for c in cols if c != total_col]
        sub.sort(key=lambda x: -x[1])
        for col, mean in sub:
            pct = 100 * mean / total_mean
            label = col[len(sec) + 1:]
            print(f"    {label:<40} {mean:7.1f} ms  ({pct:5.1f}%)")


def plot(df: pd.DataFrame) -> None:
    try:
        import matplotlib.pyplot as plt
    except ImportError:
        print("\n[plot] matplotlib not installed — skipping plots")
        return

    total_cols = [c for c in df.columns if c.endswith(".total")]
    if not total_cols:
        print("[plot] No *.total columns to plot")
        return

    fig, axes = plt.subplots(len(total_cols), 1, figsize=(12, 3 * len(total_cols)), sharex=True)
    if len(total_cols) == 1:
        axes = [axes]

    for ax, col in zip(axes, total_cols):
        ax.plot(df.index, df[col], linewidth=0.8, alpha=0.7, label=col)
        ax.axhline(df[col].median(), linestyle="--", color="orange", linewidth=1, label="median")
        ax.axhline(df[col].quantile(0.95), linestyle=":", color="red", linewidth=1, label="p95")
        ax.set_ylabel("ms")
        ax.set_title(col)
        ax.legend(fontsize=8)

    axes[-1].set_xlabel("time")
    fig.suptitle("Profiler totals over time", fontsize=13)
    plt.tight_layout()
    plt.show()


def main() -> None:
    parser = argparse.ArgumentParser(description="Analyse profiler.jsonl")
    parser.add_argument("--log", default="logs/profiler.jsonl", help="path to profiler.jsonl")
    parser.add_argument("--plot", action="store_true", help="show matplotlib time-series plots")
    parser.add_argument("--last", type=int, default=0, metavar="N", help="only use last N ticks")
    args = parser.parse_args()

    path = Path(args.log)
    if not path.exists():
        sys.exit(f"File not found: {path}")

    df = load(path)
    if args.last > 0:
        df = df.iloc[-args.last:]

    print(f"Loaded {len(df)} ticks from {path}")
    print(f"Time range: {df.index[0]}  →  {df.index[-1]}")

    summary(df)
    breakdown(df)
    slow_ticks(df)
    correlations(df)

    if args.plot:
        plot(df)


if __name__ == "__main__":
    main()
