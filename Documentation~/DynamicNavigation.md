# Dynamic navigation 1.1

## Contracts

Capture `map.CaptureSnapshot()` on Unity's thread after occupancy/terrain changes, then call `snapshot.FindPath` or `snapshot.Reachable` on a worker. Search state is local; the captured state does not mutate when the live map changes. Published path nodes are detached copies. Surface references are metadata; do not call Unity APIs from workers.

`PathResult` supplies `Status`, `TotalCost`, `Version` and a legacy-compatible `Path` including its zero-cost start marker. `Success` on the same supported cell has one marker; failures have no edges. Cancellation of FindPath returns `Cancelled`; Reachable propagates cancellation. Consume only results for the intended map state and revalidate routes after terrain changes.

`GetPath`, `HierarchicalPathfinder` and the old InsertNodes/RemoveAddedNodes signatures remain available. InsertNodes prepares an independent route graph and no longer mutates the baked graph. Prefer QueryPath for new consumers. Standalone FindPathJPS preserves its historical start marker; all jump edges now contain their actual origin and full cost.

## Terrain and occupation

Use `surface.SetObstacle(worldPosition, blocked)`, `SetSupported(worldPosition, supported)` and `SetTraversalCost(worldPosition, multiplier)`; multipliers must be finite and at least 1. Wrap multi-cell edits in `using (map.BeginUpdate())` so snapshot capture sees the whole transaction. Support/cost overrides survive a surface rebake. An occupancy change does not recreate the lattice. For existing direct Node flag writers, use BeginUpdate and MarkChanged; snapshot capture also defensively detects flag differences.

Deleting support forbids movement through that cell and disables transitions with unsupported endpoints. The application remains responsible for terrain meshes/colliders, object damage, units falling when their support disappears, save data and ensuring an authored transition has physical clearance. This package provides navigation topology, not destruction physics.

The standard Surface can automatically track its listed obstacle transforms. One transform marks one cell; applications with larger objects must update their complete footprint. Actor occupancy should use its own application layer, rather than changing every cell's persistent terrain configuration.

## Regions and links

Each SurfaceBase is a region. Author several regions to bound invalidation; automatic partitioning is not performed in this release. Unchanged region snapshots and their local portal routes remain cached. Changes rebuild the affected region lazily. The abstract graph is assembled per query so opening a passage can improve a route even if the passage was outside its old route.

A Link must be activated and have supported, traversable endpoints. `bidirectional` defaults to true for existing scenes. `costModifier` remains an absolute non-negative traversal cost by default. Set `useDistanceCost` explicitly to interpret it as a per-world-unit multiplier instead. `waypoints` are optional world-space intermediate points for stairs and landings. A zero heuristic is used globally because legacy absolute costs can be cheaper than geometric distance.

Refresh moved endpoint transforms with UpdateSurfacesLinkNodes on Unity's thread; LateUpdate also tracks endpoint motion. Invalid/missing endpoint transforms disable an active authored link. Link setting changes are detected during snapshot capture. Local JPS never traverses special links: it is restricted to each horizontal grid.

Surfaces retain their actual transform height, including fractional heights; X/Z snap relative to the region origin. cellSize defaults to 1. Node.Id identifies a surface/cell; generated surface IDs are runtime IDs unless explicitly serialized by an authoring workflow. Overlapping floors at different heights remain separate. Query positions must be within 0.35 world units of the intended floor. This release does not rasterize arbitrary sloped meshes automatically.

## Solver selection and performance

Uniform local regions use online JPS with strict no-corner-cut rules. Weighted/custom-cost regions use Dijkstra (A* with h=0). High-level graph search also uses h=0. Full reachable ranges use one bounded Dijkstra, rather than one path request per candidate cell. No JPS+ or goal-bounding database is built. Consider those only after measuring topology update frequency and query amortization.

## Testing

`dotnet run --project 'Tests~/Core/CoreTests.csproj' -c Release`

The headless test shim covers managed navigation logic and is ignored by Unity. It is not a physics/agent-controller test. Game integration should additionally test actual scenes, multilevel routes, moving blockers, destruction and action-point costs.
