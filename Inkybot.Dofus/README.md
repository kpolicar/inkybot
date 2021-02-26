#Getting Started
Inkybot is the first Dofus Maging bot. When using Inkybot, you are given 2 options.
1. use the built-in Inkybot maging AI
2. use a custom maging AI

If you choose the latter, you will need to familiarize yourself with this class library.

#Meet Inkybot's shared class library
The class library consists mostly of data classes representing Dofus data structures, such
as items, stats, runes, mage history records, etc.

There are additional data structures, that are unique to Inkybot and are used within
Inkybot's AI configuration system and the bot's maging process.
Some of these are: MageConfig, StatConfig, ActionFactory, etc.

#Important classes

## Dofus Maging AI
The **Dofus Maging AI** is an abstract class, which can be written
to replace the built-in Inkybot Maging AI.
You will need to extend this class and provide implementations to certain methods.

##Item
The **Item** class represents a single in-game item. It contains item stats with all their
constraint and statuses.
During the maging process a new item is created every tick. Equality of an item is thus
determined in the structure of the stats.

##Stat
The **Stat** class represents a single stat (vitality, agility, chance, etc.).
The class contains static accessors to all the possible in-game stats and can be accessed
in a natural fashion:
```C#
Stat.{name}
// eg. Stat.Vitality
```