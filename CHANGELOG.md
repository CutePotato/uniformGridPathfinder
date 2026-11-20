# Changelog

## 1.1.0

- Correct online JPS with no corner cutting, iterative scans, best-cost relaxation and full-cost jump edges.
- Query-local captured graphs and cancellation; no shared temporary graph mutation.
- Correct path reversal; mixed-cost abstract search uses an admissible zero heuristic.
- Per-surface lazy local route caches, invalidated by terrain/occupancy edits; unchanged regions stay cached.
- Fractional floor heights, surface/cell identities, removed support and weighted terrain (A* fallback).
- Directed transitions with intermediate waypoints, absolute legacy cost or explicit distance-based cost.
- Single-pass bounded Dijkstra reachability over cells and allowed transitions.
- Explicit query status and topology version. Legacy GetPath/JPS start markers remain supported.
- Moving obstacle tracking on Surface, safe inactive/null links and snapshot-safe edit scopes.
- Headless regression suite, 5000 randomized oracle comparisons, 20000-cell corridor and CI.

JPS+ preprocessing is intentionally not introduced. Regions use online JPS; stable local portal routes are cached. Dynamic geometry never consults stale jump tables.
