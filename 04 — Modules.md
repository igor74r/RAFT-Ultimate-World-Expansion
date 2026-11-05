[Reading 98 lines from start (total: 98 lines, 0 remaining)]

---
title: Modules
tags: [raft, modules]
---

# Modules

| Модуль | Назначение | Приоритет | Статус |
|---|---|---:|---|
| Core | Инициализация, конфигурация, логирование | P0 | План |
| Items | Предметы и ресурсы | P1 | План |
| Crafting | Рецепты и станции | P1 | План |
| Building | Строительные элементы | P1 | План |
| World | Биомы и локации | P2 | План |
| Underwater | Подводное исследование | P2 | План |
| Creatures | Животные и AI | P2 | План |
| Technology | Технологии и энергетика | P2 | План |
| Automation | Производственные цепочки | P3 | План |
| Survival | Выживание и медицина | P3 | План |
| Transport | Транспорт | P3 | План |
| NPC / Economy | Поселения и торговля | P3 | План |
| Quests | Сюжет и задания | P3 | План |
| Multiplayer | Сетевая синхронизация | P0 | Исследование |

## Карточка нового модуля
- Название:
- Ответственность:
- Зависимости:
- Игровые API:
- Файлы:
- Тесты:
- Ограничения:


## World module status update — 2026-11-06
| Subsystem | Responsibility | Status | Evidence / blocker |
|---|---|---|---|
| World.Core | Discover world managers and expose safe diagnostics | Research | Runtime probe not yet written |
| World.Chunks | Inspect chunk points, rules and spawners | API mapped | ChunkManager, ChunkPoint, SO_ChunkSpawnRuleAsset found locally |
| World.Landmarks | Load custom landmark scenes | Research | SceneLoader and AssetBundle pipeline found; custom registration unresolved |
| World.Persistence | Stable landmark/point identity and save migration | Research | Vanilla RGD_World/RGD_ChunkPoint structures found; custom payload unverified |
| World.Network | Host-authoritative spawn and client replication | Research | Vanilla network messages exist; custom multiplayer untested |
| World.Biomes | Region/biome rules | Planned | No custom biome API confirmed |

World module is not implemented. Do not mark island generation complete based on API discovery alone.


## World module status update — 2026-11-06 (probe staged)

| Subsystem | Responsibility | Status | Evidence / blocker |
|---|---|---|---|
| World.Core | Discover world managers and expose safe diagnostics | Implemented; runtime pending | Read-only reflection probe compiled and packaged in v0.1.1 |
| World.Chunks | Inspect chunk points, rules and spawners | API mapped; runtime probe pending | ChunkManager fields are queried read-only; no mutation |
| World.Landmarks | Load custom landmark scenes | Research | Custom AssetBundle registration unresolved |
| World.Persistence | Stable landmark/point identity and save migration | Research | Custom payload and migration unverified |
| World.Network | Host-authoritative spawn and client replication | Research | Custom multiplayer untested |
| World.Biomes | Region/biome rules | Planned | No custom biome API confirmed |


## Runtime probe result — 2026-11-06
| Subsystem | Updated status | Runtime evidence / next step |
|---|---|---|
| World.Core | Runtime diagnostic PASS | v0.1.1 probe completed in F10 |
| World.Chunks | Runtime counts observed; structure mapping pending | 17 spawners, 9 chunks, 3 ChunkPointType dictionary entries; inspect keys and bounded entry summaries |
| World.Landmarks | Metadata inventory observed | SceneLoader.loadedLandmarks count 38; spawned-instance count not measured |
| World.AssetBundles | Type discovery only | Manager/Hooker types resolve, but no active-scene component instances found; inspect ownership and static/instance API |
| World.Persistence | Research | No custom payload or migration test |
| World.Network | Research | No custom host-authoritative spawn/replication test |
| World.Biomes | Planned | No custom biome API confirmed |

The values above are diagnostic observations, not proof that custom islands can yet be registered or generated.


## Runtime probe result — 2026-11-06
| Subsystem | Updated status | Evidence / next step |
|---|---|---|
| World.Core | Runtime diagnostic PASS | v0.1.1 probe completed in F10 |
| World.Chunks | Runtime counts observed; mapping pending | 17 spawners, 9 chunks, 3 ChunkPointType dictionary entries; inspect keys and bounded summaries |
| World.Landmarks | Metadata inventory observed | loadedLandmarks count 38; spawned count not measured |
| World.AssetBundles | Type discovery only | Types resolve, but no active-scene component instances found; inspect ownership/API |
| World.Persistence | Research | Custom payload/migration untested |
| World.Network | Research | Custom host-authority/replication untested |

These observations do not prove custom island registration or generation.


## World module v0.1.2 update — 2026-11-06
| Subsystem | Status | Evidence / next action |
|---|---|---|
| World.Core | Candidate built; preflight compile PASS | Bounded read-only reflection diagnostics; in-game test pending |
| World.Chunks | Runtime inventory pass pending | Next output will enumerate up to 8 point-type keys, 12 points/key, 20 spawners |
| World.Landmarks | Metadata and type surface research | AssetBundle/SceneLoader methods are listed, not invoked |
| World.AssetBundles | Metadata-only API inspection added | Need runtime output to determine static/instance ownership and actual fields |
| World.Persistence | Research | No custom save data or migration implemented |
| World.Network | Research | No custom host-authority or replication implemented |
| World.Biomes | Planned | No custom biome API confirmed |

v0.1.2 is a diagnostic candidate, not custom world generation.

[executed on device: DESKTOP-3MKBODB (8088b63f-2a05-4f71-a7a4-ad740486ff11)]