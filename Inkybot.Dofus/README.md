#Important classes
[Dofus Maging AI](#DofusMagingAI-class)\
The **Dofus Maging AI** is an abstract class, which can be written
to replace the built-in Inkybot Maging AI.

[Item](#Item-class)\
The **Item** class represents a single in-game item. It contains item stats with all their
constraint and statuses.


# UserRune class
 The **UserRune** class represents the user's quantity of a certain rune.
 The quantity value is modified during the maging process.

## Syntax
```C#
public UserRune(
    Rune rune,
    int quantity=0
)
```

## Parameters
```rune```\
Type: Rune\
The type of rune that is represented.

```quantity```\
Type: int\
The number of runes the user has.


# StatConfig struct
 The **StatConfig** struct represents the AI's configuration the user is using during the maging process.
 It is a *readonly* struct. Whenever the user modifies his config, a new struct is instantiated.

## Syntax
```C#
public StatConfig(
    int? maxValueSmRuneCanHit=null,
    int? changeToPaRuneThreshold=null,
    int? maxValuePaRuneCanHit=null,
    int? changeToRaRuneThreshold=null,
    bool highSinkStat=false
)
```

## Parameters
```maxValueSmRuneCanHit```\
Type: int?\
The maximum value at which a rune of SM strength can still land on the stat.
If set to null, SM runes can always land.

```changeToPaRuneThreshold```\
Type: int?\
The lowest value at which a rune of PA strength should begin to be used.
If set to null, PA runes should not be used.

```maxValuePaRuneCanHit```\
Type: int?\
The maximum value at which a rune of PA strength can still land on the stat.
If set to null, PA runes can always land.

```changeToRaRuneThreshold```\
Type: int?\
The lowest value at which a rune of RA strength should begin to be used.
If set to null, RA runes should not be used.

```highSinkStat```\
Type: bool\
Determines whether or not the stat should be interpreted as a high-sink stat.

## Properties & Fields
```ShouldUsePaRunes```\
Type: bool\
Whether or not runes of PA strength should be used.

```ShouldUseRaRunes```\
Type: bool\
Whether or not runes of RA strength should be used.


# Stat class
 The **Stat** class represents a single stat that can be found on items.
 All the valid stats that can be found on Dofus items are initialized as
 static members to this class.
 Any Stat objects that are initialized outside these static Stat objects
 are marked as *unmageable*, meaning they should be ignored by the AI.
 Unmageable stats can be found on weapons, specifically weapon effects.

## Syntax
```C#
public Stat(
    string identifier
)
```

## Parameters
```identifier```\
Type: string\
A unique string identifier for the stat.

## Properties & Fields
```Maximum```\
Type: int\
The absolute maximum value of the stat, unconditional of the item.

```SinkValue```\
Type: float\
The amount of sink a single unit of the stat will consume.

```NegSinkValue```\
Type: float\
The amount of sink a single unit of the stat will consume when the current value of the stat is below 0.

```CanUsePaRunes```\
Type: bool\
Whether or not runes of PA strength can be used on the stat.

```CanUseRaRunes```\
Type: bool\
Whether or not runes of RA strength can be used on the stat.

```Mageable```\
Type: bool\
Whether or not the stat can be maged.

```StrongestRuneType```\
Type: RuneType\
The strongest rune strength that can be used on the stat.

```StrongestRune```\
Type: Rune\
The strongest rune that can be used on the stat.

```DisplayName```\
Type: string\
The representable display name of the stat.

```RuneName```\
Type: string\
The representable display name of the stat's rune.

```Config```\
Type: StatConfig\
The active stat configuration for the stat.


# Rune class
 The **Rune** class represents a single rune that can be used on items.

## Syntax
```C#
public Rune(
    Stat stat,
    RuneType type
)
```

## Parameters
```stat```\
Type: Stat\
The stat that the rune represents.

```type```\
Type: RuneType (enum)\
Possible values: Sm, Pa, Ra
The type (strength) of the rune.

## Properties & Fields
```Weaker```\
Type: Rune?\
A rune of the same type, but one strength lower.
If there is no weaker rune, the property returns null.

```IncreaseInValue```\
Type: int\
The amount the rune will increase.

```Sink```\
Type: float\
The amount of sink the rune will consume.

```DisplayName```\
Type: string\
The representable display name of the rune.


# MageHistoryRecord class
 The **MageHistoryRecord** class represents a single record within an item's mage history.
 These are created during the maging process.

## Syntax
```C#
public MageHistoryRecord(
    IEnumerable<StatChanged> changed,
    bool sinkChanged
)
```

## Parameters
```changed```\
Type: IEnumerable<StatChanged>\
An enumerable of all the stat changes that have occured in history record.

```sinkChanged```\
Type: bool\
Whether or not the item's sink has been modified by the history record.

## Properties & Fields
```ChangeInSink```\
Type: float\
The amount of sink that has been changed by the history record.

```ChangeInSinkFromFallen```\
Type: float\
The amount of sink that has been decreased by the fallen stats in the history record.

```Landed```\
Type: StatChanged?\
The stat that has landed in the history record. If no stat had landed, the value is null.

```Fell```\
Type: StatChanged[]\
The stats that have been modified as a result of the history record.
If the history record resulted in a failure, this array is empty.


# MageConfig class
 The **MageConfig** class represents an item's mage configuration.
 Every stat on the item is mapped to a **ItemStatMageConfig**, which
 is taken into account by the AI in determining what stat to mage next.

## Syntax
```C#
public MageHistoryRecord(
    Item item
)
```

## Parameters
```item```\
Type: Item\
The item that the MageConfig is configuring.

## Properties & Fields
```ConfigManager```\
Type: MageConfigProvider\
A config manager instance providing configuration details regarding the maging process.

```RestoreHighSinkStatsImmediately```\
Type: bool\
A configuration detail provided by the ConfigManager which determines whether the AI
should prioritize the restoration of high sink stats immediately.

```Exos```\
Type: Dictionary<Stat, ItemStatMageConfig>\
A dictionary of configured exo stats on the item.

## Methods
```C#
public bool IsConfiguredFor(
    Item item
)
```
Determines whether or not the configuration is applicable to another item.


# ItemStatMageConfig struct
 The **ItemStatMageConfig** struct represents a single item's stat mage configuration.
 It is a *readonly* struct. Whenever the user modifies his config, a new struct is instantiated.

## Syntax
```C#
public ItemStatMageConfig(
    Stat stat,
    int minimum,
    int maximum,
    int? target,
    int? targetMinimum
)
```

## Parameters
```stat```\
Type: Stat\
The item stat that the instance is configuring.

```minimum```\
Type: int\
The minimum value that the item has on the stat.

```maximum```\
Type: int\
The maximum value that the item has on the stat.

```target```\
Type: int?\
The target value that is configured for the item stat.

```targetMinimum```\
Type: int?\
The target value minimum that is configured for the item stat.

## Properties
```Exo```\
Type: bool\
Whether or not the instance represents an exotic stat mage.

```Overmage```\
Type: bool\
Whether or not the instance represents a stat overmage.

```statConfig```\
Type: StatConfig\
The active stat configuration that is being used on the item mage.

```MaxValueSmRuneCanHit```\
Type: int?\
The maximum value at which a rune of SM strength can still land on the stat.
If set to null, SM runes can always land.
This value is taken from the statConfig.

```ChangeToPaRuneThreshold```\
Type: int?\
The lowest value at which a rune of PA strength should begin to be used.
If set to null, PA runes should not be used.
This value is taken from the statConfig.


```MaxValuePaRuneCanHit```\
Type: int?\
The maximum value at which a rune of PA strength can still land on the stat.
If set to null, PA runes can always land.
This value is taken from the statConfig.

```ChangeToRaRuneThreshold```\
Type: int?\
The lowest value at which a rune of RA strength should begin to be used.
If set to null, RA runes should not be used.
This value is taken from the statConfig.

```HighSinkStat```\
Type: bool\
Determines whether or not the stat should be interpreted as a high-sink stat.
This value is taken from the statConfig.

```ShouldUsePaRunes```\
Type: bool\
Whether or not runes of PA strength should be used.
This value is taken from the statConfig.

```ShouldUseRaRunes```\
Type: bool\
Whether or not runes of RA strength should be used.
This value is taken from the statConfig.

## Methods
```C#
public bool IsApplicableTo(
    ItemStat itemStat
)
```
Determines whether or not the configuration is applicable to another item stat.
```C#
public bool Clone(
    int? target,
    int? targetMinimum,
    Stat? stat = null,
    int? minimum=null,
    int? maximum=null
)
```
Make a copy of the instance, specifying values that should be modified.


# ItemStat class
The **ItemStat** class represents a single item's stat.
Instance equality is determined by comparing the *Stat*, *Min* and *Max*.

## Syntax
```C#
public ItemStat(
    Stat stat,
    int value,
    int min,
    int max
)
```

## Parameters
```stat```\
Type: Stat\
The stat that the instance represents.

```value```\
Type: int\
The current value of the stat on the item.

```min```\
Type: int\
The minimum value of the stat on the item.

```max```\
Type: int\
The maximum value of the stat on the item.

## Syntax
```C#
public ItemStat(
    string statIdentifier,
    int value,
    int min
    int max
 )
```

## Parameters
```statIdentifier```\
Type: string\
The identifier of the stat that the instance represents.

## Properties & Fields
```Exo```\
Type: bool\
Whether or not the stat on the item is exotically maged.

```Oversink```\
Type: float\
The amount of oversink of the stat on the item.



# IAction interface
The **IAction** interface represents a single action the bot can execute.

## Methods
```C#
void Execute()
```
Executes the action.




# ItemStatRepository class
The **ItemStatRepository** class represents a collection of an item's stats with helper methods
for filtering.

## Syntax
```C#
public ItemStatRepository(
    ItemStat[] stats
)
```

## Parameters
```stats```\
Type: ItemStat[]\
An array of item stats that the repository represents.

## Properties & Fields
```MageableStats```\
Type: ItemStat[]\
Subset of item stats that are considered mageable.

```UnmageableStats```\
Type: ItemStat[]\
Subset of item stats that are considered unmageable.

```StandardStats```\
Type: ItemStat[]\
Subset of item stats that are considered to be standard to the item.

```ExoStats```\
Type: ItemStat[]\
Subset of item stats that are considered to be exotic to the item.

```Length```\
Type: int\
The number of item stats the repository contains.




# Item class
The **Item** class represents a single item.
Instance equality is determined by comparing stats.

## Syntax
```C#
public Item(
    ItemStatRepository stats
)
```

## Parameters
```stats```\
Type: ItemStatRepository\
The current stats on the item.

## Properties & Fields
```IsValid```\
Type: bool\
Whether or not the item is considered to be a valid game item, passing
the game rules.

```IsInvalid```\
Type: bool\
Whether or not the item is considered to be an invalid game item, not passing
all the game rules.

```IsOvermaged```\
Type: bool\
Whether or not the item has at least one overmaged stat.

```HasExo```\
Type: bool\
Whether or not the item has at least one exotically maged item stat.

```Oversink```\
Type: float\
The amount of oversink on the item.

## Methods
```C#
public bool HasStat(
    Stat stat
)
```
Determines whether or not the item has a stat.
```C#
public bool MatchesStandardStatsStructure(
    Item op1
)
```
Compares with another item and determines whether or not the standard stats
have a matching structure. An item's stat structure is matching, if it has
the same stats and they are all in the same order.
```C#
public bool HasDifferentStatValues(
    Item op1
)
```
Compares with another item and determines whether or not the stats have
the same value.




# ActionFactory interface
The **ActionFactory** interface specifies methods the bot can execute.

## Methods
```C#
IAction Finish(
    Item item
)
```
An action that will finish the bot maging session.
```C#
IAction CombineRune(
    Rune rune,
    bool exo
)
```
An action that will combine the specified rune with information regarding whether or
not the combination is exotic to the active item.
```C#
IAction InventorySelectResourcesAction()
```
An action that will select the in-game resources tab in the user's inventory.
```C#
IAction InventoryClearSelectionAction()
```
An action that will clear the in-game selection query in the user's inventory.




# MageConfigProvider interface
The **MageConfigProvider** interface specifies configuration options related to the maging
process, independant of the active item.

## Properties
```C#
bool RestoreHighSinkStatsImmediately {
    get;
}
```
A configuration detail provided by the ConfigManager which determines whether the AI
should prioritize the restoration of high sink stats immediately.




# StatConfigProvider interface
The **StatConfigProvider** interface specifies stat-specfic configuration options.

## Methods
```C#
StatConfig Config(
    Stat stat
)
```
Provide configuration for the specified stat.
```C#
Dictionary<Stat,StatConfig> Config()
```
Provide a mapping of all the stat configuration.




# DefaultMageConfigProvider class
The default implementation of the **MageConfigProvider** interface.
Provides subjectively reasonable configuration options.




# DefaultStatConfigProvider class
The default implementation of the **StatConfigProvider** interface.
Provides subjectively reasonable configuration options.





# DofusMagingAI class
The **DofusMagingAI** abstract class represents a maging AI instance.
The maging AI is used to determine what action to take next in regards
to the current item stats.

## Properties & Fields
```Action```\
Type: ActionFactory\
An action factory instance providing all possible actions the maging bot can execute.

```ConfigProvider```\
Type: StatConfigProvider\
A stat config provider instance providing the active stat configuration.

```resolving```\
Type: Item\
The current item being resolved. Used internally to provide helper methods without the need
to pass arguments.

## Abstract Methods
```C#
protected abstract IAction Resolve(
    Item item
)
```
Return the next action that should be taken for the specified item.

## Methods
```C#
public IAction Combine(
    Stat stat
)
```
A helper method used to provide a combine action for the specified stat.
```C#
public IAction Combine(
    Rune rune
)
```
A helper method used to provide a combine action for the specified rune.
```C#
public IAction Finish()
```
A helper method used to provide a finish action.
```C#
private Rune.RuneType ResolveRuneType(
    Stat stat
)
```
Resolve the rune type that should be used for the specified stat.