[Reading 89 lines from start (total: 89 lines, 0 remaining)]

---
title: RML API Map
tags: [raft, research, rml, api]
status: research
---

# RML API Map

## Цель
Собрать подтверждённые точки входа RML для минимального ядра и будущих игровых модулей. Это карта документации, а не готовая реализация и не гарантия совместимости с локальной установкой.

## Источники и актуальность
- RML Loader: https://www.raftmodding.com/loader/ — опубликованный Launcher/RML 2.8.10, целевая игра Raft Update 1.1, дата релиза 2026-03-23.
- RML 2.8.10 changelog: https://www.raftmodding.com/loader/2.8.10 — исправление проблемы Events API; core bumped to 2.8.10.
- Mod API: https://api.raftmodding.com/modding-api/mod — базовый класс `HMLLibrary.Mod`, информация мода, логирование и lifecycle callbacks. Страница при поиске показывала обновление около 7 месяцев назад.
- RAPI: https://api.raftmodding.com/modding-api/rapi — обзор вспомогательных методов, включая регистрацию предметов и сетевые сообщения. Страница помечена как обновлённая около 8 месяцев назад; часть сигнатур открывается через интерактивные Method/Example, поэтому их нельзя переносить в код без отдельной сверки.
- Mod project tutorial: https://api.raftmodding.com/modding-tutorials/how-to-create-a-mod-project — старое руководство по Mod Creator и Unity 2019.3.5f1 для описанного project/asset workflow.

## Подтверждено документацией

### Базовый класс `HMLLibrary.Mod`
- `GetModInfo()` возвращает `JsonModInfo` из `modinfo.json`.
- `Log(object)` пишет сообщение с префиксом имени мода.
- Документированы `WorldEvent_WorldLoaded`, `WorldEvent_WorldSaved`, `WorldEvent_WorldUnloaded`.
- Документированы локальные события игрока: получение урона, смерть, возрождение, создание предмета, подбор и выбрасывание предмета.
- Документированы события подключения/отключения игрока и загрузки/выгрузки модов.
- Примеры API показывают override методов, но точные сигнатуры каждого callback перед реализацией следует сверить с локальными references/шаблоном для фактически установленного RML.

### `RAPI`
Документация перечисляет функциональные области:
- проверка dedicated server и текущей сцены (main menu/game);
- доступ к локальному игроку;
- отправка сообщения в чат;
- выдача предметов локальному игроку;
- разрешение/запрет размещения предметов на типах block quad;
- регистрация и снятие регистрации предметов;
- установка in-hand prefab для предмета;
- отправка и приём сетевых сообщений по каналу.

**Важно:** список методов не равен подтверждённым сигнатурам. Пока нельзя писать вызовы RegisterItem, сетевого API или item prefab pipeline по догадке. Нужны Method details/Example и сверка с конкретной RML assembly.

## Предлагаемый минимальный вертикальный срез
1. Чистый проект, созданный официальным Mod Creator.
2. Единственный entry point, унаследованный от `HMLLibrary.Mod`.
3. При старте записать в лог имя и версию из `GetModInfo()`.
4. На world loaded/saved/unloaded писать диагностические события без изменения игрового состояния.
5. Проверить выгрузку/повторную загрузку, если RML и игровой режим это поддерживают.
6. Собрать `.rmod`, загрузить в отдельном тестовом мире и сохранить лог/скриншот результата.

Этот срез выбран потому, что опирается на документированные lifecycle/logging API и не требует заранее предполагать API регистрации предметов, рецептов, UI или сетевой синхронизации.

## Не подтверждено
- Точные сигнатуры и требования к регистрации предметов.
- Создание рецептов и станций.
- Добавление блоков/строительных объектов.
- Работа с AssetBundle и точная версия Unity Editor для текущего Raft Update 1.1.
- Формат и API сериализации собственных данных.
- Авторитетная модель и синхронизация новых сущностей в multiplayer.
- Совместимость локального Steam build ID `22312909` с текущим RML 2.8.10.
- Наличие RML 2.8.10/Mod Creator на рабочем компьютере.

