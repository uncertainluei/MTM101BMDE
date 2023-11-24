﻿/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTM101BaldAPI.Registers
{
    // flags for basic item behaviors so mods know how to handle them
    [Flags]
    public enum ItemFlags
    {
        Unknown = 0, // This item has no known flags. This usually only appears for built in items that have not had their metadata defined.
        MultipleUse = 1, // This item has multiple uses like the grappling hook.
        NoInventory = 2, // This item should not appear in the players inventory. This is useful for stuff like presents or non-items that use the Pickup system.
        NoUses = 4, // This item doesn't do anything when used, regardless of circumstance. This is for items like the Apple, but not the quarter as it can be used in machines.
    }

    public struct ItemMetaData
    {
        public ItemObject[] itemObjects; // for things like the grappling hook, the highest use count should be stored first.
        public ItemObject itemObject => itemObjects.First();
        public ItemFlags flags;
        public Items id;
    }
}
*/