## Метаданные и упаковка мода
Официальные страницы RaftModding:
- `modinfo.json` описывает поля `name`, `author`, `description`, `requiredByAllPlayers`, `version`, `license`, `icon`, `banner`, `gameVersion`, `updateUrl`, `isModPermanent`, `excludedFiles`; локальная Reflection-проверка подтвердила эти поля в `JsonModInfo`, а также `slug`.
- Руководство Mod Creator говорит, что проект создаётся через RML Launcher, папка проекта содержит `modinfo.json` и C# source, а build script формирует файл `.rmod`.
- Руководство по установке описывает ручную установку `.rmod` в `Raft\\mods` и проверку статуса `RUNNING...` в Mod Manager.

Создан draft `02 — Core\\RML-Project-Seed\\modinfo.json.template`. JSON успешно проверен через PowerShell `ConvertFrom-Json`. Это только metadata draft: Mod Creator не проверял его, формат архива/сборки не проверялся, файл не размещён в игровой папке. Значения `author` и `license` остаются placeholders.

## Локальная проверка API через .NET Reflection — 2026-11-06
Из установленного `Raft_Data\\Managed\\RaftModLoader_AssemblyLoader.dll` загружены экспортируемые типы и прочитаны публичные методы. Сборка сообщает identity `AssemblyLoader, Version=1.0.0.0`.

Подтверждено локально:
- Внутри этой сборки находятся `HMLLibrary.Mod`, `HMLLibrary.HAPI`, `JsonModInfo` и другие типы HML. Отдельная `HMLLibrary.dll` не требуется для ссылки на `HMLLibrary.Mod` в этой установке.
- `HMLLibrary.Mod` наследуется от `UnityEngine.MonoBehaviour`.
- `GetModInfo()` возвращает `JsonModInfo`; `JsonModInfo` содержит публичные поля `name` и `version`.
- `Log(System.Object)` доступен.
- Точные локальные сигнатуры: `WorldEvent_WorldLoaded()`, `WorldEvent_WorldSaved()`, `WorldEvent_WorldUnloaded()`. При компиляции override `WorldEvent_WorldUnloaded()` выдал warning CS0672: callback помечен Obsolete в локальной сборке; в минимальном Core он исключён.
- В локальной сборке подключение/отключение игрока имеет сигнатуры `WorldEvent_OnPlayerConnected(Network_UserId, RGD_Settings_Character)` и `WorldEvent_OnPlayerDisconnected(Network_UserId, DisconnectReason)`. Это отличается от старых примеров документации с `CSteamID`.
- Локальная сборка также объявляет `SendNetworkMessage`, `SendNetworkMessageToPlayer`, `OnNetworkMessage`, `SubscribeToNetworkChannel` и `UnsubscribeFromNetworkChannel`; эти методы пока не использовать без сетевого теста.

Ограничение: Reflection не смог перечислить все типы через `GetTypes()` из-за отсутствующих зависимостей в среде PowerShell, но `GetExportedTypes()` и чтение методов `HMLLibrary.Mod`/полей `JsonModInfo` прошли успешно. Это статическая проверка metadata, не запуск игры и не проверка работы callback.

## Решение
Не начинать игровые системы Items/World/Multiplayer до подтверждения чистого RML проекта и успешной сборки минимального Core. Следующий технический шаг — получить официальный Launcher/Mod Creator и build toolchain после разрешения пользователя на установку, затем сверить API и metadata по сгенерированному шаблону и локальным assemblies.


## World-generation API cross-reference — 2026-11-06
The local game assembly inspection is documented in [[World Generation API Audit]]. Key verified types include ChunkManager, ChunkPoint, SO_ChunkSpawnRuleAsset, SceneLoader, Landmark, AssetBundleManager, AssetBundleHooker, RGD_World and RGD_ChunkPoint. The compiled ChunkPointType enum has vanilla values only; no runtime registration API was found in the inspected surface. HMLLibrary.Mod.GetEmbeddedFileBytes(String) exists, but its use for loading a custom AssetBundle and registering Landmark scenes remains a hypothesis requiring a controlled proof.

[executed on device: DESKTOP-3MKBODB (8088b63f-2a05-4f71-a7a4-ad740486ff11)]